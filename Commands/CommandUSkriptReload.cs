using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;

namespace USkript.Commands
{
    [Command("reload")]
    [CommandAlias("rl")]
    [CommandDescription("Reloads all USkript scripts")]
    [CommandParent(typeof(CommandUSkript))]
    public class CommandUSkriptReload : UnturnedCommand
    {
        private readonly USkript _plugin;
        private readonly ILogger<CommandUSkriptReload> _logger;

        public CommandUSkriptReload(
            USkript plugin,
            ILogger<CommandUSkriptReload> logger,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _plugin = plugin;
            _logger = logger;
        }

        protected override async UniTask OnExecuteAsync()
        {
            try
            {
                _plugin.ReloadScripts();
                
                var stats = _plugin.Engine?.ScriptManager.GetStats();
                if (stats != null)
                {
                    await PrintAsync($"&aUSkript reloaded!");
                    await PrintAsync($"&fLoaded &e{stats.TotalScripts} &fscript(s) with &e{stats.TotalEvents} &fevent(s)");
                    
                    if (stats.EventTypes.Count > 0)
                    {
                        await PrintAsync($"&fEvent types: &7{string.Join(", ", stats.EventTypes)}");
                    }
                }
                else
                {
                    await PrintAsync($"&aUSkript reloaded, but no stats available");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload USkript scripts via command");
                await PrintAsync($"&cError during reload: {ex.Message}");
            }
        }
    }
}
