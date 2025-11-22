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
    message player "&aWelcome to the server, &e{player.name}&a!"
    add_money player 100
    broadcast "&7Player &f{player.name} &7joined the game!"
```

Reload scripts:
```
/usk reload
```

### Chat Commands

Create a file `scripts/commands.usk`:

```python
# commands.usk - Simple chat commands

event player_chat(player, msg):
    if equals msg "!heal":
        set_health player 100
        message player "&aYour health has been restored!"
        cancel
    
    if startswith msg "!kit":
        if has_permission player "vip.kit":
            give player "MilitaryDrum" 1
            give player "Colt" 1
            message player "&aYou received the VIP kit!"
        else:
            message player "&cYou don't have permission!"
        cancel
```

## 📖 Language Documentation

### Events (MVP)

Events are places where you can run your code:

```python
event player_join(player):
    # Executed when a player joins the server
    
event player_quit(player):
    # Executed when a player leaves the server
    
event player_chat(player, msg):
    # Executed when a player writes in chat
    
event player_death(player):
    # Executed when a player dies
    
event every(5 minutes):
    # Executed every 5 minutes (supports: seconds, minutes, hours)
```

### Actions (MVP)

Actions are things you can do in scripts:

#### Messages
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
```python
teleport player "spawn"
# Or coordinates: teleport player "0,10,0"
```

#### Economy
```python
add_money player 100
set_money player 1000
```

#### Health
```python
set_health player 100
kill player
```

#### Commands and control
```python
run_command "airdrop"
cancel  # Cancels the event (e.g., blocks a chat message)
```

### Conditions

```python
# Text checking
if equals msg "!spawn":
    # code
    
if startswith msg "!kit":
    # code

# Permission checking
if has_permission player "vip.access":
    # code

# Money checking
if money player >= 1000:
    # code

# Health checking
if health player < 50:
    # code
    
# Else
if money player >= 100:
    message player "You have a lot of money!"
else:
    message player "You're poor!"
```

### Variables in text

```python
message player "Welcome {player.name}!"
message player "Your ID: {player.id}"
broadcast "{player.displayname} joined!"
```

### Colors

Use Minecraft color codes:

```python
message player "&aGreen &eyellow &cred &fwhite"
```

Color codes:
- `&a` - green
- `&e` - yellow
- `&c` - red
- `&f` - white
- `&0` - black
- `&1` - blue
- `&2` - dark green
- etc.

## 📁 Examples

### Economy System
```python
# economy.usk

event player_join(player):
    add_money player 50
    
event player_chat(player, msg):
    if equals msg "!bal":
        message player "&eAccount balance: &a{player.money}"
        cancel
```

### Auto-Announce
```python
# announcements.usk

event every(10 minutes):
    broadcast "&e[Info] &fJoin our Discord!"
    
event every(30 minutes):
    broadcast "&e[Info] &fRemember to save your progress!"
```

### Kit System
```python
# kits.usk

event player_chat(player, msg):
    if equals msg "!kit starter":
        give player "Eaglefire" 1
        give player "MilitaryDrum" 3
        give player "Medkit" 5
        message player "&aYou received the starter kit!"
        cancel
```

## 🛠️ Commands

| Command | Alias | Description |
|---------|-------|-------------|
| `/uskript` | `/usk` | Main command - displays help |
| `/usk reload` | `/usk rl` | Reloads all scripts |
| `/usk info` | - | Shows loaded scripts statistics |

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

### ✅ MVP (v0.1.0) - COMPLETED
- [x] `.usk` parser
- [x] Basic events (join, chat, death)
- [x] Basic actions (message, broadcast, give, teleport)
- [x] Conditions (if/else)
- [x] `/usk reload` command

### 🚧 V1 (planned)
- [ ] `player_first_join` event
- [ ] `every(X minutes)` timers
- [ ] Economy actions (integration with OpenMod.Economy)
- [ ] Permission actions (has_permission)
- [ ] More conditions (>, <, >=, <=, ==)

### 🔮 V2 (future)
- [ ] Variables (`set var`, `get var`)
- [ ] Simple storage (JSON/YAML)
- [ ] Addon system (register custom actions)
- [ ] RocketMod adapter

### 🌟 V3+ (long-term plans)
- [ ] User functions
- [ ] Lists and loops
- [ ] Debugging/trace
- [ ] GUI editor

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
