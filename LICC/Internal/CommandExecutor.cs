using LICC.API;
using System;
using System.Linq;

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
            if (cmd.InjectedParameters.Length > 0)
            {
                var newArgs = new object[args.Length + cmd.InjectedParameters.Length];

                if (ObjectProvider != null)
                {
                    int offset = 0;

                    for (int i = 0; i < newArgs.Length; i++)
                    {
                        var injectedParam = Array.Find(cmd.InjectedParameters, o => o.Index == i);

                        if (injectedParam != default)
                        {
                            newArgs[i] = ObjectProvider.Get(injectedParam.Param.ParameterType);
                            offset++;
                        }
                        else
                        {
                            newArgs[i] = args[i - offset];
                        }
                    }
                }

                args = newArgs;
            }

            return cmd.Method.Invoke(null, args);
        }
    }
}
