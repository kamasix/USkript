using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Unturned.Users;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Players.Connections.Events;
using OpenMod.Unturned.Players.Life.Events;
using OpenMod.Unturned.Players.Chat.Events;
using OpenMod.Unturned.Players.Equipment.Events;
using OpenMod.Unturned.Players.Inventory.Events;
using OpenMod.Unturned.Players.Movement.Events;
using USkript.Core.Runtime;
using USkript.OpenMod;
using UnityEngine;

[assembly: PluginMetadata("USkript", DisplayName = "USkript", Author = "kamasix", Website = "https://github.com/kamasix/USkript")]

namespace USkript
{
    public class USkript : OpenModUnturnedPlugin
    {
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly ILogger<USkript> m_Logger;
        private readonly IUserManager m_UserManager;
        private readonly IEventBus m_EventBus;

        private USkriptEngine? m_Engine;
        private OpenModEnvironmentAdapter? m_EnvironmentAdapter;
        private string m_ScriptsDirectory = string.Empty;
        private readonly Dictionary<string, TimerData> m_Timers = new Dictionary<string, TimerData>();
        private bool m_TimerRunning = false;

        private class TimerData
        {
            public string EventName { get; set; } = string.Empty;
            public string EventData { get; set; } = string.Empty;
            public float IntervalSeconds { get; set; }
            public DateTime NextTrigger { get; set; }
        }

        public USkript(
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            ILogger<USkript> logger,
            IUserManager userManager,
            IEventBus eventBus,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
            m_Logger = logger;
            m_UserManager = userManager;
            m_EventBus = eventBus;
        }

        protected override async UniTask OnLoadAsync()
        {
            m_Logger.LogInformation("=== USkript Engine Loading ===");

            // Set scripts folder
            m_ScriptsDirectory = Path.Combine(WorkingDirectory, "scripts");

            // Create example script if needed
            CreateExampleScriptIfNeeded();

            // Initialize adapter and engine
            m_EnvironmentAdapter = new OpenModEnvironmentAdapter(m_Logger, m_UserManager);
            m_Engine = new USkriptEngine(m_EnvironmentAdapter);

            // Load scripts
            try
            {
                m_Engine.Reload(m_ScriptsDirectory);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Failed to load USkript scripts on startup");
            }

            // Subscribe to OpenMod events - Player
            m_EventBus.Subscribe<UnturnedPlayerConnectedEvent>(this, OnPlayerConnected);
            m_EventBus.Subscribe<UnturnedPlayerDeathEvent>(this, OnPlayerDeath);
            m_EventBus.Subscribe<UnturnedPlayerChattingEvent>(this, OnPlayerChatting);
            m_EventBus.Subscribe<UnturnedPlayerDisconnectedEvent>(this, OnPlayerDisconnected);
            m_EventBus.Subscribe<UnturnedPlayerDamagedEvent>(this, OnPlayerDamaged);
            m_EventBus.Subscribe<UnturnedPlayerSpawnedEvent>(this, OnPlayerSpawned);

            // Setup timers for "every" events
            SetupTimers();
            StartTimerLoop();

            m_Logger.LogInformation("=== USkript Engine Loaded Successfully ===");
            await UniTask.CompletedTask;
        }

        protected override async UniTask OnUnloadAsync()
        {
            m_Logger.LogInformation("USkript Engine unloading...");
            
            // Stop timer loop
            m_TimerRunning = false;
            
            // Events are automatically unsubscribed by OpenMod on unload

            await UniTask.CompletedTask;
        }

        // Event: player_join
        private Task OnPlayerConnected(IServiceProvider serviceProvider, object? sender, UnturnedPlayerConnectedEvent @event)
        {
            if (m_Engine == null) return Task.CompletedTask;

            var context = new SkriptContext
            {
                Player = new OpenModSkriptPlayer(@event.Player)
            };

            return m_Engine.RaiseEvent("player_join", context);
        }

        // Event: player_death
        private Task OnPlayerDeath(IServiceProvider serviceProvider, object? sender, UnturnedPlayerDeathEvent @event)
        {
            if (m_Engine == null) return Task.CompletedTask;

            var context = new SkriptContext
            {
                Player = new OpenModSkriptPlayer(@event.Player)
            };

            return m_Engine.RaiseEvent("player_death", context);
        }

        // Event: player_chat
        private Task OnPlayerChatting(IServiceProvider serviceProvider, object? sender, UnturnedPlayerChattingEvent @event)
        {
            if (m_Engine == null) return Task.CompletedTask;

            var context = new SkriptContext
            {
                Player = new OpenModSkriptPlayer(@event.Player),
                Message = @event.Message
            };

            var task = m_Engine.RaiseEvent("player_chat", context);
            task.Wait(); // Wait for execution

            // If script cancelled chat
            if (context.Cancelled)
            {
                @event.IsCancelled = true;
            }

            return task;
        }

        // Event: player_disconnect
        private Task OnPlayerDisconnected(IServiceProvider serviceProvider, object? sender, UnturnedPlayerDisconnectedEvent @event)
        {
            if (m_Engine == null) return Task.CompletedTask;

            var context = new SkriptContext
            {
                Player = new OpenModSkriptPlayer(@event.Player)
            };

            return m_Engine.RaiseEvent("player_disconnect", context);
        }

        // Event: player_damaged
        private Task OnPlayerDamaged(IServiceProvider serviceProvider, object? sender, UnturnedPlayerDamagedEvent @event)
        {
            if (m_Engine == null) return Task.CompletedTask;

            var context = new SkriptContext
            {
                Player = new OpenModSkriptPlayer(@event.Player),
                EventData = $"damage:{@event.DamageAmount}"
            };

            return m_Engine.RaiseEvent("player_damaged", context);
        }

        // Event: player_spawned
        private Task OnPlayerSpawned(IServiceProvider serviceProvider, object? sender, UnturnedPlayerSpawnedEvent @event)
        {
            if (m_Engine == null) return Task.CompletedTask;

            var context = new SkriptContext
            {
                Player = new OpenModSkriptPlayer(@event.Player)
            };

            return m_Engine.RaiseEvent("player_spawned", context);
        }

        // Public method for reloading scripts (used by command)
        public void ReloadScripts()
        {
            if (m_Engine != null)
            {
                m_Engine.Reload(m_ScriptsDirectory);
                SetupTimers(); // Reload timers
            }
        }

        // Getter for engine (for command)
        public USkriptEngine? Engine => m_Engine;

        // Setup timers for "every" events
        private void SetupTimers()
        {
            m_Timers.Clear();

            if (m_Engine == null) return;

            var allEvents = m_Engine.ScriptManager.GetAllEvents();
            
            foreach (var eventNode in allEvents)
            {
                if (eventNode.EventName == "every" && !string.IsNullOrEmpty(eventNode.EventData))
                {
                    float intervalSeconds = ParseTimeInterval(eventNode.EventData);
                    
                    if (intervalSeconds > 0)
                    {
                        var timerKey = $"every_{eventNode.EventData}";
                        
                        m_Timers[timerKey] = new TimerData
                        {
                            EventName = "every",
                            EventData = eventNode.EventData,
                            IntervalSeconds = intervalSeconds,
                            NextTrigger = DateTime.Now.AddSeconds(intervalSeconds)
                        };
                        
                        m_Logger.LogInformation($"USkript: Registered timer every {intervalSeconds}s");
                    }
                }
            }
        }

        // Parse time interval in seconds
        private float ParseTimeInterval(string timeString)
        {
            // Time is now specified directly in seconds: every(300)
            if (float.TryParse(timeString.Trim(), out float seconds))
            {
                return seconds;
            }
            
            return 0;
        }

        // Start async timer loop
        private async void StartTimerLoop()
        {
            m_TimerRunning = true;
            
            await UniTask.SwitchToMainThread();
            
            while (m_TimerRunning)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1)); // Check every second

                    if (m_Engine == null) continue;

                    var now = DateTime.Now;

                    foreach (var timer in m_Timers.Values)
                    {
                        if (now >= timer.NextTrigger)
                        {
                            // Execute event
                            var context = new SkriptContext
                            {
                                EventData = timer.EventData
                            };
                            
                            _ = m_Engine.RaiseEvent(timer.EventName, context);

                            // Schedule next trigger
                            timer.NextTrigger = now.AddSeconds(timer.IntervalSeconds);
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.LogError(ex, "Error in USkript timer loop");
                }
            }
        }

        // Create example script if scripts folder is empty
        private void CreateExampleScriptIfNeeded()
        {
            if (!Directory.Exists(m_ScriptsDirectory))
            {
                Directory.CreateDirectory(m_ScriptsDirectory);
            }

            var showcaseFile = Path.Combine(m_ScriptsDirectory, "showcase.usk");
            
            // Only create if file doesn't exist
            if (!File.Exists(showcaseFile))
            {
                var showcaseContent = GetShowcaseContent();
                File.WriteAllText(showcaseFile, showcaseContent);
                m_Logger.LogInformation("Created example script: showcase.usk");
            }
        }

        private string GetShowcaseContent()
        {
            return @"# =============================================================================
# USkript - Complete Feature Showcase
# This file demonstrates ALL available features in USkript
# =============================================================================

# =============================================================================
# EVENTS - All available event types
# =============================================================================

# Event: player_join - triggered when player connects to server
event player_join(player):
    message player ""&a=== Welcome to the server! ===""
    message player ""&bPlayer: &f{player.name} &7(ID: {player.id})""
    message player ""&bPosition: &f{player.position} &7| Ping: {player.ping}ms""
    
    # Full heal and feed on join
    heal player
    feed player
    set_virus player 0
    
    # Give starter kit
    give player ""15"" 5
    give player ""Medkit"" 3
    
    # Add experience
    add_experience player 100
    set_reputation player 10
    
    # Broadcast
    broadcast ""&e{player.name} &ajoined the server!""


# Event: player_disconnect
event player_disconnect(player):
    broadcast ""&7{player.name} &eleft the server""


# Event: player_chat
event player_chat(player):
    # Admin heal command
    if equals msg ""/heal"":
        if has_permission player ""admin.commands"":
            heal player
            feed player
            message player ""&aYou have been healed!""
        else:
            message player ""&cNo permission!""
        cancel
    
    # Stats command
    if equals msg ""/stats"":
        message player ""&a=== YOUR STATS ===""
        message player ""&cHealth: &f{player.health}/100""
        message player ""&6Food: &f{player.food}/100""
        message player ""&bWater: &f{player.water}/100""
        message player ""&eXP: &f{player.experience}""
        message player ""&dRep: &f{player.reputation}""
        cancel


# Event: player_death
event player_death(player):
    broadcast ""&c☠ {player.name} &7died!""


# Event: player_damaged
event player_damaged(player):
    message player ""&7You took &c{damage} &7damage""
    
    if health player < 30:
        message player ""&c&l⚠ LOW HEALTH: {player.health} HP""


# Event: player_spawned
event player_spawned(player):
    message player ""&aYou spawned!""
    heal player
    feed player


# Event: every - timer events (time in seconds)
event every(30):
    broadcast ""&7Server running...""

event every(300):
    broadcast ""&e⚙ Auto-save...""
    run_command ""save""


# =============================================================================
# ACTIONS - All available actions
# =============================================================================

# Message Actions
# - message player ""text""
# - broadcast ""text""

# Inventory Actions
# - give player ""ItemID"" amount
# - clear_inventory player

# Health & Stats Actions
# - heal player (full health)
# - feed player (full food and water)
# - set_health player amount
# - set_food player amount
# - set_water player amount
# - set_stamina player amount
# - set_virus player amount
# - kill player

# Experience & Reputation
# - add_experience player amount
# - set_experience player amount
# - set_reputation player amount

# Teleportation
# - teleport player ""x,y,z""

# Administrative
# - kick player ""reason""
# - exit_vehicle player
# - run_command ""command""

# Event Control
# - cancel


# =============================================================================
# CONDITIONS - All available conditions
# =============================================================================

# String Conditions
# - startswith msg ""text""
# - equals msg ""text""

# Permission
# - has_permission player ""permission.node""

# Stats (operators: >=, <=, >, <, ==)
# - health player >= amount
# - food player < amount
# - water player > amount
# - experience player >= amount
# - reputation player <= amount
# - money player >= amount

# Vehicle
# - is_in_vehicle player


# =============================================================================
# VARIABLES - Available in strings
# =============================================================================

# Player: {player.name}, {player.id}, {player.displayname}
# Player Stats: {player.health}, {player.food}, {player.water}
# Player Info: {player.experience}, {player.reputation}, {player.group}
# Player Pos: {player.position}, {player.ping}
# Event: {msg}, {damage}


# =============================================================================
# COLOR CODES
# =============================================================================

# &0-Black &1-DarkBlue &2-DarkGreen &3-DarkCyan &4-DarkRed &5-Purple
# &6-Orange &7-Gray &8-DarkGray &9-Blue &a-Green &b-Cyan
# &c-Red &d-Pink &e-Yellow &f-White


# =============================================================================
# COMPLETE EXAMPLES
# =============================================================================

# VIP System
event player_join(player):
    if has_permission player ""vip.rank"":
        broadcast ""&6&l[VIP] &f{player.name} &ejoined!""
        heal player
        feed player
        give player ""15"" 10
        add_experience player 500
    else:
        broadcast ""&7{player.name} joined""
        give player ""15"" 3


# Chat Commands
event player_chat(player):
    if equals msg ""/kit"":
        if has_permission player ""kit.starter"":
            give player ""15"" 5
            give player ""Medkit"" 3
            give player ""363"" 1
            message player ""&a✓ Starter kit received!""
        else:
            message player ""&c✗ No permission!""
        cancel
    
    if equals msg ""/spawn"":
        teleport player ""0,10,0""
        message player ""&a✓ Teleported to spawn!""
        cancel


# Auto-Heal System
event player_damaged(player):
    if health player < 15:
        message player ""&c&lAUTO-HEAL ACTIVATED!""
        set_health player 50


# Experience Rewards
event player_join(player):
    if experience player >= 50000:
        message player ""&6&l★★★ LEGEND ★★★""
        give player ""15"" 20
    else:
        if experience player >= 20000:
            message player ""&b&l★★ VETERAN ★★""
            give player ""15"" 10
        else:
            if experience player >= 5000:
                message player ""&a&l★ EXPERIENCED ★""
                give player ""15"" 5


# Server Maintenance
event every(300):
    run_command ""save""

event every(3600):
    broadcast ""&6&l=== HOURLY MAINTENANCE ===""
    run_command ""save""


# =============================================================================
# For complete documentation, visit:
# https://github.com/kamasix/USkript
# =============================================================================
";
        }
    }
}
