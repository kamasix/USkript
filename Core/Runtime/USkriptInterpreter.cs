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

                // set_food player amount
                if (raw.StartsWith("set_food "))
                {
                    var match = Regex.Match(raw, @"^set_food\s+(\w+)\s+(\d+)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var food = byte.Parse(match.Groups[2].Value);
                        context.Player.SetFood(food);
                    }
                    return;
                }

                // set_water player amount
                if (raw.StartsWith("set_water "))
                {
                    var match = Regex.Match(raw, @"^set_water\s+(\w+)\s+(\d+)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var water = byte.Parse(match.Groups[2].Value);
                        context.Player.SetWater(water);
                    }
                    return;
                }

                // set_stamina player amount
                if (raw.StartsWith("set_stamina "))
                {
                    var match = Regex.Match(raw, @"^set_stamina\s+(\w+)\s+(\d+)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var stamina = byte.Parse(match.Groups[2].Value);
                        context.Player.SetStamina(stamina);
                    }
                    return;
                }

                // set_virus player amount
                if (raw.StartsWith("set_virus "))
                {
                    var match = Regex.Match(raw, @"^set_virus\s+(\w+)\s+(\d+)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var virus = byte.Parse(match.Groups[2].Value);
                        context.Player.SetVirus(virus);
                    }
                    return;
                }

                // kick player "reason"
                if (raw.StartsWith("kick "))
                {
                    var match = Regex.Match(raw, @"^kick\s+(\w+)\s+""(.+?)""$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var reason = ProcessString(match.Groups[2].Value, context);
                        context.Player.Kick(reason);
                    }
                    return;
                }

                // add_experience player amount
                if (raw.StartsWith("add_experience "))
                {
                    var match = Regex.Match(raw, @"^add_experience\s+(\w+)\s+(\d+)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var amount = uint.Parse(match.Groups[2].Value);
                        context.Player.AddExperience(amount);
                    }
                    return;
                }

                // set_experience player amount
                if (raw.StartsWith("set_experience "))
                {
                    var match = Regex.Match(raw, @"^set_experience\s+(\w+)\s+(\d+)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var amount = uint.Parse(match.Groups[2].Value);
                        context.Player.SetExperience(amount);
                    }
                    return;
                }

                // set_reputation player amount
                if (raw.StartsWith("set_reputation "))
                {
                    var match = Regex.Match(raw, @"^set_reputation\s+(\w+)\s+(-?\d+)$");
                    if (match.Success && match.Groups[1].Value == "player" && context.Player != null)
                    {
                        var reputation = int.Parse(match.Groups[2].Value);
                        context.Player.SetReputation(reputation);
                    }
                    return;
                }

                // heal player
                if (raw == "heal player")
                {
                    if (context.Player != null)
                    {
                        context.Player.Heal();
                    }
                    return;
                }

                // feed player
                if (raw == "feed player")
                {
                    if (context.Player != null)
                    {
                        context.Player.Feed();
                    }
                    return;
                }

                // clear_inventory player
                if (raw == "clear_inventory player")
                {
                    if (context.Player != null)
                    {
                        context.Player.ClearInventory();
                    }
                    return;
                }

                // exit_vehicle player
                if (raw == "exit_vehicle player")
                {
                    if (context.Player != null)
                    {
                        context.Player.ExitVehicle();
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

            // food player >= amount
            var foodMatch = Regex.Match(raw, @"^food\s+(\w+)\s+(>=|<=|>|<|==)\s+(\d+)$");
            if (foodMatch.Success && foodMatch.Groups[1].Value == "player" && context.Player != null)
            {
                var op = foodMatch.Groups[2].Value;
                var amount = byte.Parse(foodMatch.Groups[3].Value);
                var playerFood = context.Player.GetFood();

                return op switch
                {
                    ">=" => playerFood >= amount,
                    "<=" => playerFood <= amount,
                    ">" => playerFood > amount,
                    "<" => playerFood < amount,
                    "==" => playerFood == amount,
                    _ => false
                };
            }

            // water player >= amount
            var waterMatch = Regex.Match(raw, @"^water\s+(\w+)\s+(>=|<=|>|<|==)\s+(\d+)$");
            if (waterMatch.Success && waterMatch.Groups[1].Value == "player" && context.Player != null)
            {
                var op = waterMatch.Groups[2].Value;
                var amount = byte.Parse(waterMatch.Groups[3].Value);
                var playerWater = context.Player.GetWater();

                return op switch
                {
                    ">=" => playerWater >= amount,
                    "<=" => playerWater <= amount,
                    ">" => playerWater > amount,
                    "<" => playerWater < amount,
                    "==" => playerWater == amount,
                    _ => false
                };
            }

            // experience player >= amount
            var expMatch = Regex.Match(raw, @"^experience\s+(\w+)\s+(>=|<=|>|<|==)\s+(\d+)$");
            if (expMatch.Success && expMatch.Groups[1].Value == "player" && context.Player != null)
            {
                var op = expMatch.Groups[2].Value;
                var amount = uint.Parse(expMatch.Groups[3].Value);
                var playerExp = context.Player.GetExperience();

                return op switch
                {
                    ">=" => playerExp >= amount,
                    "<=" => playerExp <= amount,
                    ">" => playerExp > amount,
                    "<" => playerExp < amount,
                    "==" => playerExp == amount,
                    _ => false
                };
            }

            // reputation player >= amount
            var repMatch = Regex.Match(raw, @"^reputation\s+(\w+)\s+(>=|<=|>|<|==)\s+(-?\d+)$");
            if (repMatch.Success && repMatch.Groups[1].Value == "player" && context.Player != null)
            {
                var op = repMatch.Groups[2].Value;
                var amount = int.Parse(repMatch.Groups[3].Value);
                var playerRep = context.Player.GetReputation();

                return op switch
                {
                    ">=" => playerRep >= amount,
                    "<=" => playerRep <= amount,
                    ">" => playerRep > amount,
                    "<" => playerRep < amount,
                    "==" => playerRep == amount,
                    _ => false
                };
            }

            // is_in_vehicle player
            if (raw == "is_in_vehicle player")
            {
                if (context.Player != null)
                {
                    return context.Player.IsInVehicle();
                }
                return false;
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
            
            // Variables: {player.name}, {player.id}, etc.
            if (context.Player != null)
            {
                result = result.Replace("{player.name}", context.Player.Name);
                result = result.Replace("{player.id}", context.Player.Id);
                result = result.Replace("{player.displayname}", context.Player.DisplayName);
                result = result.Replace("{player.health}", context.Player.GetHealth().ToString());
                result = result.Replace("{player.food}", context.Player.GetFood().ToString());
                result = result.Replace("{player.water}", context.Player.GetWater().ToString());
                result = result.Replace("{player.experience}", context.Player.GetExperience().ToString());
                result = result.Replace("{player.reputation}", context.Player.GetReputation().ToString());
                result = result.Replace("{player.group}", context.Player.GetGroup());
                result = result.Replace("{player.position}", context.Player.GetPosition());
                result = result.Replace("{player.ping}", context.Player.GetPing().ToString("F0"));
            }

            // Event-specific variables
            result = result.Replace("{damage}", context.DamageAmount.ToString("F1"));
            result = result.Replace("{msg}", context.Message ?? "");

            return result;
        }
    }
}
