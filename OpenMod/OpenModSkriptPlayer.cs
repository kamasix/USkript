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
            // Send message without color formatting
            ChatManager.serverSendMessage(message, Color.white, null, _player.channel.owner, 
                EChatMode.SAY, null, useRichTextFormatting: false);
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

        public byte GetFood()
        {
            return _player.life.food;
        }

        public void SetFood(byte food)
        {
            _player.life.serverModifyFood(food);
        }

        public byte GetWater()
        {
            return _player.life.water;
        }

        public void SetWater(byte water)
        {
            _player.life.serverModifyWater(water);
        }

        public byte GetStamina()
        {
            return _player.life.stamina;
        }

        public void SetStamina(byte stamina)
        {
            _player.life.serverModifyStamina(stamina);
        }

        public byte GetVirus()
        {
            return _player.life.virus;
        }

        public void SetVirus(byte virus)
        {
            _player.life.serverModifyVirus(virus);
        }

        public void Kick(string reason)
        {
            Provider.kick(_player.channel.owner.playerID.steamID, reason);
        }

        public string GetGroup()
        {
            return _player.channel.owner.playerID.group.ToString();
        }

        public uint GetExperience()
        {
            return _player.skills.experience;
        }

        public void AddExperience(uint amount)
        {
            _player.skills.ServerSetExperience(_player.skills.experience + amount);
        }

        public void SetExperience(uint amount)
        {
            _player.skills.ServerSetExperience(amount);
        }

        public int GetReputation()
        {
            return _player.skills.reputation;
        }

        public void SetReputation(int reputation)
        {
            _player.skills.askRep(reputation);
        }

        public void Heal()
        {
            _player.life.askHeal(100, true, true);
        }

        public void Feed()
        {
            _player.life.serverModifyFood(100);
            _player.life.serverModifyWater(100);
        }

        public void ClearInventory()
        {
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                if (page == PlayerInventory.AREA) continue; // Skip area (hands)
                
                var count = _player.inventory.getItemCount(page);
                for (byte index = 0; index < count; index++)
                {
                    _player.inventory.removeItem(page, 0);
                }
            }
        }

        public string GetPosition()
        {
            var pos = _player.transform.position;
            return $"{pos.x:F1},{pos.y:F1},{pos.z:F1}";
        }

        public float GetPing()
        {
            return _player.channel.owner.ping;
        }

        public bool IsInVehicle()
        {
            return _player.movement.getVehicle() != null;
        }

        public void ExitVehicle()
        {
            var vehicle = _player.movement.getVehicle();
            if (vehicle != null)
            {
                VehicleManager.forceRemovePlayer(vehicle, _player.channel.owner.playerID.steamID);
            }
        }

        public void SpawnVehicle(string vehicleId)
        {
            // Parse vehicle ID
            if (!ushort.TryParse(vehicleId, out ushort id))
            {
                return;
            }

            // Get player position
            var position = _player.transform.position + _player.transform.forward * 5f; // 5 meters in front
            var rotation = _player.transform.rotation;

            // Spawn vehicle
            VehicleManager.spawnVehicleV2(id, position, rotation);
        }

        public bool HasItem(string itemId, int amount)
        {
            // Parse item ID
            if (!ushort.TryParse(itemId, out ushort id))
            {
                return false;
            }

            // Count items in inventory
            int count = 0;
            var items = _player.inventory.items;

            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                var pageItems = items[page];
                if (pageItems == null) continue;

                for (byte index = 0; index < pageItems.getItemCount(); index++)
                {
                    var jar = pageItems.getItem(index);
                    if (jar != null && jar.item.id == id)
                    {
                        count += jar.item.amount;
                    }
                }
            }

            return count >= amount;
        }

        public void Ban(string reason, uint duration)
        {
            // Ban player using Unturned's Provider
            var steamId = _player.channel.owner.playerID.steamID;
            var hwids = new byte[0][]; // Empty HWID list
            
#pragma warning disable CS0618
            SDG.Unturned.Provider.requestBanPlayer(
                Steamworks.CSteamID.Nil, // Admin SteamID (nil = console)
                steamId,
                0, // IP (0 = use Steam ID only)
                reason,
                duration
            );
#pragma warning restore CS0618
        }
    }
}
