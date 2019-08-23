using System;

namespace LICC.Exceptions
{
    internal sealed class CommandNotFoundException : Exception
    {
        public string CommandName { get; }

        public override string Message => $"No command found with name '{CommandName}'";

        public CommandNotFoundException(string commandName)
        {
            this.CommandName = commandName;
        }
    }
}
