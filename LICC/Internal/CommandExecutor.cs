using System;
using System.Collections.Generic;

namespace LICC.Internal
{
    internal interface ICommandExecutor
    {
        object Execute(Command cmd, object[] args);
    }

    public delegate object ObjectProviderDelegate(Type type);

    internal class CommandExecutor : ICommandExecutor
    {
        private readonly ObjectProviderDelegate ObjectProvider;

        public CommandExecutor(ObjectProviderDelegate objectProvider)
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
                            newArgs[i] = ObjectProvider(injectedParam.Param.ParameterType);
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

            object instance = null;

            if (!cmd.Method.IsStatic)
            {
                try
                {
                    instance = ObjectProvider(cmd.InstanceType);
                }
                catch (Exception ex)
                {
                    throw new KeyNotFoundException($"Failed to get object instance for command {cmd.Name}", ex);
                }
            }

            return cmd.Method.Invoke(instance, args);
        }
    }
}
