# USkript Scripting Guide

## Introduction

USkript is a simple scripting language that allows you to modify Unturned server behavior without knowing C#.

## Syntax Basics

### Indentation

USkript uses indentation (like Python) to define code blocks. **Always use 4 spaces** per indentation level.

```python
event player_join(player):
    message player "Welcome!"  # 4 spaces indentation
    if money player >= 100:
        message player "You're rich!"  # 8 spaces (2 levels)
```

❌ **Wrong:**
```python
event player_join(player):
  message player "Welcome!"  # 2 spaces - error!
```

### Comments

Use `#` to add comments:

```python
# This is a comment
message player "Text"  # Comment at the end of line
```

### Events

An event is a code block executed when something happens. Always ends with a colon `:`.

```python
event event_name(parameters):
    actions
```

Example:
```python
event player_join(player):
    message player "Welcome!"
```

## Available Events

### player_join
Executed when a player joins the server.

```python
event player_join(player):
    message player "Welcome to the server!"
    add_money player 100
```

**Parameters:**
- `player` - the player who joined

### player_quit
Executed when a player leaves the server.

```python
event player_quit(player):
    broadcast "{player.name} left the server"
```

**Parameters:**
- `player` - the player who left

### player_chat
Executed when a player writes in chat.

```python
event player_chat(player, msg):
    if startswith msg "!help":
        message player "Available commands: !kit, !spawn, !bal"
        cancel
```

**Parameters:**
- `player` - the player who wrote
- `msg` - message content

**Important:** Use `cancel` to block the message from appearing in chat.

### player_death
Executed when a player dies.

```python
event player_death(player):
    message player "You died! Respawn in 10s"
```

**Parameters:**
- `player` - the player who died

### every (timers)
Executed cyclically at specified intervals.

```python
event every(5 minutes):
    broadcast "Remember to save!"

event every(30 seconds):
    broadcast "Short message"
```

**Time units:**
- `seconds` - seconds
- `minutes` - minutes
- `hours` - hours

## Actions

### Messages

#### message
Sends a private message to a player.

```python
message player "Text"
message player "&aColored &etext"
```

#### broadcast
Sends a global message to all players.

```python
broadcast "Restart in 5 minutes!"
broadcast "&c[WARNING] &fImportant message!"
```

### Items

#### give
Gives an item to a player.

```python
give player "ItemId" amount
```

Examples:
```python
give player "MapleStrike" 1
give player "MilitaryDrum" 5
give player "363" 1  # You can use numeric ID
```

**How to find item ID?**
- In Unturned: editor mode, Items tab
- Online list: [Unturned Bunker](https://unturnedbunker.com/items/)

### Teleportation

#### teleport
Teleports a player to a location.

```python
teleport player "warp_name"
teleport player "0,10,0"  # Coordinates x,y,z
```

### Economy

#### add_money
Adds money to a player.

```python
add_money player 100
add_money player 50.5  # You can use decimals
```

#### set_money
Sets player's money to a specific value.

```python
set_money player 1000
```

**Note:** Requires an economy plugin installed on the server (e.g., OpenMod.Economy).

### Health

#### set_health
Sets player health (0-100).

```python
set_health player 100  # Full health
set_health player 50   # Half health
```

#### kill
Kills a player.

```python
kill player
```

### Commands and Control

#### run_command
Executes a server command.

```python
run_command "airdrop"
run_command "give {player.name} 363 1"
```

#### cancel
Cancels an event (e.g., blocks a chat message).

```python
event player_chat(player, msg):
    if startswith msg "!":
        cancel  # Blocks message display
```

## Conditions

### Basic Syntax

```python
if condition:
    actions_when_true
else:
    actions_when_false
```

Example:
```python
if money player >= 100:
    message player "You have a lot of money!"
else:
    message player "You're poor!"
```

### Condition Types

#### equals
Checks if values are equal (case-insensitive).

```python
if equals msg "!spawn":
    teleport player "spawn"
```

#### startswith
Checks if text starts with a given string.

```python
if startswith msg "!kit":
    # Handle all commands starting with !kit
```

#### has_permission
Checks if a player has a permission.

```python
if has_permission player "vip.kit":
    give player "MilitaryDrum" 1
else:
    message player "No permission!"
```

#### Numeric Comparisons

Checks player's money or health.

```python
# Money
if money player >= 1000:
    message player "You're rich!"
    
if money player < 50:
    add_money player 50

# Health
if health player <= 30:
    message player "You have low health!"
    set_health player 100
```

**Operators:**
- `>=` - greater than or equal
- `<=` - less than or equal
- `>` - greater than
- `<` - less than
- `==` - equal

## Variables in Text

You can use variables in message text:

```python
message player "Welcome {player.name}!"
broadcast "{player.displayname} joined the game!"
message player "Your ID: {player.id}"
```

**Available variables:**
- `{player.name}` - player name
- `{player.id}` - player Steam ID
- `{player.displayname}` - player display name

## Colors

Use color codes (like in Minecraft) to colorize text:

```python
message player "&aGreen text"
message player "&eyellow &aand &cgreen"
broadcast "&c[ERROR] &fSomething went wrong!"
```

**Color codes:**

| Code | Color |
|------|-------|
| `&a` | Green |
| `&e` | Yellow |
| `&c` | Red |
| `&f` | White |
| `&0` | Black |
| `&1` | Dark Blue |
| `&2` | Dark Green |
| `&3` | Cyan |
| `&4` | Dark Red |
| `&5` | Purple |
| `&6` | Orange |
| `&7` | Gray |
| `&8` | Dark Gray |
| `&9` | Blue |
| `&b` | Light Blue |
| `&d` | Pink |

## Example Scripts

### Complete Kit System

```python
# kits.usk

event player_chat(player, msg):
    # Starter kit
    if equals msg "!kit starter":
        give player "Eaglefire" 1
        give player "MilitaryDrum" 3
        give player "Medkit" 2
        message player "&aYou received the starter kit!"
        cancel
    
    # VIP kit
    if equals msg "!kit vip":
        if has_permission player "kits.vip":
            give player "Grizzly" 1
            give player "GrizzlyMagazine" 5
            give player "Medkit" 5
            give player "Adrenaline" 2
            message player "&eYou received the VIP kit!"
        else:
            message player "&cYou don't have VIP permissions!"
        cancel
    
    # Kit list
    if equals msg "!kits":
        message player "&e=== Available Kits ==="
        message player "&f!kit starter &7- Starter kit (for everyone)"
        message player "&e!kit vip &7- VIP kit (requires VIP)"
        cancel
```

### Economy System

```python
# economy.usk

event player_join(player):
    add_money player 50
    message player "&aYou received &e$50 &ato start!"

event player_chat(player, msg):
    # Check balance
    if equals msg "!bal":
        message player "&eYour balance: &a${player.money}"
        cancel
    
    # Buy health
    if equals msg "!buy health":
        if money player >= 100:
            set_health player 100
            add_money player -100
            message player "&aBought full health for $100!"
        else:
            message player "&cNot enough money! (need $100)"
        cancel
```

### Auto-Announcements

```python
# announcements.usk

event every(5 minutes):
    broadcast "&e[Tip] &fUse &a!help &fto see commands!"

event every(10 minutes):
    broadcast "&e[Info] &fJoin our Discord: discord.gg/example"

event every(15 minutes):
    broadcast "&e[Tip] &fRemember to save regularly!"
```

### Welcome New Players

```python
# welcome.usk

event player_join(player):
    broadcast "&7[&a+&7] &f{player.name} &7joined the game!"
    message player "&e=== Welcome to the server! ==="
    message player "&fType &a!help &fto see commands"
    message player "&fJoin our Discord: &bdiscord.gg/example"
    add_money player 100

event player_quit(player):
    broadcast "&7[&c-&7] &f{player.name} &7left the game!"
```

## Best Practices

### 1. Organize scripts into thematic files
✅ **Good:**
- `kits.usk` - kit system
- `economy.usk` - economy
- `teleports.usk` - teleportation
- `announcements.usk` - announcements

❌ **Bad:**
- `script1.usk`, `script2.usk` - unclear names

### 2. Use cancel for chat commands
```python
event player_chat(player, msg):
    if startswith msg "!":
        # Handle command
        cancel  # Block display
```

### 3. Add comments
```python
# Kit system - version 1.0
# Author: YourName

event player_chat(player, msg):
    # Starter kit for everyone
    if equals msg "!kit starter":
        give player "Eaglefire" 1
```

### 4. Check permissions
```python
if has_permission player "admin.teleport":
    teleport player "{target}"
else:
    message player "&cNo permission!"
```

### 5. Use colors for readability
```python
# Good - different colors for different message types
message player "&aSuccess! &fOperation completed"  # Success - green
message player "&cError! &fSomething went wrong"   # Error - red
message player "&eInfo: &fInformation"              # Info - yellow
```

## Debugging

### Checking Errors
After `/usk reload` check server logs (`OpenMod/logs/`) for parsing errors.

### Common Errors

**Error: Invalid indentation**
```python
event player_join(player):
  message player "Welcome!"  # Not enough spaces!
```
**Solution:** Use 4 spaces.

**Error: Missing colon**
```python
event player_join(player)  # Missing :
    message player "Welcome!"
```
**Solution:** Add `:` at the end of the line.

**Error: Unknown action**
```python
send_message player "Welcome!"  # Wrong action name
```
**Solution:** Use `message` instead of `send_message`.

## What's Next?

- Read [README.md](README.md) for full documentation
- See examples in the `scripts/` folder
- Join the community on Discord
- Report bugs on GitHub Issues

---

**Happy scripting! 🚀**
