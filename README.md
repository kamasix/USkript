# USkript - Scripting Language for Unturned

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Version](https://img.shields.io/badge/version-0.1.0--mvp-blue.svg)]()

**USkript** is a simple, intuitive scripting language designed specifically for Unturned servers based on OpenMod. It allows server owners to modify server behavior using simple text files (`.usk`) without needing to compile C# plugins.

## 🎯 Key Features

- ✅ **No compilation** - edit `.usk` files and reload with `/usk reload`
- ✅ **Simple language** - syntax inspired by Python and Minecraft Skript
- ✅ **Safe layer** - controlled access to server functions
- ✅ **Hot reload** - reload without server restart
- ✅ **Extensible** - ability to add custom actions and events

## 📦 Installation

### Requirements
- Unturned Server
- OpenMod installed on the server
- .NET Standard 2.1

### Installation Steps

1. Download the latest USkript release from [Releases](https://github.com/kamasix/USkript/releases)
2. Copy `USkript.dll` to the `OpenMod/Plugins/` folder
3. Start the server - the plugin will automatically create the `scripts/` folder
4. Done!

## 🚀 Quick Start

### First Script

Create a file `scripts/welcome.usk`:

```python
# welcome.usk - Welcome players on join

event player_join(player):
    message player "Welcome to the server, {player.name}!"
    heal player
    feed player
    give player "15" 5
    broadcast "Player {player.name} joined the game!"
```

### Complete Feature Showcase

See `scripts/showcase.usk` for a complete reference of **ALL** available features:
- All 7 event types (player_join, player_chat, player_damaged, etc.)
- All 25+ actions (heal, teleport, kick, give items, etc.)
- All 11+ conditions (health checks, permissions, etc.)
- All 12+ variables ({player.name}, {player.health}, etc.)
- Real-world examples and use cases

Reload scripts:
```
/uskript reload
```

## 📖 Language Documentation

USkript provides a simple Python-like syntax for server scripting.

### Quick Reference

**Events**: `player_join`, `player_disconnect`, `player_chat`, `player_death`, `player_damaged`, `player_spawned`, `every(seconds)`

**Actions**: `message`, `broadcast`, `give`, `teleport`, `heal`, `feed`, `kick`, `kill`, `add_experience`, `set_health`, `clear_inventory`, and more...

**Conditions**: `has_permission`, `health player >=`, `food player <`, `startswith msg`, `equals msg`, `is_in_vehicle player`, etc.

**Variables**: `{player.name}`, `{player.health}`, `{player.experience}`, `{player.position}`, `{damage}`, `{msg}`, etc.
- All 7 event types with examples
- All 25+ actions with syntax
- All 11+ conditions with operators
- All 12+ variables
- Color code reference
- Real-world complete examples

### Example - VIP Welcome System

```python
event player_join(player):
    if has_permission player "vip.rank":
        broadcast "&6&l[VIP] &f{player.name} &ejoined!"
        heal player
        feed player
        give player "15" 10
        add_experience player 500
        message player "&6VIP benefits applied!"
    else:
        broadcast "&7{player.name} joined"
        give player "15" 3
        add_experience player 100
```

### Example - Admin Commands

```python
event player_chat(player):
    if equals msg "/heal":
        if has_permission player "admin.commands":
            heal player
            feed player
            message player "&aYou have been healed!"
        else:
            message player "&cNo permission!"
        cancel
    
    if equals msg "/stats":
        message player "&a=== YOUR STATS ==="
        message player "&cHealth: &f{player.health}/100"
        message player "&eXP: &f{player.experience}"
        message player "&dRep: &f{player.reputation}"
        cancel
```
```python
message player "Message text"
broadcast "Global text"
```

#### Items
```python
give player "ItemId" amount
# Example: give player "MapleStrike" 1
```

#### Teleportation
## 🛠️ Commands

| Command | Alias | Description |
|---------|-------|-------------|
| `/uskript` | `/usk` | Main command - displays help |
| `/uskript reload` | `/usk reload` | Reloads all scripts |
| `/uskript info` | `/usk info` | Shows loaded scripts statistics |

## 🎨 Features

### Events (7 types)
- `player_join` - player connects
- `player_disconnect` - player leaves
- `player_chat` - player sends message
- `player_death` - player dies
- `player_damaged` - player takes damage
- `player_spawned` - player spawns/respawns
- `every(seconds)` - timer events (time in seconds: every(300) = 5 minutes)

### Actions (25+)
Messages: `message`, `broadcast`  
Items: `give`, `clear_inventory`  
Health: `heal`, `set_health`, `feed`, `set_food`, `set_water`, `set_stamina`, `set_virus`  
XP & Rep: `add_experience`, `set_experience`, `set_reputation`  
Admin: `kick`, `kill`, `teleport`, `exit_vehicle`, `run_command`  
Economy: `add_money`, `set_money`  
Control: `cancel`

### Conditions (11+)
String: `startswith msg`, `equals msg`  
Permission: `has_permission player`  
Stats: `health player >=`, `food player <`, `water player >`, `experience player ==`, `reputation player <=`  
Economy: `money player >=`  
Vehicle: `is_in_vehicle player`

### Variables (12+)
Player: `{player.name}`, `{player.id}`, `{player.displayname}`, `{player.health}`, `{player.food}`, `{player.water}`, `{player.experience}`, `{player.reputation}`, `{player.group}`, `{player.position}`, `{player.ping}`  
Event: `{msg}`, `{damage}`

## 🏗️ Architecture

USkript consists of three main layers:

### 1. Core (USkript.Core)
- **Models/** - AST (Node, EventNode, IfNode, ActionNode, ScriptFile)
- **Parsing/** - ScriptParser - parses `.usk` files to AST tree
- **Runtime/** - Execution engine:
  - `ISkriptPlayer` - player abstraction
  - `SkriptContext` - execution context
  - `USkriptInterpreter` - executes actions and conditions
  - `ScriptManager` - manages loaded scripts
  - `USkriptEngine` - main engine

### 2. OpenMod Adapter (USkript.OpenMod)
- **OpenModSkriptPlayer** - `ISkriptPlayer` implementation for OpenMod
- **OpenModEnvironmentAdapter** - OpenMod environment adapter
- OpenMod events mapping → USkript

### 3. Plugin (USkript.cs)
- Main OpenMod plugin
- Event subscriptions (`player_join`, `player_chat`, `player_death`)
- Integration with OpenMod DI

## 📊 Roadmap

### ✅ v0.1.0 - COMPLETED
- [x] `.usk` parser with Python-like syntax
- [x] 7 event types (join, disconnect, chat, death, damaged, spawned, timers)
- [x] 25+ actions (heal, teleport, kick, give, experience, etc.)
- [x] 11+ conditions (permissions, health checks, comparisons)
- [x] 12+ variables ({player.name}, {player.health}, etc.)
- [x] Timer system (every X seconds/minutes/hours)
- [x] `/uskript reload` command
- [x] Hot reload without server restart
- [x] Complete feature showcase in `scripts/showcase.usk`

### 🚧 v0.2.0 (planned)
- [ ] All players support in timer events
- [ ] Custom variables and storage
- [ ] Weather and time control
- [ ] Vehicle spawning actions
- [ ] Item conditions (has_item, etc.)
- [ ] Ban/unban actions

### 🔮 v0.3.0 (future)
- [ ] User-defined functions
- [ ] Lists and foreach loops
- [ ] File-based storage (JSON/YAML)
- [ ] Warp system integration
- [ ] NPC interaction events

### 🌟 v1.0+ (long-term)
- [ ] Debugging tools and traces
- [ ] Visual script editor
- [ ] RocketMod adapter
- [ ] Plugin API for custom actions

## 🤝 Contribution

The project is open to community contributions! If you want to help:

1. Fork the project
2. Create a branch for your feature (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

Project distributed under the MIT License. See the `LICENSE` file for details.

## 💬 Support

- **GitHub Issues**: [Report bugs or request features](https://github.com/kamasix/USkript/issues)
- **Discord**: [Join our community](https://discord.gg/7crjRskdyj)

## 🙏 Acknowledgments

- OpenMod Team - for the framework
- Skript (Minecraft) - for inspiration
- Unturned Community - for feedback

---

**Made with ❤️ for the Unturned community**
