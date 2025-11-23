using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using USkript.Core.Models;

namespace USkript.Core.Parsing
{
    /// <summary>
    /// Parser for .usk files
    /// Parses indentation-based language (Python-style)
    /// </summary>
    public class ScriptParser
    {
        private const int IndentSize = 4; // 4 spaces = 1 indentation level
        
        /// <summary>
        /// Parses a .usk file from disk
        /// </summary>
        public ScriptFile ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Script file not found: {filePath}");

            var lines = File.ReadAllLines(filePath);
            return ParseLines(filePath, lines);
        }

        /// <summary>
        /// Parses .usk code lines
        /// </summary>
        public ScriptFile ParseLines(string filePath, string[] lines)
        {
            var scriptFile = new ScriptFile
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath
            };

            var processedLines = PreprocessLines(lines);
            int i = 0;

            while (i < processedLines.Count)
            {
                var line = processedLines[i];
                
                // Only top-level can be events
                if (line.IndentLevel == 0 && line.Content.StartsWith("event "))
                {
                    var eventNode = ParseEvent(processedLines, ref i);
                    if (eventNode != null)
                        scriptFile.Events.Add(eventNode);
                }
                else
                {
                    i++; // Skip unknown top-level lines
                }
            }

            return scriptFile;
        }

        /// <summary>
        /// Preprocesses lines - removes comments, empty lines, calculates indentation
        /// </summary>
        private List<ProcessedLine> PreprocessLines(string[] lines)
        {
            var result = new List<ProcessedLine>();

            for (int i = 0; i < lines.Length; i++)
            {
                var raw = lines[i];
                
                // Remove comments (# in line)
                var commentIndex = raw.IndexOf('#');
                if (commentIndex >= 0)
                    raw = raw.Substring(0, commentIndex);

                // Trim trailing whitespace
                raw = raw.TrimEnd();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                // Calculate indentation
                int indent = 0;
                foreach (char c in raw)
                {
                    if (c == ' ')
                        indent++;
                    else if (c == '\t')
                        indent += IndentSize; // Tab = 4 spacje
                    else
                        break;
                }

                var content = raw.TrimStart();
                int indentLevel = indent / IndentSize;

                result.Add(new ProcessedLine
                {
                    LineNumber = i + 1,
                    IndentLevel = indentLevel,
                    Content = content
                });
            }

            return result;
        }

        /// <summary>
        /// Parses event (event player_join(player):)
        /// </summary>
        private EventNode? ParseEvent(List<ProcessedLine> lines, ref int index)
        {
            var line = lines[index];
            
            // Regex: event <name>(<parameters>):
            // or: event every(<time>):
            var match = Regex.Match(line.Content, @"^event\s+(\w+)\s*\((.*?)\)\s*:\s*$");
            if (!match.Success)
            {
                index++;
                return null;
            }

            var eventNode = new EventNode
            {
                LineNumber = line.LineNumber,
                IndentLevel = line.IndentLevel,
                EventName = match.Groups[1].Value
            };

            // Parse parameters
            var paramsStr = match.Groups[2].Value.Trim();
            if (!string.IsNullOrEmpty(paramsStr))
            {
                // Special case for every(5 minutes)
                if (eventNode.EventName == "every")
                {
                    eventNode.EventData = paramsStr; // "5 minutes", "30 seconds"
                }
                else
                {
                    // Normal comma-separated parameters
                    eventNode.Parameters = paramsStr
                        .Split(',')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToList();
                }
            }

            // Parse body (all lines with indentation greater than event)
            index++;
            var bodyIndent = line.IndentLevel + 1;
            eventNode.Body = ParseBlock(lines, ref index, bodyIndent);

            return eventNode;
        }

        /// <summary>
        /// Parses a code block (event body, if, else)
        /// </summary>
        private List<Node> ParseBlock(List<ProcessedLine> lines, ref int index, int expectedIndent)
        {
            var nodes = new List<Node>();

            while (index < lines.Count)
            {
                var line = lines[index];

                // If indentation less than expected - end of block
                if (line.IndentLevel < expectedIndent)
                    break;

                // If indentation greater - error (invalid indentation)
                if (line.IndentLevel > expectedIndent)
                {
                    index++; // Skip, but could also throw exception
                    continue;
                }

                // if statement
                if (line.Content.StartsWith("if ") && line.Content.EndsWith(":"))
                {
                    var ifNode = ParseIf(lines, ref index, expectedIndent);
                    if (ifNode != null)
                        nodes.Add(ifNode);
                }
                // else statement
                else if (line.Content == "else:")
                {
                    // else is handled in ParseIf
                    break;
                }
                // Action (everything else)
                else
                {
                    nodes.Add(new ActionNode
                    {
                        LineNumber = line.LineNumber,
                        IndentLevel = line.IndentLevel,
                        ActionRaw = line.Content
                    });
                    index++;
                }
            }

            return nodes;
        }

        /// <summary>
        /// Parses if (and optionally else)
        /// </summary>
        private IfNode? ParseIf(List<ProcessedLine> lines, ref int index, int currentIndent)
        {
            var line = lines[index];

            // Regex: if <warunek>:
            var match = Regex.Match(line.Content, @"^if\s+(.+?):\s*$");
            if (!match.Success)
            {
                index++;
                return null;
            }

            var ifNode = new IfNode
            {
                LineNumber = line.LineNumber,
                IndentLevel = line.IndentLevel,
                ConditionRaw = match.Groups[1].Value.Trim()
            };

            // Parse then body
            index++;
            var bodyIndent = currentIndent + 1;
            ifNode.ThenBody = ParseBlock(lines, ref index, bodyIndent);

            // Check if there's else
            if (index < lines.Count)
            {
                var nextLine = lines[index];
                if (nextLine.IndentLevel == currentIndent && nextLine.Content == "else:")
                {
                    index++; // Skip "else:"
                    ifNode.ElseBody = ParseBlock(lines, ref index, bodyIndent);
                }
            }

            return ifNode;
        }

        /// <summary>
        /// Helper class for storing processed line
        /// </summary>
        private class ProcessedLine
        {
            public int LineNumber { get; set; }
            public int IndentLevel { get; set; }
            public string Content { get; set; } = string.Empty;
        }
    }
}
