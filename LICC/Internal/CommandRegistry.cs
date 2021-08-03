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
        CommandRegistry.CommandCollectionByName AllRegisteredCommands { get; }
        Dictionary<string, CommandRegistry.CommandCollectionByName> CommandsByAssembly { get; }

        void RegisterCommand(MethodInfo method, bool ignoreInvalid);
    }

    internal sealed class CommandRegistry : ICommandRegistryInternal
    {
        internal sealed class CommandCollectionByName
        {
            private readonly Dictionary<string, NamedCommandEntry> CommandsByName = new Dictionary<string, NamedCommandEntry>(StringComparer.OrdinalIgnoreCase); // Note that command case is ignored

            public bool ContainsCommand(string commandName) => CommandsByName.ContainsKey(commandName);

            public NamedCommandEntry GetEntry(string commandName)
            {
                if (!CommandsByName.TryGetValue(commandName, out var entry))
                {
                    entry = new NamedCommandEntry();
                    CommandsByName.Add(commandName, entry);
                }

                return entry;
            }

            public int GetCommandCount()
            {
                int count = 0;

                foreach (var entry in CommandsByName.Values)
                    count += entry.Commands.Count;

                return count;
            }
        }

        internal sealed class NamedCommandEntry
        {
            public List<Command> Commands = new List<Command>();
        }



        public CommandCollectionByName AllRegisteredCommands { get; } = new CommandCollectionByName();
        public Dictionary<string, CommandCollectionByName> CommandsByAssembly { get; } = new Dictionary<string, CommandCollectionByName>(StringComparer.OrdinalIgnoreCase);

        internal CommandRegistry()
        {
        }

        public void RegisterCommand(MethodInfo method, bool ignoreInvalid)
        {
            CommandAttribute attribute;

            try
            {
                attribute = method.GetCustomAttribute<CommandAttribute>();
            }
            catch (System.IO.FileNotFoundException)
            {
                // If the method has an attribute which is part of an assembly that isn't loaded,
                // you'll get a FileNotFoundException here.
                return;
            }

            if (attribute == null)
            {
                if (ignoreInvalid)
                    return;
                else
                    throw new InvalidCommandMethodException("Command methods must be decorated with the [Command] attribute");
            }

            string commandName = String.IsNullOrEmpty(attribute.Name) ? method.Name : attribute.Name;

            if (commandName.StartsWith("$"))
                throw new InvalidCommandMethodException("Command names cannot start with '$'");


            var registryEntry = AllRegisteredCommands.GetEntry(commandName);
            var command = new Command(commandName, attribute.Description, method, attribute.ProviderType, attribute.Hidden);

            if (
                registryEntry.Commands.Any(o 
                    => o.RequiredParamCount == command.RequiredParamCount
                    && o.OptionalParamCount == command.OptionalParamCount)
                )
            {
                if (ignoreInvalid)
                    return;
                else
                    throw new InvalidCommandMethodException("That command name, with that number of parameters, is already in use");
            }


            registryEntry.Commands.Add(command);

            // Also track commands by assembly, for nicer `help` methods
            string assemblyName = method.DeclaringType.Assembly.GetName().Name;
            if (!CommandsByAssembly.TryGetValue(assemblyName, out var assemblyCommandCollection))
            {
                assemblyCommandCollection = new CommandCollectionByName();
                CommandsByAssembly.Add(assemblyName, assemblyCommandCollection);
            }
            assemblyCommandCollection.GetEntry(commandName).Commands.Add(command);
        }

        public void RegisterCommand(MethodInfo method) => RegisterCommand(method, false);

        public void RegisterCommandsIn(Type type)
        {
            List<Exception> exceptions = null;

            foreach (var item in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                try
                {
                    RegisterCommand(item, true);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }

            if (exceptions != null)
                throw new AggregateException("Exceptions when registering commands in type", exceptions);
        }
    }
}
