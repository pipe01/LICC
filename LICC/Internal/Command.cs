using LICC.API;
using System;
using System.Linq;
using System.Reflection;

namespace LICC.Internal
{
    internal struct Command : IEquatable<Command>
    {
        public Guid ID { get; }
        public string Name { get; }
        public string Description { get; }
        public (string Name, Type Type, bool Optional)[] Params { get; }
        public MethodInfo Method { get; }
        public int ArgCount { get; }
        public (ParameterInfo Param, int Index)[] InjectedParameters { get; }

        public Command(string name, string desc, MethodInfo method)
        {
            var methodParams = method.GetParameters();

            this.ID = Guid.NewGuid();
            this.Name = name;
            this.Description = desc;
            this.Params = methodParams.Where(o => !o.IsDefined(typeof(InjectAttribute))).Select(o => (o.Name, o.ParameterType, o.HasDefaultValue)).ToArray();
            this.Method = method;
            this.ArgCount = methodParams.Length;
            this.InjectedParameters = methodParams
                    .Select((param, index) => (param, index))
                    .Where(o => o.param.IsDefined(typeof(InjectAttribute)))
                    .ToArray();
        }

        public bool Equals(Command other) => other.ID == ID;
    }

    internal static class CommandExtensions
    {
        public static void PrintUsage(this Command cmd)
        {
            if (cmd.Description != null)
                LConsole.WriteLine(cmd.Description, ConsoleColor.Cyan);

            using (var writer = LConsole.BeginLine())
            {
                writer.Write("Usage: ", ConsoleColor.DarkYellow);

                string usage = cmd.Name + " " + cmd.GetParamsString();
                writer.Write(usage, ConsoleColor.Cyan);
            }
        }

        public static string GetParamsString(this Command cmd)
        {
            return string.Join(" ", cmd.Params.Select(o => (o.Optional ? "[" : "<") + o.Type.Name + " " + o.Name + (o.Optional ? "]" : ">")));
        }
    }
}
