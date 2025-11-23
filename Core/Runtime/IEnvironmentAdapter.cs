using System;
using System.Threading.Tasks;

namespace USkript.Core.Runtime
{
    /// <summary>
    /// Environment adapter - global functions independent of player
    /// </summary>
    public interface IEnvironmentAdapter
    {
        /// <summary>
        /// Sends global message (broadcast)
        /// </summary>
        void Broadcast(string message);

        /// <summary>
        /// Executes server command
        /// </summary>
        Task RunCommand(string command);

        /// <summary>
        /// Logger for debugging
        /// </summary>
        void Log(string message);

        /// <summary>
        /// Error logger
        /// </summary>
        void LogError(string message, Exception? exception = null);

        /// <summary>
        /// Gets all online players
        /// </summary>
        System.Collections.Generic.List<ISkriptPlayer> GetAllPlayers();

        /// <summary>
        /// Sets weather on the server
        /// </summary>
        void SetWeather(string weatherType);

        /// <summary>
        /// Sets time of day
        /// </summary>
        void SetTime(string timeOfDay);

        /// <summary>
        /// Enables or disables day/night cycle
        /// </summary>
        void SetTimeCycle(bool enabled);

        /// <summary>
        /// Unbans player by Steam ID
        /// </summary>
        void UnbanPlayer(string steamId);

        /// <summary>
        /// Checks if Steam ID is banned
        /// </summary>
        bool IsBanned(string steamId);
    }
}
