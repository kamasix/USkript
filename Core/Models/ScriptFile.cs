using System.Collections.Generic;

namespace USkript.Core.Models
{
    /// <summary>
    /// Represents a parsed .usk file
    /// </summary>
    public class ScriptFile
    {
        /// <summary>
        /// File name (e.g. "join.usk")
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Full file path
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// List of all events in this file
        /// </summary>
        public List<EventNode> Events { get; set; } = new List<EventNode>();

        public override string ToString()
        {
            return $"ScriptFile: {FileName} ({Events.Count} events)";
        }
    }
}
