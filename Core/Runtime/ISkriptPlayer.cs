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

        /// <summary>
        /// Gets player's food level
        /// </summary>
        byte GetFood();

        /// <summary>
        /// Sets player's food level
        /// </summary>
        void SetFood(byte food);

        /// <summary>
        /// Gets player's water level
        /// </summary>
        byte GetWater();

        /// <summary>
        /// Sets player's water level
        /// </summary>
        void SetWater(byte water);

        /// <summary>
        /// Gets player's stamina
        /// </summary>
        byte GetStamina();

        /// <summary>
        /// Sets player's stamina
        /// </summary>
        void SetStamina(byte stamina);

        /// <summary>
        /// Gets player's virus level (infection)
        /// </summary>
        byte GetVirus();

        /// <summary>
        /// Sets player's virus level
        /// </summary>
        void SetVirus(byte virus);

        /// <summary>
        /// Kicks player from server
        /// </summary>
        void Kick(string reason);

        /// <summary>
        /// Gets player's group
        /// </summary>
        string GetGroup();

        /// <summary>
        /// Gets player's experience
        /// </summary>
        uint GetExperience();

        /// <summary>
        /// Adds experience to player
        /// </summary>
        void AddExperience(uint amount);

        /// <summary>
        /// Sets player's experience
        /// </summary>
        void SetExperience(uint amount);

        /// <summary>
        /// Gets player's reputation
        /// </summary>
        int GetReputation();

        /// <summary>
        /// Sets player's reputation
        /// </summary>
        void SetReputation(int reputation);

        /// <summary>
        /// Heals player fully
        /// </summary>
        void Heal();

        /// <summary>
        /// Feeds player (max food and water)
        /// </summary>
        void Feed();

        /// <summary>
        /// Clears player's inventory
        /// </summary>
        void ClearInventory();

        /// <summary>
        /// Gets player's position as string
        /// </summary>
        string GetPosition();

        /// <summary>
        /// Gets player's ping
        /// </summary>
        float GetPing();

        /// <summary>
        /// Checks if player is in vehicle
        /// </summary>
        bool IsInVehicle();

        /// <summary>
        /// Removes player from vehicle
        /// </summary>
        void ExitVehicle();
    }
}
