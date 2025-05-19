#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using CustomUtility.ConsoleUtility.Attributes;

[InitializeOnLoad]
public static class CommandRegister
{
    static CommandRegister() => GenerateRegisteredCommand();

    private static void GenerateRegisteredCommand()
    {
        const string path = "Assets/Scripts/Generated/CommandRegistered.cs";
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var methods = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.FullName.StartsWith("Unity"))
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            .Where(m => m.GetCustomAttribute<ConsoleCommandAttribute>() != null)
            .ToList();

        var code = new StringBuilder();
        code.AppendLine("using System.Reflection;");
        code.AppendLine("using UnityEngine;");
        code.AppendLine("using CustomUtility.ConsoleUtility.Editor;");
        code.AppendLine();
        code.AppendLine("public static class CommandRegistered");
        code.AppendLine("{");
        code.AppendLine("    public static void Register()");
        code.AppendLine("    {");

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<ConsoleCommandAttribute>();
            var declaringType = method.DeclaringType;
            var isStatic = method.IsStatic;

            if (declaringType == null) continue;
            var targetExpr = isStatic ? "null" : $"Object.FindObjectOfType(typeof({declaringType.FullName}))";
            var targetGetterExpr = isStatic 
                ? "() => System.Linq.Enumerable.Empty<object>()" 
                : $"() => UnityEngine.Object.FindObjectsOfType(typeof({declaringType.FullName}))";

            code.AppendLine("    CommandRegistry.RegisterCommand(");
            code.AppendLine($"        \"{attr.CommandName}\",");
            code.AppendLine($"        \"{attr.CommandDescription}\",");
            code.AppendLine($"        typeof({declaringType.FullName}).GetMethod(\"{method.Name}\", BindingFlags.{(isStatic ? "Static" : "Instance")} | BindingFlags.Public | BindingFlags.NonPublic),");
            code.AppendLine($"        {targetExpr},");
            code.AppendLine($"        {targetGetterExpr});");
        }

        code.AppendLine("    }");
        code.AppendLine("}");
        File.WriteAllText(path, code.ToString());
        AssetDatabase.Refresh();
    }
}
#endif
