using LICC.Internal;
using System;
using System.IO;
using System.Linq;

#pragma warning disable IDE0051

namespace LICC
{
    internal static class Commands
    {
        [Command(Description = "Lists all commands")]
        private static void Help()
        {
            var cmds = CommandConsole.Current.CommandRegistry.GetCommands().ToArray();

            LConsole.WriteLine("Available commands:", ConsoleColor.Magenta);

            LineWriter writer = null;

            int i = 0;
            foreach (var group in cmds.GroupBy(o => o.Method.DeclaringType.Assembly, (a, b) => new { Assembly = a, Commands = b.OrderBy(o => o.Name).ThenBy(o => o.Params.Length) }))
            {
                int maxLength = group.Commands.Max(o => (o.Name + (o.Params.Length > 0 ? " " + o.GetParamsString() : "")).Length) + 2;

                var name = new System.Reflection.AssemblyName(group.Assembly.FullName).Name;
                if (!LConsole.Frontend.PreferOneLine)
                {
                    LConsole.WriteLine("------", ConsoleColor.DarkGray);
                    LConsole.WriteLine(name, ConsoleColor.Cyan);
                }
                else
                {
                    LConsole.BeginLine()
                            .WriteLine("------", ConsoleColor.DarkGray)
                            .WriteLine(name, ConsoleColor.Yellow)
                            .End();
                }

                if (LConsole.Frontend.PreferOneLine)
                    writer = LConsole.BeginLine();

                foreach (var cmd in group.Commands)
                {
                    if (!LConsole.Frontend.PreferOneLine)
                        writer = LConsole.BeginLine();

                    int len = 0;

                    writer.Write(cmd.Name, ConsoleColor.Blue);
                    len += cmd.Name.Length;

                    string paramsStr = " " + cmd.GetParamsString();
                    writer.Write(paramsStr, ConsoleColor.DarkGreen);
                    len += paramsStr.Length;

                    writer.Write(new string(' ', maxLength - len));

                    if (cmd.Description != null)
                    {
                        writer.Write(": ", ConsoleColor.DarkGray);
                        writer.Write(cmd.Description, ConsoleColor.DarkYellow);
                    }

                    if (!LConsole.Frontend.PreferOneLine)
                        writer.End();
                    else if (i != cmds.Length - 1)
                        writer.WriteLine();

                    i++;
                }
            }

            if (LConsole.Frontend.PreferOneLine)
                writer.End();
        }

        [Command(Description = "Prints a command's usage")]
        private static void Help(string command)
        {
            var cmds = CommandConsole.Current.CommandRegistry.GetCommands().Where(o => o.Name == command).ToArray();

            if (cmds.Length == 0)
            {
                LConsole.WriteLine($"Cannot find command with name '{command}'", ConsoleColor.Red);
                return;
            }

            foreach (var cmd in cmds)
            {
                cmd.PrintUsage();
            }
        }

        [Command(Description = "Runs a .lsf file from the file system")]
        private static void Exec(string fileName)
        {
            try
            {
                CommandConsole.Current.Shell.ExecuteLsf(fileName);
            }
            catch (FileNotFoundException)
            {
                LConsole.WriteLine("File not found", ConsoleColor.Red);
            }
        }

        [Command(Description = "Print a string to screen")]
        private static void Echo(string str)
        {
            LConsole.WriteLine(str);
        }

        [Command("printex", Description = "Prints detailed info about the last exception encountered by a command")]
        private static void PrintException()
        {
            var ex = CommandConsole.Current.Shell.LastException;

            if (ex == null)
            {
                LConsole.WriteLine("No exception has occurred so far!", ConsoleColor.Green);
            }
            else
            {
                LConsole.WriteLine(ex.ToString());
            }
        }

        [Command("env", Description = "Prints all current variables and their values")]
        private static void PrintEnvironment()
        {
            if (!CommandConsole.Current.Config.EnableVariables)
            {
                LConsole.WriteLine("Variables are disabled", ConsoleColor.Red);
                return;
            }

            foreach (var item in CommandConsole.Current.Shell.Environment.GetAll())
            {
                LConsole.BeginLine()
                    .Write(item.Key, ConsoleColor.DarkGreen)
                    .Write(" = ", ConsoleColor.DarkGray)
                    .Write(item.Value)
                    .End();
            }
        }
    }
}
