using System;
using System.Collections.Generic;
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
            SDG.Unturned.ChatManager.serverSendMessage(
                message, 
                UnityEngine.Color.white, 
                null, 
                null, 
                SDG.Unturned.EChatMode.SAY, 
                null, 
                useRichTextFormatting: false);
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

        public List<ISkriptPlayer> GetAllPlayers()
        {
            var players = new List<ISkriptPlayer>();
            
            // Get all online Unturned players
            foreach (var client in SDG.Unturned.Provider.clients)
            {
                var steamId = client.playerID.steamID.ToString();
                var unturnedPlayer = SDG.Unturned.PlayerTool.getPlayer(client.playerID.steamID);
                
                if (unturnedPlayer != null)
                {
                    // Find OpenMod player wrapper
                    var task = _userManager.FindUserAsync(
                        KnownActorTypes.Player, 
                        steamId, 
                        UserSearchMode.FindById);
                    task.Wait();
                    
                    if (task.Result is global::OpenMod.Unturned.Players.UnturnedPlayer openModPlayer)
                    {
                        players.Add(new OpenModSkriptPlayer(openModPlayer));
                    }
                }
            }
            
            return players;
        }

        public void SetWeather(string weatherType)
        {
            // Unturned weather control using LightingManager
            SDG.Unturned.ELightingRain weather;
            switch (weatherType.ToLower())
            {
                case "none":
                case "clear":
                    weather = SDG.Unturned.ELightingRain.NONE;
                    break;
                case "drizzle":
                    weather = SDG.Unturned.ELightingRain.DRIZZLE;
                    break;
                case "rain":
                    weather = SDG.Unturned.ELightingRain.PRE_DRIZZLE;
                    break;
                case "storm":
                    weather = SDG.Unturned.ELightingRain.POST_DRIZZLE;
                    break;
                default:
                    weather = SDG.Unturned.ELightingRain.NONE;
                    break;
            }

            // Note: rainDuration and rainFrequency are obsolete in newer Unturned versions
            // Using deprecated methods for compatibility
#pragma warning disable CS0612
            SDG.Unturned.LightingManager.rainDuration = weather == SDG.Unturned.ELightingRain.NONE ? (uint)0 : (uint)3600;
            SDG.Unturned.LightingManager.rainFrequency = weather == SDG.Unturned.ELightingRain.NONE ? (uint)0 : (uint)1;
#pragma warning restore CS0612
        }

        public void SetTime(string timeOfDay)
        {
            // Set time using LightingManager
            uint time;
            switch (timeOfDay.ToLower())
            {
                case "day":
                case "noon":
                    time = 540; // 12:00 (noon)
                    break;
                case "night":
                case "midnight":
                    time = 0; // 00:00 (midnight)
                    break;
                case "dawn":
                case "sunrise":
                    time = 180; // 06:00
                    break;
                case "dusk":
                case "sunset":
                    time = 900; // 18:00
                    break;
                default:
                    time = 540;
                    break;
            }

            SDG.Unturned.LightingManager.time = time;
        }

        public void SetTimeCycle(bool enabled)
        {
            // Enable/disable day-night cycle
            SDG.Unturned.LightingManager.cycle = enabled ? (uint)60 : (uint)0;
        }

        public void UnbanPlayer(string steamId)
        {
            // Parse Steam ID
            if (!ulong.TryParse(steamId, out ulong id))
            {
                return;
            }

            var cSteamId = new Steamworks.CSteamID(id);
            
            // Remove from ban list
            for (int i = SDG.Unturned.SteamBlacklist.list.Count - 1; i >= 0; i--)
            {
                var ban = SDG.Unturned.SteamBlacklist.list[i];
                if (ban.playerID == cSteamId)
                {
                    SDG.Unturned.SteamBlacklist.list.RemoveAt(i);
                }
            }
            
            // Save ban list
            SDG.Unturned.SteamBlacklist.save();
        }

        public bool IsBanned(string steamId)
        {
            // Parse Steam ID
            if (!ulong.TryParse(steamId, out ulong id))
            {
                return false;
            }

            var cSteamId = new Steamworks.CSteamID(id);
            uint ipAddress = 0;
            
#pragma warning disable CS0618
            return SDG.Unturned.SteamBlacklist.checkBanned(cSteamId, ipAddress, out _);
#pragma warning restore CS0618
        }
    }
}
