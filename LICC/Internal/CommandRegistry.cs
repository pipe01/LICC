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

        void RegisterCommand(MethodInfo method, bool ignoreInvalid);
    }

    internal sealed class CommandRegistry : ICommandRegistryInternal
    {
        private readonly IList<Command> Commands = new List<Command>();

        private ICommandRegistryInternal Internal => this;

        internal CommandRegistry()
        {
        }

        IEnumerable<Command> ICommandRegistryInternal.GetCommands() => Commands.Distinct();

        void ICommandRegistryInternal.RegisterCommand(MethodInfo method, bool ignoreInvalid)
        {
            CommandAttribute attr;

            try
            {
                attr = method.GetCustomAttribute<CommandAttribute>();
            }
            catch (System.IO.FileNotFoundException)
            {
                // if the method has an attribute which is part of an assembly that isn't loaded,
                // you'll get a FileNotFoundException here.
                return;
            }

            if (attr == null)
            {
                if (ignoreInvalid)
                    return;
                else
                    throw new InvalidCommandMethodException("Command methods must be decorated with the [Command] attribute");
            }

            string name = attr.Name ?? method.Name.ToLower();

            var cmd = new Command(name, attr.Description, method);

            if (Commands.Any(o => o.Name == name && o.RequiredParamCount == cmd.RequiredParamCount
                                && o.OptionalParamCount == cmd.OptionalParamCount))
            {
                if (ignoreInvalid)
                    return;
                else
                    throw new InvalidCommandMethodException("That command name is already in use");
            }

            if (name.StartsWith("$"))
                throw new InvalidCommandMethodException("Command names cannot start with '$'");

            Commands.Add(cmd);
        }

        public void RegisterCommand(MethodInfo method) => Internal.RegisterCommand(method, false);

        public void RegisterCommandsIn(Type type)
        {
            foreach (var item in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                Internal.RegisterCommand(item, true);
            }
        }
    }
}
