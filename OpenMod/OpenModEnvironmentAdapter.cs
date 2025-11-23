using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMod.API.Users;
using OpenMod.API.Commands;
using USkript.Core.Runtime;
using OpenMod.Core.Users;

namespace USkript.OpenMod
{
    /// <summary>
    /// OpenMod environment adapter for USkript
    /// </summary>
    public class OpenModEnvironmentAdapter : IEnvironmentAdapter
    {
        private readonly ILogger _logger;
        private readonly IUserManager _userManager;

        public OpenModEnvironmentAdapter(
            ILogger logger, 
            IUserManager userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public void Broadcast(string message)
        {
            var formattedMessage = FormatColors(message);
            SDG.Unturned.ChatManager.serverSendMessage(
                formattedMessage, 
                UnityEngine.Color.white, 
                null, 
                null, 
                SDG.Unturned.EChatMode.SAY, 
                null, 
                useRichTextFormatting: true);
        }

        public async Task RunCommand(string command)
        {
            try
            {
                // Execute command through command executor
                // TODO: Requires implementation through ICommandExecutor from OpenMod
                await Task.CompletedTask;
                _logger.LogWarning($"RunCommand not fully implemented yet: {command}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to run command: {command}", ex);
            }
        }

        public void Log(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            if (exception != null)
            {
                _logger.LogError(exception, message);
            }
            else
            {
                _logger.LogError(message);
            }
        }

        public ISkriptPlayer? FindPlayer(string nameOrId)
        {
            // Get all online players through Unturned SDK
            var onlinePlayers = SDG.Unturned.Provider.clients;
            
            foreach (var client in onlinePlayers)
            {
                var steamId = client.playerID.steamID.ToString();
                var displayName = client.playerID.characterName;
                
                if (steamId.Equals(nameOrId, StringComparison.OrdinalIgnoreCase) ||
                    displayName.Equals(nameOrId, StringComparison.OrdinalIgnoreCase))
                {
                    var unturnedPlayer = SDG.Unturned.PlayerTool.getPlayer(client.playerID.steamID);
                    if (unturnedPlayer != null)
                    {
                        // Try finding UnturnedPlayer through UserManager
                        var task = _userManager.FindUserAsync(KnownActorTypes.Player, steamId, UserSearchMode.FindById);
                        task.Wait();
                        
                        if (task.Result is global::OpenMod.Unturned.Players.UnturnedPlayer openModPlayer)
                        {
                            return new OpenModSkriptPlayer(openModPlayer);
                        }
                    }
                }
            }

            return null;
        }

        private string FormatColors(string message)
        {
            return message
                .Replace("&a", "<color=green>")
                .Replace("&e", "<color=yellow>")
                .Replace("&c", "<color=red>")
                .Replace("&f", "<color=white>")
                .Replace("&0", "<color=black>")
                .Replace("&1", "<color=blue>")
                .Replace("&2", "<color=#00AA00>")
                .Replace("&3", "<color=cyan>")
                .Replace("&4", "<color=#AA0000>")
                .Replace("&5", "<color=magenta>")
                .Replace("&6", "<color=orange>")
                .Replace("&7", "<color=grey>")
                .Replace("&8", "<color=#555555>")
                .Replace("&9", "<color=#5555FF>")
                .Replace("&b", "<color=#55FFFF>")
                .Replace("&d", "<color=#FF55FF>")
                + "</color>";
        }
    }
}
