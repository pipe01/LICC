using System;

namespace LICC
{
    /// <summary>
    /// Marks a method to be used as a command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// The command name (e.g. "help").
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The command's description that will be printed when using the "help" command.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If the method is an instance method, this type will be fetched from the <see cref="API.IObjectProvider"/> instead of the method's declaring type.
        /// </summary>
        public Type ProviderType { get; set; }

        /// <summary>
        /// Marks this method as a command whose name will be derived from the method's name.
        /// </summary>
        public CommandAttribute()
        {
        }

        /// <summary>
        /// Marks this method as a command.
        /// </summary>
        /// <param name="name">The command's name (e.g. "help").</param>
        public CommandAttribute(string name)
        {
            this.Name = name;
        }
    }
}
