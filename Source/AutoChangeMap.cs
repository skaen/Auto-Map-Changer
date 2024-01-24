using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
namespace AutoMapChanger;

public class AutoMapChanger : BasePlugin
{
    public override string ModuleName => "Auto Map Changer";
    public override string ModuleVersion => "1.0.8"; 
    public override string ModuleAuthor => "skaen";

    private static Config _config = null!;

    public override void Load(bool hotReload)
    {
        Log($"{ModuleName} [{ModuleVersion}] loaded success");

        LoadConfig();

        RegisterListener<Listeners.OnMapStart>(mapName => {
            if (_config.Debug)
                Logger.LogInformation($"[ {ModuleName} ] Map {mapName} has started!");

            if (mapName != _config.DefaultMap && mapName != _config.DefaultMap[3..])
            {
                if (_config.Debug)
                    Logger.LogInformation($"[ {ModuleName} ] Map {mapName} has start timer!");

                AddTimer(_config.Delay, MapChange, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
            }
        });
    }

    public void MapChange()
    {
        var players = Utilities.GetPlayers().Count(p => p.IsBot == false);

        if (_config.Debug)
            Logger.LogInformation($"[ {ModuleName} ] Players count: {players}");

        if (players < 1)
        {
            if (_config.DefaultMap.IndexOf("ws:") != -1)
                Server.ExecuteCommand($"ds_workshop_changelevel {_config.DefaultMap[3..]}");
            else
                Server.ExecuteCommand($"map {_config!.DefaultMap}");

            if(_config.Debug)
                Logger.LogInformation($"[ {ModuleName} ] Change level on map \"{_config.DefaultMap}\"");
        }        
    }

    [ConsoleCommand("css_acm_reload", "Reload config AutoChangeMap")]
    public void ReloadACMConfig(CCSPlayerController? controller, CommandInfo command)
    {
        if (controller != null) return;

        LoadConfig();
        Log($"[ {ModuleName} ] loaded config success");
    }

    private void LoadConfig()
    {
        var configPath = Path.Combine(ModuleDirectory, "autochangemap.json");

        if (!File.Exists(configPath)) 
            CreateConfig(configPath);
        else
            _config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;
    }

    
    private void CreateConfig(string configPath)
    {
        _config = new Config
        {
            Delay = 180.0f,
            DefaultMap = "de_dust2",
            Debug = false
        };

        File.WriteAllText(configPath,
            JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true }));

        Log($"[ {ModuleName} ] The configuration was successfully saved to a file: " + configPath);
    }

    public void Log(string message)
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}

public class Config
{
    public float Delay { get; set; }
    public string DefaultMap { get; set; } = null!;
    public bool Debug { get; set; }
}
