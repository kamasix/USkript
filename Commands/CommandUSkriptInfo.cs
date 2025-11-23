using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;

namespace USkript.Commands
{
    [Command("info")]
    [CommandDescription("Displays information about loaded scripts")]
    [CommandParent(typeof(CommandUSkript))]
    public class CommandUSkriptInfo : UnturnedCommand
    {
        private readonly USkript _plugin;
        private readonly ILogger<CommandUSkriptInfo> _logger;

        public CommandUSkriptInfo(
            USkript plugin,
            ILogger<CommandUSkriptInfo> logger,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _plugin = plugin;
            _logger = logger;
        }

        protected override async UniTask OnExecuteAsync()
        {
            var stats = _plugin.Engine?.ScriptManager.GetStats();
            
            if (stats == null || stats.TotalScripts == 0)
            {
                await PrintAsync("&cNo scripts loaded");
                return;
            }

            await PrintAsync($"&e=== USkript Info ===");
            await PrintAsync($"&fLoaded scripts: &a{stats.TotalScripts}");
            await PrintAsync($"&fTotal events: &a{stats.TotalEvents}");
            await PrintAsync($"&f");
            await PrintAsync($"&fEvent types:");
            
            foreach (var eventType in stats.EventTypes)
            {
                var count = _plugin.Engine?.ScriptManager.GetEvents(eventType).Count ?? 0;
                await PrintAsync($"  &7- &e{eventType} &7({count} handler(s))");
            }
        }
    }
}
