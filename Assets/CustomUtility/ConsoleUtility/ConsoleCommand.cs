using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using CustomUtility.ConsoleUtility.Attributes;

namespace CustomUtility.ConsoleUtility
{
    public static class ConsoleCommands
    {
        private const int CommandColumnWidth = 35;
        private const string SeparatorLine = "--------------------------------------------------------------------------------";
        private const string CommandHeader = "Command (Params)";
        private const string DescriptionHeader = "Description";

        [ConsoleCommand("help", "Lists all available console commands.")]
        public static string Help()
        {
            var commands = CommandRegistry.Commands.Values.OrderBy(c => c.Name);
            var sb = new StringBuilder();

            sb.AppendLine("Available Commands:");
            sb.AppendLine(SeparatorLine);
            sb.AppendLine($"{Pad(CommandHeader, CommandColumnWidth)} {DescriptionHeader}");
            sb.AppendLine(SeparatorLine);

            foreach (var cmd in commands)
            {
                var paramList = string.Join(", ", cmd.Parameters.Select(p => SimplifyType(p.ParameterType)));
                var commandWithParams = paramList.Length > 0 ? $"{cmd.Name}({paramList})" : cmd.Name;
                sb.AppendLine($"{commandWithParams,-CommandColumnWidth} {cmd.Description}");
            }

            return sb.ToString();
        }

        // Helper method to pad the text to the required width
        private static string Pad(string text, int width)
        {
            return text.Length >= width ? text[..width] : text.PadRight(width);
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
                { typeof(Color), "Color" },
            };

            return typeMap.TryGetValue(type, out var simplified) ? simplified : type.Name;
        }

        [ConsoleCommand("clear", "Clears the console output.")]
        public static string Clear()
        {
            //DevConsoleEvents.OnClearConsole?.Invoke();
            return "Console cleared.";
        }

        [ConsoleCommand("echo", "Echoes the input text back.")]
        public static string Echo(string text)
        {
            return text;
        }

        [ConsoleCommand("teleport", "Teleports the player to a given position (x y z).")]
        public static string Teleport(float x, float y, float z)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
                return "Player not found.";

            player.transform.position = new Vector3(x, y, z);
            return $"Teleported player to ({x}, {y}, {z})";
        }

        [ConsoleCommand("get_position", "Gets the player's current position.")]
        public static string GetPosition()
        {
            var player = GameObject.FindWithTag("Player");
            return player == null ? "Player not found." : $"Player position: {player.transform.position}";
        }
    }
}
