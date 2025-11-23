using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using USkript.Core.Models;

namespace USkript.Core.Runtime
{
    /// <summary>
    /// Interpreter executing USkript scripts
    /// </summary>
    public class USkriptInterpreter
    {
        private readonly IEnvironmentAdapter _environment;

        public USkriptInterpreter(IEnvironmentAdapter environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        /// Executes event
        /// </summary>
        public async Task ExecuteEvent(EventNode eventNode, SkriptContext context)
        {
            context.Environment = _environment;
            context.EventData = eventNode.EventData;

            try
            {
                await ExecuteBlock(eventNode.Body, context);
            }
            catch (Exception ex)
            {
                _environment.LogError($"Error executing event {eventNode.EventName} at line {eventNode.LineNumber}", ex);
            }
        }

        /// <summary>
        /// Executes code block (list of nodes)
        /// </summary>
        public async Task ExecuteBlock(List<Node> nodes, SkriptContext context)
        {
            foreach (var node in nodes)
            {
                // If event was cancelled, some actions may be skipped
                // but for now we execute everything

                if (node is ActionNode actionNode)
                {
                    await ExecuteAction(actionNode, context);
                }
                else if (node is IfNode ifNode)
                {
                    await ExecuteIf(ifNode, context);
                }
            }
        }

        /// <summary>
        /// Executes action
        /// </summary>
        private async Task ExecuteAction(Models.ActionNode actionNode, SkriptContext context)
        {
            var raw = actionNode.ActionRaw.Trim();

            try
            {
                // cancel
                if (raw == "cancel")
                {
                    context.Cancelled = true;
                    return;
                }

                // message player "tekst"
                if (raw.StartsWith("message "))
                {
                    var match = Regex.Match(raw, @"^message\s+(\w+)\s+""(.+?)""$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var message = ProcessString(match.Groups[2].Value, context);
                        context.Player.SendMessage(message);
                    }
                    return;
                }

                // broadcast "tekst"
                if (raw.StartsWith("broadcast "))
                {
                    var match = Regex.Match(raw, @"^broadcast\s+""(.+?)""$");
                    if (match.Success)
                    {
                        var message = ProcessString(match.Groups[1].Value, context);
                        _environment.Broadcast(message);
                    }
                    return;
                }

                // give player "ItemId" amount
                if (raw.StartsWith("give "))
                {
                    var match = Regex.Match(raw, @"^give\s+(\w+)\s+""(.+?)""\s+(\d+)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var itemId = match.Groups[2].Value;
                        var amount = int.Parse(match.Groups[3].Value);
                        context.Player.GiveItem(itemId, amount);
                    }
                    return;
                }

                // teleport player "location"
                if (raw.StartsWith("teleport "))
                {
                    var match = Regex.Match(raw, @"^teleport\s+(\w+)\s+""(.+?)""$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var location = match.Groups[2].Value;
                        context.Player.Teleport(location);
                    }
                    return;
                }

                // add_money player amount
                if (raw.StartsWith("add_money "))
                {
                    var match = Regex.Match(raw, @"^add_money\s+(\w+)\s+(\d+(?:\.\d+)?)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var amount = decimal.Parse(match.Groups[2].Value);
                        context.Player.AddMoney(amount);
                    }
                    return;
                }

                // set_money player amount
                if (raw.StartsWith("set_money "))
                {
                    var match = Regex.Match(raw, @"^set_money\s+(\w+)\s+(\d+(?:\.\d+)?)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var amount = decimal.Parse(match.Groups[2].Value);
                        context.Player.SetMoney(amount);
                    }
                    return;
                }

                // set_health player amount
                if (raw.StartsWith("set_health "))
                {
                    var match = Regex.Match(raw, @"^set_health\s+(\w+)\s+(\d+)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var health = byte.Parse(match.Groups[2].Value);
                        context.Player.SetHealth(health);
                    }
                    return;
                }

                // kill player
                if (raw == "kill player")
                {
                    if (context.Player != null)
                    {
                        context.Player.Kill();
                    }
                    return;
                }

                // run_command "command"
                if (raw.StartsWith("run_command "))
                {
                    var match = Regex.Match(raw, @"^run_command\s+""(.+?)""$");
                    if (match.Success)
                    {
                        var command = ProcessString(match.Groups[1].Value, context);
                        await _environment.RunCommand(command);
                    }
                    return;
                }

                // Unknown action
                _environment.LogError($"Unknown action at line {actionNode.LineNumber}: {raw}");
            }
            catch (Exception ex)
            {
                _environment.LogError($"Error executing action at line {actionNode.LineNumber}: {raw}", ex);
            }
        }

        /// <summary>
        /// Executes if
        /// </summary>
        private async Task ExecuteIf(IfNode ifNode, SkriptContext context)
        {
            try
            {
                var conditionResult = EvaluateCondition(ifNode.ConditionRaw, context);

                if (conditionResult)
                {
                    await ExecuteBlock(ifNode.ThenBody, context);
                }
                else if (ifNode.ElseBody.Count > 0)
                {
                    await ExecuteBlock(ifNode.ElseBody, context);
                }
            }
            catch (Exception ex)
            {
                _environment.LogError($"Error executing if at line {ifNode.LineNumber}: {ifNode.ConditionRaw}", ex);
            }
        }

        /// <summary>
        /// Evaluates condition
        /// </summary>
        public bool EvaluateCondition(string conditionRaw, SkriptContext context)
        {
            var raw = conditionRaw.Trim();

            // startswith msg "text"
            var startsWithMatch = Regex.Match(raw, @"^startswith\s+(\w+)\s+""(.+?)""$");
            if (startsWithMatch.Success)
            {
                var varName = startsWithMatch.Groups[1].Value;
                var value = startsWithMatch.Groups[2].Value;
                
                if (varName == "msg" && context.Message != null)
                {
                    return context.Message.StartsWith(value, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }

            // equals msg "text"
            var equalsMatch = Regex.Match(raw, @"^equals\s+(\w+)\s+""(.+?)""$");
            if (equalsMatch.Success)
            {
                var varName = equalsMatch.Groups[1].Value;
                var value = equalsMatch.Groups[2].Value;
                
                if (varName == "msg" && context.Message != null)
                {
                    return context.Message.Equals(value, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }

            // has_permission player "perm.name"
            var permMatch = Regex.Match(raw, @"^has_permission\s+(\w+)\s+""(.+?)""$");
            if (permMatch.Success && permMatch.Groups[1].Value == "player" && context.Player != null)
            {
                var permission = permMatch.Groups[2].Value;
                return context.Player.HasPermission(permission);
            }

            // money player >= amount
            var moneyMatch = Regex.Match(raw, @"^money\s+(\w+)\s+(>=|<=|>|<|==)\s+(\d+(?:\.\d+)?)$");
            if (moneyMatch.Success && moneyMatch.Groups[1].Value == "player" && context.Player != null)
            {
                var op = moneyMatch.Groups[2].Value;
                var amount = decimal.Parse(moneyMatch.Groups[3].Value);
                var playerMoney = context.Player.GetMoney();

                return op switch
                {
                    ">=" => playerMoney >= amount,
                    "<=" => playerMoney <= amount,
                    ">" => playerMoney > amount,
                    "<" => playerMoney < amount,
                    "==" => playerMoney == amount,
                    _ => false
                };
            }

            // health player >= amount
            var healthMatch = Regex.Match(raw, @"^health\s+(\w+)\s+(>=|<=|>|<|==)\s+(\d+)$");
            if (healthMatch.Success && healthMatch.Groups[1].Value == "player" && context.Player != null)
            {
                var op = healthMatch.Groups[2].Value;
                var amount = byte.Parse(healthMatch.Groups[3].Value);
                var playerHealth = context.Player.GetHealth();

                return op switch
                {
                    ">=" => playerHealth >= amount,
                    "<=" => playerHealth <= amount,
                    ">" => playerHealth > amount,
                    "<" => playerHealth < amount,
                    "==" => playerHealth == amount,
                    _ => false
                };
            }

            _environment.LogError($"Unknown condition: {raw}");
            return false;
        }

        /// <summary>
        /// Processes string - replaces variables {player.name}, {player.id}, etc.
        /// </summary>
        private string ProcessString(string text, SkriptContext context)
        {
            var result = text;

            // OpenMod/Rocket colors: &a, &e, &f etc. - leave as is
            
            // Variables: {player.name}, {player.id}
            if (context.Player != null)
            {
                result = result.Replace("{player.name}", context.Player.Name);
                result = result.Replace("{player.id}", context.Player.Id);
                result = result.Replace("{player.displayname}", context.Player.DisplayName);
            }

            return result;
        }
    }
}
