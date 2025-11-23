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
        /// Finds player by name or ID
        /// </summary>
        ISkriptPlayer? FindPlayer(string nameOrId);
    }
}
