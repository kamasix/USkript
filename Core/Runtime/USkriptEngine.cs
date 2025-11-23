using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using USkript.Core.Models;

namespace USkript.Core.Runtime
{
    /// <summary>
    /// Main USkript engine - combines ScriptManager and Interpreter
    /// </summary>
    public class USkriptEngine
    {
        private readonly ScriptManager _scriptManager;
        private readonly USkriptInterpreter _interpreter;
        private readonly IEnvironmentAdapter _environment;

        public USkriptEngine(IEnvironmentAdapter environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _scriptManager = new ScriptManager();
            _interpreter = new USkriptInterpreter(_environment);
        }

        /// <summary>
        /// Script manager (for diagnostic purposes)
        /// </summary>
        public ScriptManager ScriptManager => _scriptManager;

        /// <summary>
        /// Loads/reloads all scripts from folder
        /// </summary>
        public void Reload(string scriptsDirectory)
        {
            try
            {
                _scriptManager.LoadScripts(scriptsDirectory);
                
                var stats = _scriptManager.GetStats();
                _environment.Log($"USkript: Loaded {stats.TotalScripts} script(s) with {stats.TotalEvents} event(s)");
                
                if (stats.EventTypes.Count > 0)
                {
                    _environment.Log($"USkript: Event types: {string.Join(", ", stats.EventTypes)}");
                }
            }
            catch (Exception ex)
            {
                _environment.LogError("Failed to reload USkript scripts", ex);
                throw;
            }
        }

        /// <summary>
        /// Raises event - executes all handlers for given event
        /// </summary>
        public async Task RaiseEvent(string eventName, SkriptContext context)
        {
            var events = _scriptManager.GetEvents(eventName);

            foreach (var eventNode in events)
            {
                await _interpreter.ExecuteEvent(eventNode, context);
                
                // If any handler cancelled the event, we can stop further execution
                // (optional - could execute all handlers)
                // if (context.Cancelled) break;
            }
        }

        /// <summary>
        /// Raises event synchronously (wrapper for compatibility)
        /// </summary>
        public void RaiseEventSync(string eventName, SkriptContext context)
        {
            Task.Run(async () => await RaiseEvent(eventName, context)).Wait();
        }
    }
}
