using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

namespace CustomUtility.ConsoleUtility.Editor
{
    /// <summary>
    /// A static class responsible for managing and registering console commands.
    /// </summary>
    public static class CommandRegistry
    {
        /// <summary>
        /// Represents information about a registered console command.
        /// </summary>
        public class CommandInfo
        {
            /// <summary>
            /// The name of the command.
            /// </summary>
            public string Name;

            /// <summary>
            /// A brief description of the command.
            /// </summary>
            public string Description;

            /// <summary>
            /// The method associated with the command.
            /// </summary>
            public MethodInfo Method;

            /// <summary>
            /// The parameters required by the command method.
            /// </summary>
            public ParameterInfo[] Parameters;

            /// <summary>
            /// The target object on which the command method is invoked (if applicable).
            /// </summary>
            public object Target;

            /// <summary>
            /// A function to retrieve potential target objects for instance methods.
            /// </summary>
            public Func<IEnumerable<object>> TargetGetter;
        }

        /// <summary>
        /// A dictionary containing all registered commands, keyed by their lowercase names.
        /// </summary>
        public static readonly Dictionary<string, CommandInfo> Commands = new();
        
        public static void Initialize(ConsoleSettings config)
        {
            Commands.Clear();            
            if (config.Mode == ConsoleSettings.ExecutionMode.Reflection) RegisterViaReflection();
            else CommandRegistered.Register();
        }

        private static void RegisterViaReflection()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes());

            foreach (var type in allTypes)
            {
                RegisterCommandsFromType(
                    type,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    () => null);

                RegisterCommandsFromType(
                    type,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    () => Object.FindObjectsOfType(type));
            }
        }
        
        /// <summary>
        /// Registers all methods marked with the ConsoleCommandAttribute from a given type.
        /// </summary>
        /// <param name="type">The type to scan for command methods.</param>
        /// <param name="bindingFlags">The binding flags to use when searching for methods.</param>
        /// <param name="targetGetter">A function to retrieve potential target objects for instance methods.</param>
        private static void RegisterCommandsFromType(Type type, BindingFlags bindingFlags, Func<IEnumerable<object>> targetGetter)
        {
            foreach (var method in type.GetMethods(bindingFlags))
            {
                var attribute = method.GetCustomAttribute<Attributes.ConsoleCommandAttribute>();
                if (attribute == null) continue;

                RegisterCommand(
                    attribute.CommandName,
                    attribute.CommandDescription,
                    method,
                    null,
                    bindingFlags.HasFlag(BindingFlags.Instance) ? () => Object.FindObjectsOfType(type) : targetGetter
                );
            }
        }

        /// <summary>
        /// Registers a single console command.
        /// </summary>
        /// <param name="command">The name of the command.</param>
        /// <param name="description">A brief description of the command.</param>
        /// <param name="method">The method associated with the command.</param>
        /// <param name="target">The target object for the command (if applicable).</param>
        /// <param name="targetGetter">A function to retrieve potential target objects for instance methods.</param>
        internal static void RegisterCommand(string command, string description, MethodInfo method, object target, Func<IEnumerable<object>> targetGetter)
        {
            // Convert the command name to lowercase for consistent key storage.
            var key = command.ToLower();
            if (Commands.ContainsKey(key)) return;

            // Add the command to the dictionary.
            Commands[key] = new CommandInfo
            {
                Name = command,
                Description = description,
                Method = method,
                Parameters = method.GetParameters(),
                Target = target,
                TargetGetter = targetGetter
            };
        }
    }
}