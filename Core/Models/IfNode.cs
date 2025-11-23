using System.Collections.Generic;

namespace USkript.Core.Models
{
    /// <summary>
    /// Represents an if condition in USkript
    /// </summary>
    public class IfNode : Node
    {
        /// <summary>
        /// Raw condition text (e.g. "startswith msg \"!kit\"", "money player >= 1000")
        /// Interpreter will parse this at runtime
        /// </summary>
        public string ConditionRaw { get; set; } = string.Empty;

        /// <summary>
        /// If block body (when condition is true)
        /// </summary>
        public List<Node> ThenBody { get; set; } = new List<Node>();

        /// <summary>
        /// Else block body (optional)
        /// </summary>
        public List<Node> ElseBody { get; set; } = new List<Node>();

        public override string ToString()
        {
            return $"If: {ConditionRaw}";
        }
    }
}
