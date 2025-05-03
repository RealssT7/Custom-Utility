using System;
using System.Linq;
using Vector3 = System.Numerics.Vector3;

namespace  CustomUtility.ConsoleUtility.Editor
{
    public static class CommandParser
    {
        public static string Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "No command.";

            var tokens = input.Split(' ');
            var commandName = tokens[0].ToLower();

            if (!CommandRegistry.Commands.TryGetValue(commandName, out var info))
                return $"Unknown command: {commandName}";
            
            var args = tokens.Skip(1).ToArray();
            var parameters = info.Parameters;

            if (args.Length != parameters.Length)
                return $"Usage: {info.Name} ({string.Join(", ", parameters.Select(p => p.ParameterType.Name))})";

            object[] parsedArgs = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    parsedArgs[i] = ConvertArg(args[i], parameters[i].ParameterType);
                }
                catch (Exception ex)
                {
                    return $"Invalid argument for '{parameters[i].Name}': {ex.Message}";
                }
            }

            try
            {
                var result = info.Method.Invoke(info.Target, parsedArgs);
                return result?.ToString() ?? "Command executed.";
            }
            catch (Exception ex)
            {
                return $"Command failed: {ex.InnerException?.Message ?? ex.Message}";
            }
        }

        private static object ConvertArg(string arg, Type type)
        {
            if (type == typeof(string)) return arg;
            if (type == typeof(int)) return int.Parse(arg);
            if (type == typeof(float)) return float.Parse(arg);
            if (type == typeof(bool)) return bool.Parse(arg);
            if (type == typeof(Vector3))
            {
                var parts = arg.Split(',');
                if (parts.Length != 3)
                    throw new FormatException("Vector3 must be in the format x,y,z");

                return new Vector3(
                    float.Parse(parts[0]),
                    float.Parse(parts[1]),
                    float.Parse(parts[2]));
            }

            // You can extend this for more types like Vector2, Color, enums, etc.
            throw new NotSupportedException($"Unsupported parameter type: {type.Name}");
        }
    }
}