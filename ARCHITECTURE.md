# USkript Architecture

## Overview

USkript is a scripting language interpreter built in a multi-layered architecture, providing separation of logic from the platform (OpenMod/RocketMod).

```
┌─────────────────────────────────────────┐
│         USkript Plugin Layer            │
│  (USkript.cs - OpenMod Integration)     │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      OpenMod Adapter Layer              │
│  - OpenModSkriptPlayer                  │
│  - OpenModEnvironmentAdapter            │
│  - Commands (USkript, Reload, Info)     │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         USkript.Core                    │
│  (Platform-independent logic)           │
│  - Models (AST)                         │
│  - Parser                               │
│  - Runtime (Interpreter, Engine)        │
└─────────────────────────────────────────┘
```

## Layers

### 1. USkript.Core - Core

Platform-independent library containing all parsing and execution logic.

#### Models/ - Abstract Syntax Tree
- **Node.cs** - Base class for all AST nodes
- **EventNode.cs** - Represents an event (e.g., `player_join`)
- **IfNode.cs** - Represents an if/else condition
- **ActionNode.cs** - Represents an action (e.g., `message`, `give`)
- **ScriptFile.cs** - Represents a parsed `.usk` file

#### Parsing/
- **ScriptParser.cs** - `.usk` file parser
  - Handles indentation (Python-style)
  - Parses events, conditions, actions
  - Returns `ScriptFile` with AST tree

#### Runtime/
- **ISkriptPlayer.cs** - Player abstraction interface
  - Methods: `SendMessage`, `GiveItem`, `Teleport`, `AddMoney`, etc.
  - Implemented by platform adapters

- **IEnvironmentAdapter.cs** - Environment abstraction interface
  - Methods: `Broadcast`, `RunCommand`, `Log`, `FindPlayer`
  - Implemented by platform adapters

- **SkriptContext.cs** - Event execution context
  - Stores `Player`, `Message`, `Cancelled`, etc.
  - Passed between actions

- **USkriptInterpreter.cs** - Execution engine
  - `ExecuteEvent()` - executes an event
  - `ExecuteBlock()` - executes a block of statements
  - `ExecuteAction()` - executes a single action
  - `ExecuteIf()` - executes a condition
  - `EvaluateCondition()` - evaluates a conditional expression

- **ScriptManager.cs** - Script management
  - `LoadScripts(path)` - loads all `.usk` from folder
  - `GetEvents(name)` - returns all handlers for a given event
  - Indexes events by name for fast access

- **USkriptEngine.cs** - Main engine
  - Combines `ScriptManager` and `USkriptInterpreter`
  - `Reload(path)` - reloads scripts
  - `RaiseEvent(name, context)` - invokes all event handlers

### 2. OpenMod Adapter

OpenMod integration layer - maps Core abstractions to concrete OpenMod API.

#### OpenModSkriptPlayer.cs
Implements `ISkriptPlayer` for OpenMod/Unturned:
- Wraps `UnturnedPlayer` / `UnturnedUser`
- Implements all player actions using SDG.Unturned API
- Formats Minecraft-style colors to Unity Rich Text

#### OpenModEnvironmentAdapter.cs
Implements `IEnvironmentAdapter` for OpenMod:
- Broadcast through `ChatManager`
- Logging through `ILogger`
- Finding players through `IUserManager`

#### Commands/
- **CommandUSkript.cs** - Main command `/usk`
- **CommandUSkriptReload.cs** - `/usk reload` - reload scripts
- **CommandUSkriptInfo.cs** - `/usk info` - statistics

### 3. Plugin Layer (USkript.cs)

Main OpenMod plugin:
- Initializes `USkriptEngine` with `OpenModEnvironmentAdapter`
- Subscribes to OpenMod events through `IEventBus`:
  - `UnturnedPlayerConnectedEvent` → `player_join`
  - `UnturnedPlayerDeathEvent` → `player_death`
  - `UnturnedPlayerChattingEvent` → `player_chat`
- Maps events to `SkriptContext` and calls `engine.RaiseEvent()`
- Handles `cancel` for events (e.g., blocking chat)

## Execution Flow

### Plugin Startup

```
1. OpenMod loads USkript.dll
2. USkript.OnLoadAsync():
   - Creates OpenModEnvironmentAdapter
   - Creates USkriptEngine(adapter)
   - Calls engine.Reload(scriptsDirectory)
   - Subscribes to OpenMod events
3. ScriptManager.LoadScripts():
   - Finds all *.usk
   - For each: ScriptParser.ParseFile()
   - Indexes events in Dictionary<string, List<EventNode>>
```

### Event Invocation (e.g., player joins)

```
1. OpenMod calls UnturnedPlayerConnectedEvent
2. USkript.OnPlayerConnected():
   - Creates SkriptContext { Player = new OpenModSkriptPlayer(user) }
   - Calls engine.RaiseEvent("player_join", context)
3. USkriptEngine.RaiseEvent():
   - Gets all EventNode for "player_join"
   - For each: interpreter.ExecuteEvent(eventNode, context)
4. USkriptInterpreter.ExecuteEvent():
   - Calls ExecuteBlock(eventNode.Body, context)
5. ExecuteBlock():
   - For each Node in body:
     - If ActionNode → ExecuteAction()
     - If IfNode → ExecuteIf()
6. ExecuteAction():
   - Parses raw string action (regex)
   - Calls appropriate methods on context.Player or context.Environment
```

### Example: `message player "Welcome {player.name}!"`

```
1. ExecuteAction() recognizes "message" action
2. Regex: message\s+(\w+)\s+"(.+?)"
3. Extracts: target="player", text="Welcome {player.name}!"
4. ProcessString(text, context):
   - Replaces {player.name} → context.Player.Name
5. context.Player.SendMessage("Welcome John!")
6. OpenModSkriptPlayer.SendMessage():
   - FormatColors() - converts &a → <color=green>
   - ChatManager.serverSendMessage(...)
```

## Extensibility Points

### Adding New Actions

In `USkriptInterpreter.ExecuteAction()`:
```csharp
if (raw.StartsWith("new_action "))
{
    var match = Regex.Match(raw, @"^new_action\s+(.+)$");
    if (match.Success)
    {
        // Implementation
    }
    return;
}
```

### Adding New Conditions

In `USkriptInterpreter.EvaluateCondition()`:
```csharp
var match = Regex.Match(raw, @"^new_condition\s+(.+)$");
if (match.Success)
{
    // Return true/false
}
```

### Adding New Events

1. Subscribe to event in `USkript.OnLoadAsync()`:
```csharp
m_EventBus.Subscribe<NewEvent>(this, OnNewEvent);
```

2. Create handler:
```csharp
private Task OnNewEvent(IServiceProvider sp, object? sender, NewEvent @event)
{
    var ctx = new SkriptContext { /* ... */ };
    return m_Engine.RaiseEvent("new_event", ctx);
}
```

3. Users can now write:
```python
event new_event(player):
    message player "It works!"
```

### Addon System (Future)

Planned interface:
```csharp
public interface IUSkriptAddon
{
    void RegisterActions(ActionRegistry registry);
    void RegisterConditions(ConditionRegistry registry);
    void RegisterEvents(EventRegistry registry);
}
```

Addons will be able to register custom actions/conditions without modifying Core.

## Parsing Strategy

### Indentation-based

Parser uses indentation to determine blocks (like Python):
- 1 indentation level = 4 spaces
- Tab = 4 spaces
- Nested blocks = more indentation

```python
event player_join(player):          # Level 0
    if money player >= 100:         # Level 1 (4 spaces)
        message player "Rich!"      # Level 2 (8 spaces)
    else:                           # Level 1
        message player "Poor!"      # Level 2
```

### Preprocessing

`ScriptParser.PreprocessLines()`:
1. Removes comments (`#`)
2. Trim trailing whitespace
3. Skips empty lines
4. Calculates `IndentLevel` for each line

### Recursive Descent

Parser works recursively:
- `ParseFile()` → top-level (events)
- `ParseEvent()` → event block
- `ParseBlock()` → list of nodes (actions/ifs)
- `ParseIf()` → then/else bodies

## Performance Considerations

### Parsing
- Parsing occurs **only during reload** (not on every event)
- Parsed scripts to AST are cached in memory
- O(n) parsing complexity relative to number of lines

### Runtime
- Dictionary lookup for events: O(1)
- Regex matching for actions/conditions: optimized by .NET
- No reflection - all actions are hardcoded switch

### Memory
- AST in memory (few KB per script)
- No garbage during event execution (reused SkriptContext)

## Thread Safety

- **ScriptManager**: Thread-safe reload (lock during loading)
- **USkriptEngine**: Asynchronous event execution (async/await)
- **OpenMod integration**: Events can be called from different threads

## Error Handling

### Parse-time Errors
- Invalid indentation → logged in `ScriptParser`
- Event syntax error → event skipped
- Missing files → default `scripts/` folder created

### Runtime Errors
- Unknown action → log warning, continue
- Unknown condition → log warning, returns `false`
- Exception in action → log error, continue next action
- Exception in event → log error, doesn't interrupt other events

## Testing Strategy (Future)

### Unit Tests
- `ScriptParser` - parsing various constructs
- `USkriptInterpreter` - executing actions/conditions
- Mock `ISkriptPlayer` and `IEnvironmentAdapter`

### Integration Tests
- Full flow: parse → execute
- Mock OpenMod events
- Verify side-effects (messages, items)

### End-to-End Tests
- Tests on Unturned test server
- Verification of real events

## Future Enhancements

### Variables (V2)
```python
set var "kills.{player.id}" to 0
add 1 to var "kills.{player.id}"
if var "kills.{player.id}" >= 10:
    message player "10 kills!"
```

Implementation:
- `SkriptContext.Variables` (per-event)
- Global storage (JSON/YAML file)
- Actions: `set var`, `add to var`, `get var`

### Functions (V3)
```python
function give_starter_kit(player):
    give player "Eaglefire" 1
    give player "MilitaryDrum" 3

event player_join(player):
    call give_starter_kit player
```

Implementation:
- New `FunctionNode` in AST
- `ScriptManager.Functions` dictionary
- `ExecuteFunction()` in interpreter

### Loops (V3)
```python
loop 5 times:
    broadcast "Countdown..."
    wait 1 second
```

Implementation:
- New `LoopNode`
- `await Task.Delay()` for wait

## RocketMod Adapter (V2)

Planned structure:
```
USkript.RocketMod/
  - RocketSkriptPlayer.cs (ISkriptPlayer)
  - RocketEnvironmentAdapter.cs (IEnvironmentAdapter)
  - RocketPlugin.cs (main plugin)
```

Uses the same `USkript.Core`, just different adapter implementations.

## Conclusion

USkript architecture provides:
- ✅ Separation of logic from platform
- ✅ Easy extensibility
- ✅ Readable code
- ✅ Good performance
- ✅ Future-proof (RocketMod, other platforms)

---

**For developers:** See also [README.md](README.md) and [GUIDE.md](GUIDE.md)
