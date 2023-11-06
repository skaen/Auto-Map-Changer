using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace AutoMapChanger;

public class AutoMapChanger : BasePlugin
{
    public override string ModuleName => "Auto Map Changer";
    public override string ModuleVersion => "1.0.3"; 
    public override string ModuleAuthor => "skaen";

    private static Config _config = null!;
    private static Timer myTimer = null!;

    public override void Load(bool hotReload)
    {
        _config = LoadConfig();
        Log($"{ModuleName} [{ModuleVersion}] loaded success");
        SetupListener();
    }

    public void SetupListener()
    {

        RegisterListener<Listeners.OnMapStart>(mapName => {
            Log($"[ {ModuleName} ] Map {mapName} has started!");
            StartTimer();
        });
        RegisterListener<Listeners.OnClientPutInServer>(playerSlot => {
            if(myTimer != null)
                myTimer.Kill();
        });
        RegisterListener<Listeners.OnClientDisconnectPost>(playerSlot => {
            var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
            if (playerEntities.Count<CCSPlayerController>() == 0)
            {
                StartTimer();
            }
        });
    }
   
    public void StartTimer()
    {
        myTimer = AddTimer(_config.Delay, MapChange, TimerFlags.REPEAT);
    }

    private void MapChange()
    {
        if (NativeAPI.GetMapName() == _config.DefaultMap) return;

        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
        if (playerEntities.Count<CCSPlayerController>() > 0) return;

        Server.ExecuteCommand($"changelevel \"{_config.DefaultMap}\"");
        Log($"[ {ModuleName} ] Change level on map \"{_config.DefaultMap}\"");
    }

    [ConsoleCommand("css_acm_reload", "Reload config AutoChangeMap")]
    public void ReloadAdvertConfig(CCSPlayerController? controller, CommandInfo command)
    {
        if (controller != null)
        {
            return;
        }

        _config = LoadConfig();

        if (myTimer != null)
            myTimer.Kill();

        StartTimer();
    }

    private Config LoadConfig()
    {
        var configPath = Path.Combine(ModuleDirectory, "autochangemap.json");

        if (!File.Exists(configPath)) return CreateConfig(configPath);

        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

        return config;
    }

    private Config CreateConfig(string configPath)
    {
        var config = new Config
        {
            Delay = 180.0f,
            DefaultMap = "de_dust2",
        };

        File.WriteAllText(configPath,
            JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

        Log($"[ {ModuleName} ] The configuration was successfully saved to a file: " + configPath);

        return config;
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