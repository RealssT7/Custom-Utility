using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomUtility.ConsoleUtility
{
    public static class CommandRegistry
    {
        public class CommandInfo
        {
            public string Name;
            public string Description;
            public MethodInfo Method;
            public ParameterInfo[] Parameters;
            public object Target;
        }

        public static readonly Dictionary<string, CommandInfo> Commands = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void RegisterAllCommands()
        {
            Commands.Clear();

            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(asm => asm.GetTypes()))
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attr = method.GetCustomAttribute<Attributes.ConsoleCommandAttribute>();
                    if (attr != null)
                    {
                        RegisterCommand(method, null, attr);
                        Debug.Log($"Registering static command: {attr.CommandName} from {type.Name}");
                    }
                }
            }

            foreach (var mono in Object.FindObjectsOfType<MonoBehaviour>(true))
            {
                var type = mono.GetType();
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attr = method.GetCustomAttribute<Attributes.ConsoleCommandAttribute>();
                    if (attr != null)
                    {
                        RegisterCommand(method, mono, attr);
                        Debug.Log($"Registering instance command: {attr.CommandName} from {type.Name}");
                    }
                }
            }
        }

        private static void RegisterCommand(MethodInfo method, object target, Attributes.ConsoleCommandAttribute attr)
        {
            string key = attr.CommandName.ToLower();
            if (Commands.ContainsKey(key)) return;

            Commands[key] = new CommandInfo
            {
                Name = attr.CommandName,
                Description = attr.CommandDescription,
                Method = method,
                Parameters = method.GetParameters(),
                Target = target
            };
        }
    }
}
