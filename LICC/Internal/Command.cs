using LICC.API;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace LICC.Internal
{
    [DebuggerDisplay("{Name} {Params.Length}")]
    internal class Command : IEquatable<Command>
    {
        public Guid ID { get; }
        public string Name { get; }
        public string Description { get; }
        public bool Hidden { get; }
        public (string Name, Type Type, bool Optional)[] Params { get; }
        public int RequiredParamCount { get; }
        public int OptionalParamCount { get; }
        public MethodInfo Method { get; }
        public Type InstanceType { get; }
        public int ArgCount { get; }
        public (ParameterInfo Param, int Index)[] InjectedParameters { get; }

        public Command(string name, string desc, MethodInfo method, Type instanceType, bool hidden)
        {
            var methodParams = method.GetParameters();

            this.ID = Guid.NewGuid();
            this.Name = name;
            this.Description = desc;
            this.Hidden = hidden;
            this.Params = methodParams.Where(o => !o.IsDefined(typeof(InjectAttribute))).Select(o => (o.Name, o.ParameterType, o.HasDefaultValue)).ToArray();
            this.RequiredParamCount = Params.Count(o => !o.Optional);
            this.OptionalParamCount = Params.Count(o => o.Optional);
            this.Method = method;
            this.InstanceType = instanceType ?? method.DeclaringType;
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
