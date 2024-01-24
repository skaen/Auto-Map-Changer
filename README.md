# Auto Map Changer
 Changes the map to default when not active

# Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
3. Download [Auto-Map-Changer](https://github.com/skaen/Auto-Map-Changer/releases)
4. Unzip the archive and upload it to the game server

# Config
The config is created automatically in the same place where the dll is located
```
{
  "Delay": 180,
  "DefaultMap": "de_dust2" // 
  "Debug": false // Output of checks to the log file
}
```
> [!NOTE]
> For Workshop maps needs prefix `ws:`

# Commands
`css_acm_reload` - Reload config AutoChangeMap
