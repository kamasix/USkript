namespace USkript.Core.Models
{
    /// <summary>
    /// Base class for all AST nodes in USkript
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// Line number in source file (for debugging)
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Indentation level (for parser)
        /// </summary>
        public int IndentLevel { get; set; }
    }
}
