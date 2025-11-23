using System;

namespace USkript.Core.Runtime
{
    /// <summary>
    /// Player abstraction - platform independent (OpenMod, RocketMod)
    /// </summary>
    public interface ISkriptPlayer
    {
        /// <summary>
        /// Unique player identifier (CSteamID)
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Player name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Display name for player (with colors)
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Sends message to player
        /// </summary>
        void SendMessage(string message);

        /// <summary>
        /// Gives item to player
        /// </summary>
        /// <param name="itemId">Item ID (name or number)</param>
        /// <param name="amount">Amount</param>
        void GiveItem(string itemId, int amount);

        /// <summary>
        /// Teleports player to location
        /// </summary>
        /// <param name="location">Warp name or coordinates</param>
        void Teleport(string location);

        /// <summary>
        /// Checks if player has permission
        /// </summary>
        bool HasPermission(string permission);

        /// <summary>
        /// Gets player's money amount
        /// </summary>
        decimal GetMoney();

        /// <summary>
        /// Adds money to player
        /// </summary>
        void AddMoney(decimal amount);

        /// <summary>
        /// Sets player's money
        /// </summary>
        void SetMoney(decimal amount);

        /// <summary>
        /// Gets player's health
        /// </summary>
        byte GetHealth();

        /// <summary>
        /// Sets player's health
        /// </summary>
        void SetHealth(byte health);

        /// <summary>
        /// Kills player
        /// </summary>
        void Kill();
    }
}
