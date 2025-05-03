using System;

namespace CustomUtility.ConsoleUtility.Attributes
{
    /// <summary>
    /// Attribute to mark a method as a console command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)] // Specifies that this attribute can only be applied to methods.
    public class ConsoleCommandAttribute : Attribute
    {
        /// <summary>
        /// The name of the console command.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// A brief description of the console command.
        /// </summary>
        public string CommandDescription { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCommandAttribute"/> class.
        /// </summary>
        /// <param name="command">The name of the console command.</param>
        /// <param name="commandDescription">A brief description of the console command (optional).</param>
        public ConsoleCommandAttribute(string command, string commandDescription = "")
        {
            CommandName = command;
            CommandDescription = commandDescription;
        }
    }
}