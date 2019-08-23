using LICC.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LICC
{
    public sealed class CommandRegistry
    {
        internal struct Command
        {
            public string Name { get; }
            public string Usage { get; }
            public (string Name, Type Type, bool Optional)[] Params { get; }
            public MethodInfo Method { get; }

            public Command(string name, string usage, MethodInfo method)
            {
                this.Name = name;
                this.Usage = usage;
                this.Params = method.GetParameters().Select(o => (o.Name, o.ParameterType, o.HasDefaultValue)).ToArray();
                this.Method = method;
            }
        }

        private readonly IDictionary<string, Command> Commands = new Dictionary<string, Command>();

        internal CommandRegistry()
        {
        }

        private void RegisterCommand(MethodInfo method, bool ignoreInvalid)
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

            if (Commands.ContainsKey(name))
            {
                if (ignoreInvalid)
                    return;
                else
                    throw new InvalidCommandMethodException("That command name is already in use");
            }

            Commands.Add(name, new Command(name, attr.Usage, method));
        }

        public void RegisterCommand(MethodInfo method) => RegisterCommand(method, false);

        public void RegisterCommandsIn(Type type)
        {
            foreach (var item in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                RegisterCommand(item, true);
            }
        }

        public void RegisterCommandsIn(Assembly assembly)
        {
            foreach (var item in assembly.GetTypes())
            {
                RegisterCommandsIn(item);
            }
        }

        internal bool TryGetCommand(string name, out Command cmd, bool ignoreCase = false)
        {
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
    }
}
