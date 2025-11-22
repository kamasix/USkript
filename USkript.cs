using System;
using System.IO;
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
using USkript.Core.Runtime;
using USkript.OpenMod;

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

            // Subscribe to OpenMod events
            m_EventBus.Subscribe<UnturnedPlayerConnectedEvent>(this, OnPlayerConnected);
            m_EventBus.Subscribe<UnturnedPlayerDeathEvent>(this, OnPlayerDeath);
            m_EventBus.Subscribe<UnturnedPlayerChattingEvent>(this, OnPlayerChatting);

            m_Logger.LogInformation("=== USkript Engine Loaded Successfully ===");
            await UniTask.CompletedTask;
        }

        protected override async UniTask OnUnloadAsync()
        {
            m_Logger.LogInformation("USkript Engine unloading...");
            
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

        // Public method for reloading scripts (used by command)
        public void ReloadScripts()
        {
            if (m_Engine != null)
            {
                m_Engine.Reload(m_ScriptsDirectory);
            }
        }

        // Getter for engine (for command)
        public USkriptEngine? Engine => m_Engine;
    }
}
