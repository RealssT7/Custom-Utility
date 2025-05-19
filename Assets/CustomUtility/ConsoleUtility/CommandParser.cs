using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtility.ConsoleUtility.Helper;
using UnityEngine;

namespace CustomUtility.ConsoleUtility.Editor
{
    /// <summary>
    ///     A static class responsible for parsing and executing console commands.
    /// </summary>
    public static class CommandParser
    {
        /// <summary>
        ///     The name of the command currently awaiting target selection.
        /// </summary>
        private static string _pendingCommandName;

        /// <summary>
        ///     The list of potential target objects for the pending command.
        /// </summary>
        private static List<object> _pendingTargets;

        /// <summary>
        ///     The arguments for the pending command.
        /// </summary>
        private static object[] _pendingArgs;

        /// <summary>
        ///     Executes a console command based on the provided input string.
        /// </summary>
        /// <param name="input">The input string containing the command and arguments.</param>
        /// <returns>A string indicating the result of the command execution.</returns>
        public static string Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "No Command.";

            input = input.Trim().ToLower();

            if (!IsAwaitingTargetSelection()) 
                return HandleCommandInput(input);
            
            return input is not "cancel" ? HandleSelectionInput(input) : ResetParser();
        }

        /// <summary>
        ///     Resets the parser state, clearing any pending command or target information.
        /// </summary>
        public static string ResetParser()
        {
            _pendingArgs = null;
            _pendingCommandName = null;

            if (_pendingTargets != null)
                foreach (var target in _pendingTargets.OfType<Component>())
                {
                    var indicator = target.GetComponentInChildren<CommandTargetIndicator>();
                    indicator?.DestroyLabel();
                }

            _pendingTargets = null;
            return "Reset Operation.";
        }

        /// <summary>
        ///     Checks if the parser is currently awaiting target selection for a command.
        /// </summary>
        /// <returns>True if awaiting target selection, otherwise false.</returns>
        private static bool IsAwaitingTargetSelection()
        {
            return _pendingCommandName != null;
        }

        /// <summary>
        ///     Handles input for selecting a target when multiple targets are available.
        /// </summary>
        /// <param name="input">The input string containing the target selection.</param>
        /// <returns>A string indicating the result of the selection process.</returns>
        private static string HandleSelectionInput(string input)
        {
            return int.TryParse(input.Trim(), out var selectionIndex)
                ? SelectTarget(selectionIndex - 1)
                : $"Invalid input. Please enter a number between 1 and {_pendingTargets.Count}.";
        }

        /// <summary>
        ///     Handles input for executing a command.
        /// </summary>
        /// <param name="input">The input string containing the command and arguments.</param>
        /// <returns>A string indicating the result of the command execution.</returns>
        private static string HandleCommandInput(string input)
        {
            var tokens = input.Split(' ');
            var commandName = tokens[0].ToLower();

            if (!CommandRegistry.Commands.TryGetValue(commandName, out var info))
                return $"Unknown command: {commandName}";

            var args = tokens.Skip(1).ToArray();
            if (!TryParseArguments(info, args, out var parsedArgs, out var error))
                return error;

            return info.Method.IsStatic
                ? InvokeCommand(info, parsedArgs)
                : HandleInstanceCommand(commandName, info, parsedArgs);
        }

        /// <summary>
        ///     Invokes a command method with the provided arguments.
        /// </summary>
        /// <param name="info">The command information.</param>
        /// <param name="args">The arguments for the command.</param>
        /// <returns>A string indicating the result of the command execution.</returns>
        private static string InvokeCommand(CommandRegistry.CommandInfo info, object[] args)
        {
            try
            {
                var result = info.Method.Invoke(info.Target, args);
                return result?.ToString() ?? "Command executed.";
            }
            catch (Exception ex)
            {
                return $"Command failed: {ex.InnerException?.Message ?? ex.Message}";
            }
        }

        /// <summary>
        ///     Handles the execution of an instance command, including target selection if necessary.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="info">The command information.</param>
        /// <param name="args">The arguments for the command.</param>
        /// <returns>A string indicating the result of the command execution.</returns>
        private static string HandleInstanceCommand(string commandName, CommandRegistry.CommandInfo info, object[] args)
        {
            var targets = info.TargetGetter?.Invoke()?.ToList();
            if (targets == null || targets.Count == 0)
                return "No valid targets found.";

            if (targets.Count != 1) 
                return HandleMultipleTargets(commandName, args, targets);

            info.Target = targets[0];
            return InvokeCommand(info, args);
        }

        /// <summary>
        ///     Handles the case where multiple targets are available for a command.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="args">The arguments for the command.</param>
        /// <param name="targets">The list of potential target objects.</param>
        /// <returns>A string listing the available targets.</returns>
        private static string HandleMultipleTargets(string commandName, object[] args, List<object> targets)
        {
            _pendingCommandName = commandName;
            _pendingArgs = args;
            _pendingTargets = targets;

            return ListAvailableTargets(targets);
        }

        /// <summary>
        ///     Selects a target from the list of available targets based on the provided index.
        /// </summary>
        /// <param name="index">The index of the selected target.</param>
        /// <returns>A string indicating the result of the selection process.</returns>
        private static string SelectTarget(int index)
        {
            if (_pendingTargets == null)
                return "No pending targets available.";

            var max = _pendingTargets.Count;
            if (index < 0 || index >= max)
                return $"Invalid selection. Please choose a number between 1 and {max}.";

            if (!CommandRegistry.Commands.TryGetValue(_pendingCommandName, out var info))
            {
                ResetParser();
                return $"Command '{_pendingCommandName}' not found.";
            }

            try
            {
                var result = info.Method.Invoke(_pendingTargets[index], _pendingArgs);
                return result?.ToString() ?? "Command executed successfully.";
            }
            catch (Exception ex)
            {
                return $"Command failed: {ex.InnerException?.Message ?? ex.Message}";
            }
            finally
            {
                ResetParser();
            }
        }

        /// <summary>
        ///     Attempts to parse the arguments for a command.
        /// </summary>
        /// <param name="info">The command information.</param>
        /// <param name="args">The input arguments as strings.</param>
        /// <param name="parsedArgs">The parsed arguments as objects.</param>
        /// <param name="error">An error message if parsing fails.</param>
        /// <returns>True if parsing is successful, otherwise false.</returns>
        private static bool TryParseArguments(CommandRegistry.CommandInfo info, string[] args, out object[] parsedArgs,
            out string error)
        {
            parsedArgs = new object[info.Parameters.Length];
            error = null;

            if (args.Length != info.Parameters.Length)
            {
                error =
                    $"Argument mismatch. Expected: ({DescribeParameters(info)}). Provided: ({string.Join(", ", args.Select(a => $"'{a}'"))})";
                return false;
            }

            for (var i = 0; i < args.Length; i++)
                try
                {
                    parsedArgs[i] = ConvertArg(args[i], info.Parameters[i].ParameterType);
                }
                catch (Exception ex)
                {
                    error = $"Invalid argument for '{info.Parameters[i].Name}': {ex.Message}";
                    return false;
                }

            return true;
        }

        /// <summary>
        ///     Describes the parameters of a command method.
        /// </summary>
        /// <param name="info">The command information.</param>
        /// <returns>A string describing the parameters.</returns>
        private static string DescribeParameters(CommandRegistry.CommandInfo info)
        {
            return string.Join(", ", info.Parameters.Select(p => $"{p.Name}:{p.ParameterType.Name}"));
        }

        /// <summary>
        ///     Lists the available targets for a command.
        /// </summary>
        /// <param name="targets">The list of potential target objects.</param>
        /// <returns>A string listing the available targets.</returns>
        private static string ListAvailableTargets(List<object> targets)
        {
            var output = "Multiple targets found. Please choose one using: select <number>\n\n";
            for (var i = 0; i < targets.Count; i++)
                output += targets[i] switch
                {
                    Component comp => SetupIndicator(comp, i),
                    _ => $"{i + 1}. {targets[i].GetType().Name} - {targets[i]}\n"
                };

            return output.TrimEnd();
        }

        /// <summary>
        ///     Sets up a visual indicator for a target component.
        /// </summary>
        /// <param name="comp">The target component.</param>
        /// <param name="index">The index of the target.</param>
        /// <returns>A string describing the target.</returns>
        private static string SetupIndicator(Component comp, int index)
        {
            var indicator = comp.GetComponent<CommandTargetIndicator>() ??
                            comp.gameObject.AddComponent<CommandTargetIndicator>();

            indicator.Index = index + 1;
            return $"{index + 1}. {comp.GetType().Name} on GameObject '{comp.gameObject.name}'\n";
        }

        /// <summary>
        ///     Converts a string argument to the specified type.
        /// </summary>
        /// <param name="arg">The string argument.</param>
        /// <param name="type">The target type.</param>
        /// <returns>The converted argument as an object.</returns>
        private static object ConvertArg(string arg, Type type)
        {
            return type switch
            {
                _ when type == typeof(string) => arg,
                _ when type == typeof(int) => int.Parse(arg),
                _ when type == typeof(float) => float.Parse(arg),
                _ when type == typeof(bool) => bool.Parse(arg),
                _ when type == typeof(Vector3) => ParseVector3(arg),
                _ => throw new NotSupportedException($"Unsupported parameter type: {type.Name}")
            };
        }

        /// <summary>
        ///     Parses a string into a Vector3 object.
        /// </summary>
        /// <param name="arg">The string in the format "x,y,z".</param>
        /// <returns>A Vector3 object.</returns>
        /// <exception cref="FormatException">Thrown if the string is not in the correct format.</exception>
        private static Vector3 ParseVector3(string arg)
        {
            var parts = arg.Split(',');
            if (parts.Length != 3)
                throw new FormatException("Vector3 must be in the format x,y,z");

            return new Vector3(
                float.Parse(parts[0]),
                float.Parse(parts[1]),
                float.Parse(parts[2]));
        }
    }
}