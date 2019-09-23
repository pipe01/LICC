using LICC.API;
using LICC.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LICC.Internal
{
    internal interface ICommandRegistryInternal : ICommandRegistry
    {
        IEnumerable<Command> GetCommands();
        bool TryGetCommand(string name, int argCount, out Command cmd);
        bool TryGetCommand(string name, int argCount, out Command cmd, bool ignoreCase);

        void RegisterCommand(MethodInfo method, bool ignoreInvalid);
    }

    internal sealed class CommandRegistry : ICommandRegistryInternal
    {
        private readonly IDictionary<string, Command> Commands = new Dictionary<string, Command>();

        private ICommandRegistryInternal Internal => this;

        internal CommandRegistry()
        {
        }

        IEnumerable<Command> ICommandRegistryInternal.GetCommands() => Commands.Values.Distinct();

        void ICommandRegistryInternal.RegisterCommand(MethodInfo method, bool ignoreInvalid)
        {
            var attr = method.GetCustomAttribute<CommandAttribute>();

            if (attr == null)
            {
                if (ignoreInvalid)
                    return;
                else
                    throw new InvalidCommandMethodException("Command methods must be decorated with the [Command] attribute");
            }

            string name = attr.Name ?? method.Name.ToLower();

            var cmd = new Command(name, attr.Description, method);

            if (Commands.ContainsKey(name + "_" + cmd.Params.Length))
            {
                if (ignoreInvalid)
                    return;
                else
                    throw new InvalidCommandMethodException("That command name is already in use");
            }

            if (name.StartsWith("$"))
                throw new InvalidCommandMethodException("Command names cannot start with '$'");

            for (int i = cmd.Params.Count(o => !o.Optional); i < cmd.Params.Length + 1; i++)
            {
                Commands.Add(name + "_" + i, cmd);
            }
        }

        bool ICommandRegistryInternal.TryGetCommand(string name, int argCount, out Command cmd) => Internal.TryGetCommand(name, argCount, out cmd, false);

        bool ICommandRegistryInternal.TryGetCommand(string name, int argCount, out Command cmd, bool ignoreCase)
        {
            if (argCount != -1)
            {
                name += "_" + argCount;
            }
            else
            {
                cmd = Commands.SingleOrDefault(o => o.Key.StartsWith(name + "_", ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)).Value;
                return cmd.Name != null;
            }

            if (ignoreCase)
            {
                cmd = Commands.SingleOrDefault(o => o.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
                return cmd.Name != null;
            }
            else
            {
                return Commands.TryGetValue(name, out cmd);
            }
        }

        public void RegisterCommand(MethodInfo method) => Internal.RegisterCommand(method, false);

        public void RegisterCommandsIn(Type type)
        {
            foreach (var item in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Internal.RegisterCommand(item, true);
            }
        }
    }
}
