using System;

namespace LICC.Exceptions
{
    internal sealed class CommandNotFoundException : Exception
    {
        public string CommandName { get; }

        public override string Message { get; }

        public CommandNotFoundException(string commandName, int argsCount)
        {
            this.CommandName = commandName;

            this.Message = $"No command found with name '{CommandName}' and '{argsCount}' arguments";
        }
    }
}
