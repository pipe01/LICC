using LICC.API;
using System;

namespace LICC.Internal
{
    internal interface ICommandExecutor
    {
        object Execute(Command cmd, object[] args);
    }

    internal class CommandExecutor : ICommandExecutor
    {
        private readonly IObjectProvider ObjectProvider;

        public CommandExecutor(IObjectProvider objectProvider)
        {
            this.ObjectProvider = objectProvider;
        }

        public object Execute(Command cmd, object[] args)
        {
            return cmd.Method.Invoke(null, args);
        }
    }
}
