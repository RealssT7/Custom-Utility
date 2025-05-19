using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomUtility.ConsoleUtility.Attributes;
using CustomUtility.ConsoleUtility.Editor;
using UnityEngine;

namespace CustomUtility.ConsoleUtility
{
    public static class DefaultConsoleCommand
    {
        #region HelpCommand
        
        [ConsoleCommand("help", "Lists all available console commands.")]
        public static string Help()
        {
            var commands = CommandRegistry.Commands.Values.OrderBy(c => c.Name);
            var sb = new StringBuilder();

            sb.AppendLine("Available Commands:");
            sb.Append(new string('=', 45));

            foreach (var cmd in commands)
            {
                var paramList = string.Join(", ", cmd.Parameters.Select(p => SimplifyType(p.ParameterType)));
                var commandSignature = paramList.Length > 0 ? $"{cmd.Name}({paramList})" : cmd.Name;

                sb.AppendLine();
                sb.AppendLine(commandSignature);
                sb.AppendLine($"    {cmd.Description}");
                sb.Append(new string('-', 45));
            }

            return sb.ToString();
        }

        private static string SimplifyType(Type type)
        {
            var typeMap = new Dictionary<Type, string>
            {
                { typeof(string), "String" },
                { typeof(int), "Int" },
                { typeof(float), "Float" },
                { typeof(bool), "Bool" },
                { typeof(double), "Double" },
                { typeof(object), "Object" },
                { typeof(Vector2), "Vector2" },
                { typeof(Vector3), "Vector3" },
                { typeof(Quaternion), "Quaternion" },
                { typeof(Color), "Color" }
            };

            return typeMap.TryGetValue(type, out var simplified) ? simplified : type.Name;
        }

        #endregion

        [ConsoleCommand("echo", "Prints the input text back.")]
        public static string Echo(string text)
        {
            return text;
        }

        [ConsoleCommand("player_teleport", "Teleports the player to a given position (x y z).")]
        public static string Teleport(Vector3 position)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return "Player not found.";

            player.transform.position = new Vector3(position.x, position.y, position.z);
            return $"Teleported player to ({position.x}, {position.y}, {position.z})";
        }

        [ConsoleCommand("player_position", "Gets the player's current position.")]
        public static string GetPosition()
        {
            var player = GameObject.FindWithTag("Player");
            return player == null ? "Player not found." : $"Player position: {player.transform.position}";
        }
    }
}