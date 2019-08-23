using LICC.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LICC
{
    internal struct Command
    {
        public string Name { get; }
        public string Description { get; }
        public (string Name, Type Type, bool Optional)[] Params { get; }
        public MethodInfo Method { get; }

        public Command(string name, string desc, MethodInfo method)
        {
            this.Name = name;
            this.Description = desc;
            this.Params = method.GetParameters().Select(o => (o.Name, o.ParameterType, o.HasDefaultValue)).ToArray();
            this.Method = method;
        }
    }

    internal static class CommandExtensions
    {
        public static void PrintUsage(this Command cmd)
        {
            if (cmd.Description != null)
                LConsole.WriteLine(cmd.Description, Color.Cyan);
            LConsole.Write("Usage: ", Color.DarkYellow);

            string usage = cmd.Name + " " + string.Join(" ", cmd.Params.Select(o => (o.Optional ? "[" : "<") + o.Type.Name + " " + o.Name + (o.Optional ? "]" : ">")));
            LConsole.WriteLine(usage, Color.Cyan);
        }
    }
}
