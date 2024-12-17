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
    public override string ModuleVersion => "1.1.0";
    public override string ModuleAuthor => "skaen";

    private static Config _config = null!;
    private static Timer myTimer = null!;

    public override void Load(bool hotReload)
    {
        Log($"{ModuleName} [{ModuleVersion}] Plugin has been successfully loaded");

        LoadConfig();

        RegisterListener<Listeners.OnMapStart>(mapName => {
            if (_config.Debug)
                Log($"[ {ModuleName} ] Map {mapName} has started!");
            StartTimer();
        });
        RegisterEventHandler<EventGameEnd>((@event, info) =>
        {
            myTimer?.Kill();
            return HookResult.Continue;
        });
    }

    public void StartTimer()
    {
        myTimer ??= AddTimer(_config.Delay, MapChange, TimerFlags.REPEAT);
    }

    private void MapChange()
    {
        var DefaultMap = _config.DefaultMap.StartsWith("ws:") ? _config.DefaultMap[3..] : _config.DefaultMap;
        var Players = Utilities.GetPlayers().Where(controller => controller is { IsValid: true, IsBot: false, IsHLTV: false });

        if (Players.Any() && !_config.ChangeMap)
        {
            if (_config.Debug)
                Log($"[ {ModuleName} ] Map change skipped: players ({Players.Count()}) are online and ChangeMap is false");
            return;
        }

        if (NativeAPI.GetMapName() == DefaultMap && !_config.ChangeMap)
        {
            if (_config.Debug)
                Log($"[ {ModuleName} ] Map change skipped: current map is already \"{DefaultMap}\" and ChangeMap is false");
            return;
        }

        if (_config.DefaultMap.StartsWith("ws:"))
            Server.ExecuteCommand($"ds_workshop_changelevel {DefaultMap}");
        else
            Server.ExecuteCommand($"map {DefaultMap}");

        if (_config.Debug)
            Log($"[ {ModuleName} ] Map change to \"{DefaultMap}\"");
    }

    [ConsoleCommand("css_acm_reload", "Reload config AutoChangeMap")]
    public void ReloadACMConfig(CCSPlayerController? controller, CommandInfo command)
    {
        if (controller != null) return;

        LoadConfig();
        myTimer?.Kill();
        StartTimer();

        Log($"[ {ModuleName} ] Loaded config success (map: {_config.DefaultMap}, delay: {_config.Delay})");
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
            ChangeMap = false,
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
    public float Delay { get; set; } = 180.0f;
    public string DefaultMap { get; set; } = "de_dust2";
    public bool ChangeMap { get; set; } = false;
    public bool Debug { get; set; }
}