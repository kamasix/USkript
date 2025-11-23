namespace USkript.Core.Models
{
    /// <summary>
    /// Represents an action in USkript (e.g. message, give, teleport, cancel)
    /// </summary>
    public class ActionNode : Node
    {
        /// <summary>
        /// Raw action text (e.g. "message player \"hello\"", "give player \"MapleStrike\" 1")
        /// Interpreter will parse this at runtime
        /// </summary>
        public string ActionRaw { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Action: {ActionRaw}";
        }
    }
}
