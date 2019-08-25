using LICC.Internal;
using System;
using System.IO;
using System.Linq;

#pragma warning disable IDE0051

namespace LICC
{
    internal static class Commands
    {
        [Command(Description = "Lists all commands or prints help for a command")]
        private static void Help(string command = null)
        {
            if (command == null)
            {
                var cmds = CommandConsole.Current.CommandRegistry.GetCommands().ToArray();
                int maxLength = cmds.Max(o => o.Name.Length);

                LConsole.WriteLine("Available commands:", ConsoleColor.Magenta);

                LineWriter writer = null;

                if (LConsole.Frontend.PreferOneLine)
                    writer = LConsole.BeginLine();

                int i = 0;
                foreach (var cmd in cmds)
                {
                    if (!LConsole.Frontend.PreferOneLine)
                        writer = LConsole.BeginLine();

                    writer.Write(cmd.Name.PadLeft(maxLength), ConsoleColor.Blue);

                    if (cmd.Description != null)
                    {
                        writer.Write(": ", ConsoleColor.DarkGray);
                        writer.Write(cmd.Description, ConsoleColor.DarkYellow);
                    }

                    if (!LConsole.Frontend.PreferOneLine)
                        writer.End();
                    else if (i++ != cmds.Length - 1)
                        writer.Write(System.Environment.NewLine);
                }

                if (LConsole.Frontend.PreferOneLine)
                    writer.End();
            }
            else
            {
                if (!CommandConsole.Current.CommandRegistry.TryGetCommand(command, out var cmd, !CommandConsole.Current.Config.CaseSensitiveCommandNames))
                {
                    LConsole.WriteLine($"Cannot find command with name '{command}'", ConsoleColor.Red);
                    return;
                }

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
