# Changelog

All notable changes to USkript will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### In Progress (v0.2.0)
- Weather and time control actions
- Vehicle spawning
- Item condition checks
- Ban/unban actions

## [0.2.0] - 2025-11-23

### Added
- ✅ **All players support in timer events**
  - `GetAllPlayers()` method in `IEnvironmentAdapter`
  - Timer events can now iterate over all online players
  
- ✅ **Custom variables system**
  - `set var "name" "value"` - Store custom variables
  - `{var.name}` - Use variables in text/messages
  - Variables stored in `SkriptContext.Variables` dictionary
  - Support for storing any text or player data

- ✅ **Weather and time control**
  - `set_weather "rain/clear/storm/drizzle"` - Control server weather
  - `set_time "day/night/dawn/dusk"` - Set time of day
  - `set_time_cycle "true/false"` - Enable/disable day-night cycle

- ✅ **Vehicle spawning**
  - `spawn_vehicle player "VehicleID"` - Spawn vehicles near players
  - Spawns 5 meters in front of player
  - Supports all Unturned vehicle IDs

- ✅ **Item conditions**
  - `has_item player "ItemID" amount` - Check inventory for items
  - Counts items across all inventory pages
  - Supports both item IDs and amounts

- ✅ **Ban/unban system**
  - `ban player "reason" duration` - Ban players with reason and duration (seconds)
  - `unban "SteamID"` - Unban players by Steam ID
  - `is_banned "SteamID"` - Check if Steam ID is banned

## [0.1.0-mvp] - 2025-01-XX

### Added
- ✅ Core scripting engine (USkript.Core)
  - AST-based parser for `.usk` files
  - Indentation-based syntax (Python-style)
  - Event system
  - Action execution
  - Condition evaluation
  - Runtime interpreter

- ✅ OpenMod integration
  - Plugin for OpenMod Unturned
  - Player adapter (`OpenModSkriptPlayer`)
  - Environment adapter (`OpenModEnvironmentAdapter`)
  - Event mapping (OpenMod → USkript)

- ✅ Events
  - `player_join(player)` - When player connects
  - `player_quit(player)` - When player disconnects
  - `player_death(player)` - When player dies
  - `player_chat(player, msg)` - When player sends chat message
  - `every(X minutes/seconds/hours)` - Timed events (basic support)

- ✅ Actions
  - `message player "text"` - Send private message
  - `broadcast "text"` - Send global message
  - `give player "ItemId" amount` - Give item to player
  - `teleport player "location"` - Teleport player
  - `add_money player amount` - Add money (placeholder)
  - `set_money player amount` - Set money (placeholder)
  - `set_health player amount` - Set player health
  - `kill player` - Kill player
  - `run_command "command"` - Execute server command (placeholder)
  - `cancel` - Cancel event (e.g., block chat message)

- ✅ Conditions
  - `if equals msg "text"` - String equality check
  - `if startswith msg "text"` - String prefix check
  - `if has_permission player "perm"` - Permission check (placeholder)
  - `if money player >= amount` - Money comparison (placeholder)
  - `if health player >= amount` - Health comparison
  - `else` - Else block

- ✅ Features
  - Color codes support (`&a`, `&e`, `&c`, etc.)
  - Variable substitution in text (`{player.name}`, `{player.id}`)
  - Comment support (`#`)
  - Nested if/else blocks
  - Hot reload via `/usk reload`

- ✅ Commands
  - `/uskript` (alias: `/usk`) - Main command
  - `/usk reload` - Reload all scripts
  - `/usk info` - Show loaded scripts statistics

- ✅ Documentation
  - README.md - Project overview and quick start
  - GUIDE.md - Complete scripting guide with examples
  - ARCHITECTURE.md - Technical architecture documentation
  - Example scripts in `scripts/` folder

### Technical Details
- Platform: .NET Standard 2.1
- Dependencies: OpenMod.Unturned 3.8.10+
- Architecture: Multi-layer (Core → Adapter → Plugin)
- Parser: Recursive descent, indentation-based
- Interpreter: AST walker with async execution

### Known Limitations (MVP)
- ⚠️ Economy actions require plugin integration (not implemented)
- ⚠️ Permission checking requires integration (not implemented)
- ⚠️ Timer events (`every`) need full implementation
- ⚠️ Teleport only supports coordinates (no warp system)
- ⚠️ No variable system yet
- ⚠️ No function definitions
- ⚠️ No loops

### Files Structure
```
USkript/
├── Core/
│   ├── Models/         - AST node definitions
│   ├── Parsing/        - Script parser
│   └── Runtime/        - Execution engine
├── OpenMod/            - OpenMod adapter layer
├── Commands/           - Chat commands
├── scripts/            - Example scripts
│   ├── join.usk
│   ├── chat.usk
│   ├── timers.usk
│   └── example_full.usk
├── USkript.cs          - Main plugin
├── README.md           - Documentation
├── GUIDE.md            - Scripting guide
└── ARCHITECTURE.md     - Technical docs
```

## [0.0.1] - 2025-01-XX (Initial Commit)

### Added
- Initial project setup
- OpenMod plugin template
- Basic project structure

---

## Version Naming Convention

- **MVP (0.1.x)** - Minimum Viable Product
  - Basic events, actions, conditions
  - Hot reload
  - OpenMod integration

- **V1 (0.2.x)** - First Stable Release
  - Full economy integration
  - Permission system
  - Timer events
  - More conditions and actions

- **V2 (0.3.x)** - Variables & Storage
  - Variable system
  - Persistent storage (JSON/YAML)
  - Addon system
  - RocketMod support

- **V3+ (0.4.x+)** - Advanced Features
  - Function definitions
  - Loops and lists
  - Debugging tools
  - GUI editor

---

**Legend:**
- ✅ Implemented
- ⚠️ Partially implemented / Has limitations
- ❌ Not implemented yet
- 🚧 Work in progress
