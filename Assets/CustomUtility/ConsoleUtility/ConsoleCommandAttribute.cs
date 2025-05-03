using System;

namespace CustomUtility.ConsoleUtility.Attributes
{
    /// <summary>
    /// Attribute to mark a method as a console command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string CommandName { get; }
        public string CommandDescription { get; }

        public ConsoleCommandAttribute(string command, string commandDescription = "")
        {
            CommandName = command;
            CommandDescription = commandDescription;
        }
    }
}