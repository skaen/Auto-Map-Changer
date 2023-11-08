using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace AutoMapChanger;

public class AutoMapChanger : BasePlugin
{
    public override string ModuleName => "Auto Map Changer";
    public override string ModuleVersion => "1.0.6"; 
    public override string ModuleAuthor => "skaen";

    private static Config _config = null!;
    private static Timer myTimer = null!;

    public override void Load(bool hotReload)
    {
        Log($"{ModuleName} [{ModuleVersion}] loaded success");

        LoadConfig();

        RegisterListener<Listeners.OnMapStart>(mapName => {
            Log($"[ {ModuleName} ] Map {mapName} has started!");
            StartTimer();
        });
        RegisterListener<Listeners.OnClientPutInServer>(playerSlot => {
            myTimer?.Kill();
        });
        RegisterListener<Listeners.OnClientDisconnectPost>(playerSlot => {
            if (Utilities.GetPlayers().Count == 0)
                StartTimer();
        });
    }
   
    public void StartTimer()
    {
        myTimer ??= AddTimer(_config.Delay, MapChange, TimerFlags.REPEAT);
    }

    private void MapChange()
    {
        var DefaultMap = _config.DefaultMap.IndexOf("ws:") != -1 ? _config.DefaultMap[3..] : _config.DefaultMap;

        if (NativeAPI.GetMapName() == DefaultMap) return;
        if (Utilities.GetPlayers().Count > 0) return;


        if (_config.DefaultMap.IndexOf("ws:") != -1)
            Server.ExecuteCommand($"ds_workshop_changelevel {DefaultMap}");
        else
            Server.ExecuteCommand($"map {DefaultMap}");

        Log($"[ {ModuleName} ] Change level on map \"{DefaultMap}\"");
    }

    [ConsoleCommand("css_acm_reload", "Reload config AutoChangeMap")]
    public void ReloadACMConfig(CCSPlayerController? controller, CommandInfo command)
    {
        if (controller != null) return;

        LoadConfig();
        myTimer?.Kill();
        StartTimer();

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
}
