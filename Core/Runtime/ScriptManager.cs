using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using USkript.Core.Models;
using USkript.Core.Parsing;

namespace USkript.Core.Runtime
{
    /// <summary>
    /// Manages loaded scripts
    /// </summary>
    public class ScriptManager
    {
        private readonly ScriptParser _parser = new ScriptParser();
        private readonly List<ScriptFile> _loadedScripts = new List<ScriptFile>();
        
        /// <summary>
        /// Event dictionary: event name -> list of event nodes
        /// </summary>
        private readonly Dictionary<string, List<EventNode>> _eventsByName = 
            new Dictionary<string, List<EventNode>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// All loaded scripts
        /// </summary>
        public IReadOnlyList<ScriptFile> LoadedScripts => _loadedScripts.AsReadOnly();

        /// <summary>
        /// Loads all .usk files from folder
        /// </summary>
        public void LoadScripts(string scriptsDirectory)
        {
            Clear();

            if (!Directory.Exists(scriptsDirectory))
            {
                Directory.CreateDirectory(scriptsDirectory);
                return;
            }

            var uskFiles = Directory.GetFiles(scriptsDirectory, "*.usk", SearchOption.AllDirectories);

            foreach (var file in uskFiles)
            {
                try
                {
                    var scriptFile = _parser.ParseFile(file);
                    _loadedScripts.Add(scriptFile);

                    // Indexuj eventy
                    foreach (var eventNode in scriptFile.Events)
                    {
                        if (!_eventsByName.ContainsKey(eventNode.EventName))
                        {
                            _eventsByName[eventNode.EventName] = new List<EventNode>();
                        }
                        _eventsByName[eventNode.EventName].Add(eventNode);
                    }
                }
                catch (Exception ex)
                {
                    // Parsing error log - will be handled by IEnvironmentAdapter
                    throw new Exception($"Failed to parse script file: {file}", ex);
                }
            }
        }

        /// <summary>
        /// Gets all events with given name
        /// </summary>
        public List<EventNode> GetEvents(string eventName)
        {
            if (_eventsByName.TryGetValue(eventName, out var events))
            {
                return events;
            }
            return new List<EventNode>();
        }

        /// <summary>
        /// Clears all loaded scripts
        /// </summary>
        public void Clear()
        {
            _loadedScripts.Clear();
            _eventsByName.Clear();
        }

        /// <summary>
        /// Statistics of loaded scripts
        /// </summary>
        public ScriptStats GetStats()
        {
            return new ScriptStats
            {
                TotalScripts = _loadedScripts.Count,
                TotalEvents = _loadedScripts.Sum(s => s.Events.Count),
                EventTypes = _eventsByName.Keys.ToList()
            };
        }
    }

    /// <summary>
    /// Script statistics
    /// </summary>
    public class ScriptStats
    {
        public int TotalScripts { get; set; }
        public int TotalEvents { get; set; }
        public List<string> EventTypes { get; set; } = new List<string>();
    }
}
