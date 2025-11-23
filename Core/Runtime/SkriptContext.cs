using System.Collections.Generic;

namespace USkript.Core.Runtime
{
    /// <summary>
    /// Script execution context
    /// Stores data available for event and actions
    /// </summary>
    public class SkriptContext
    {
        /// <summary>
        /// Player associated with event (if applicable)
        /// </summary>
        public ISkriptPlayer? Player { get; set; }

        /// <summary>
        /// Message (for chat events)
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Whether event was cancelled (for cancel)
        /// </summary>
        public bool Cancelled { get; set; }

        /// <summary>
        /// Event local variables (for future extension)
        /// </summary>
        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Environment adapter (access to broadcast, run_command, etc.)
        /// </summary>
        public IEnvironmentAdapter? Environment { get; set; }

        /// <summary>
        /// Event data (e.g. for "every" - time)
        /// </summary>
        public string? EventData { get; set; }

        /// <summary>
        /// Damage amount (for player_damaged event)
        /// </summary>
        public float DamageAmount
        {
            get
            {
                if (!string.IsNullOrEmpty(EventData) && EventData.StartsWith("damage:"))
                {
                    var parts = EventData.Split(':');
                    if (parts.Length == 2 && float.TryParse(parts[1], out var damage))
                    {
                        return damage;
                    }
                }
                return 0;
            }
        }
    }
}
