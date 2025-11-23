using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenMod.API.Users;
using OpenMod.Unturned.Users;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using USkript.Core.Runtime;
using UnityEngine;
using Steamworks;

namespace USkript.OpenMod
{
    /// <summary>
    /// Unturned player adapter for USkript
    /// </summary>
    public class OpenModSkriptPlayer : ISkriptPlayer
    {
        private readonly UnturnedPlayer _unturnedPlayer;
        private readonly SDG.Unturned.Player _player;

        public OpenModSkriptPlayer(UnturnedPlayer unturnedPlayer)
        {
            _unturnedPlayer = unturnedPlayer ?? throw new ArgumentNullException(nameof(unturnedPlayer));
            _player = unturnedPlayer.Player; // SDG.Unturned.Player
        }

        public string Id => _player.channel.owner.playerID.steamID.ToString();

        public string Name => _player.channel.owner.playerID.characterName;

        public string DisplayName => _player.channel.owner.playerID.nickName;

        public void SendMessage(string message)
        {
            // Convert colors &a -> <color=#...>
            var formattedMessage = FormatColors(message);
            ChatManager.serverSendMessage(formattedMessage, Color.white, null, _player.channel.owner, 
                EChatMode.SAY, null, useRichTextFormatting: true);
        }

        public void GiveItem(string itemId, int amount)
        {
            // Try parsing as ushort (numeric ID) or find by name
            ushort id;
            if (!ushort.TryParse(itemId, out id))
            {
                // Search by name
                ItemAsset? asset = null;
                
                #pragma warning disable CS0618 // Type or member is obsolete
                var allAssets = Assets.find(EAssetType.ITEM);
                #pragma warning restore CS0618
                
                foreach (var a in allAssets)
                {
                    if (a is ItemAsset itemAsset && 
                        (itemAsset.itemName?.IndexOf(itemId, StringComparison.OrdinalIgnoreCase) >= 0 ||
                         itemAsset.name?.IndexOf(itemId, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        asset = itemAsset;
                        break;
                    }
                }
                
                if (asset != null)
                {
                    id = asset.id;
                }
                else
                {
                    return; // Item not found
                }
            }

            var item = new Item(id, EItemOrigin.ADMIN);
            for (int i = 0; i < amount; i++)
            {
                _player.inventory.tryAddItem(item, true);
            }
        }

        public void Teleport(string location)
        {
            // Simple implementation - can be extended with warp system
            // For now we assume coordinates "x,y,z" or warp name (TODO)
            
            // Example: "0,10,0"
            var parts = location.Split(',');
            if (parts.Length == 3)
            {
                if (float.TryParse(parts[0], out var x) && 
                    float.TryParse(parts[1], out var y) && 
                    float.TryParse(parts[2], out var z))
                {
                    _player.teleportToLocation(new Vector3(x, y, z), _player.transform.rotation.eulerAngles.y);
                }
            }
            // TODO: Integration with warp system from other plugins
        }

        public bool HasPermission(string permission)
        {
            // OpenMod permission check
            // Requires access to IPermissionChecker - will be passed through adapter
            return false; // TODO: Implementation through IPermissionChecker
        }

        public decimal GetMoney()
        {
            // TODO: Integration with economy plugin (e.g. OpenMod.Economy)
            return 0;
        }

        public void AddMoney(decimal amount)
        {
            // TODO: Integration with economy plugin
        }

        public void SetMoney(decimal amount)
        {
            // TODO: Integration with economy plugin
        }

        public byte GetHealth()
        {
            return _player.life.health;
        }

        public void SetHealth(byte health)
        {
            _player.life.askHeal(health, true, true);
        }

        public void Kill()
        {
            EPlayerKill outKill;
            _player.life.askDamage(100, Vector3.up * 10f, EDeathCause.KILL, ELimb.SKULL, 
                Steamworks.CSteamID.Nil, out outKill, trackKill: false, ERagdollEffect.NONE);
        }

        private string FormatColors(string message)
        {
            // Convert Minecraft-style colors to Unity Rich Text
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
