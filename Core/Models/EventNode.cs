using System.Collections.Generic;

namespace USkript.Core.Models
{
    /// <summary>
    /// Represents an event in USkript, e.g. event player_join(player)
    /// </summary>
    public class EventNode : Node
    {
        /// <summary>
        /// Event name (e.g. "player_join", "player_chat", "every")
        /// </summary>
        public string EventName { get; set; } = string.Empty;

        /// <summary>
        /// Event parameter list (e.g. ["player"], ["player", "msg"])
        /// </summary>
        public List<string> Parameters { get; set; } = new List<string>();

        /// <summary>
        /// Additional data for special events (e.g. "5 minutes" for "every")
        /// </summary>
        public string? EventData { get; set; }

        /// <summary>
        /// Event body - list of statements (actions, ifs, etc.)
        /// </summary>
        public List<Node> Body { get; set; } = new List<Node>();

        public override string ToString()
        {
            var paramStr = string.Join(", ", Parameters);
            var dataStr = string.IsNullOrEmpty(EventData) ? "" : $" [{EventData}]";
            return $"Event: {EventName}({paramStr}){dataStr}";
        }
    }
}
