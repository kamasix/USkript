using System;
using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;

namespace USkript.Commands
{
    [Command("uskript")]
    [CommandAlias("usk")]
    [CommandDescription("Main USkript command")]
    public class CommandUSkript : UnturnedCommand
    {
        public CommandUSkript(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override async UniTask OnExecuteAsync()
        {
            await PrintAsync("&e=== USkript Engine ===");
            await PrintAsync("&fUse &a/usk reload &fto reload scripts");
            await PrintAsync("&fUse &a/usk info &fto view information about loaded scripts");
        }
    }
}
