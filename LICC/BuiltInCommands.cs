using LICC.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable IDE0051

namespace LICC
{
    internal static class BuiltInCommands
    {
        [Command(Description = "Lists all available commands")]
        private static void Help()
        {
            int totalCommandCount = CommandConsole.Current.CommandRegistry.AllRegisteredCommands.GetCommandCount();

            if (totalCommandCount == 0)
            {
                LConsole.WriteLine("Uh oh, there aren't any registered commands");
                return;
            }


            int maxUsageLength = GetMaxCommandUsageLength(CommandConsole.Current.CommandRegistry.AllRegisteredCommands.EnumerateAllCommands());

            LineWriter writer = LConsole.BeginLine();
            writer.WriteLine($"Available commands ({totalCommandCount}):", ConsoleColor.Magenta);
            writer.WriteLine();

            foreach (string assemblyName in CommandConsole.Current.CommandRegistry.CommandsByAssembly.Keys.OrderBy(s => s))
            {
                WriteAssemblyCommands(maxUsageLength, writer, assemblyName);
                writer.WriteLine();
            }

            writer.End();
        }

        [Command(Description = "Lists all available commands in a given assembly")]
        private static void HelpAss(string assemblyName)
        {
            if (!CommandConsole.Current.CommandRegistry.CommandsByAssembly.TryGetValue(assemblyName, out var commands))
            {
                LConsole.WriteLine($"No assembly with commands found by name {assemblyName}. See `help` for commands in all assemblies.");
                return;
            }


            int maxUsageLength = GetMaxCommandUsageLength(commands);

            LineWriter writer = LConsole.BeginLine();
            WriteAssemblyCommands(maxUsageLength, writer, assemblyName);
            writer.End();
        }

        private static int GetMaxCommandUsageLength(IEnumerable<Command> commands)
        {
            const int hardMaxLength = 100;
            int maxLength = 0;

            foreach (var command in commands)
            {
                int commandUsageLength = (command.Name + (command.Params.Length > 0 ? " " + command.GetParamsString() : "")).Length + 2;

                if (commandUsageLength < hardMaxLength)
                    maxLength = Math.Max(maxLength, commandUsageLength);
            }

            return maxLength;
        }

        private static void WriteAssemblyCommands(int maxUsageLength, LineWriter writer, string assemblyName)
        {
            var assemblyCommands = CommandConsole.Current.CommandRegistry.CommandsByAssembly[assemblyName];

            string assemblyHeader = $"{assemblyName} ({assemblyCommands.Count} commands)";
            writer.WriteLine(assemblyHeader, ConsoleColor.Yellow);

            foreach (var command in assemblyCommands.OrderBy(c => c.Name).ThenBy(c => c.RequiredParamCount).ThenBy(c => c.OptionalParamCount))
            {
                if (command.Hidden)
                    continue;


                int lineLength = 0;

                writer.Write(command.Name, ConsoleColor.Blue);
                lineLength += command.Name.Length;

                string paramsStr = " " + command.GetParamsString();
                writer.Write(paramsStr, ConsoleColor.DarkGreen);
                lineLength += paramsStr.Length;

                if (command.Description != null)
                {
                    if (lineLength < maxUsageLength)
                        writer.Write(new string('-', maxUsageLength - lineLength), ConsoleColor.DarkGray);

                    writer.Write(": ", ConsoleColor.DarkGray);
                    writer.Write(command.Description, ConsoleColor.DarkYellow);
                }

                writer.WriteLine();
            }
        }

        [Command(Description = "Prints a command's usage")]
        private static void Help(string commandName)
        {
            if (!CommandConsole.Current.CommandRegistry.AllRegisteredCommands.ContainsCommand(commandName))
            {
                LConsole.WriteLine($"No registered command with name '{commandName}'", ConsoleColor.Red);
                return;
            }

            foreach (var cmd in CommandConsole.Current.CommandRegistry.AllRegisteredCommands.GetEntry(commandName).Commands)
            {
                cmd.PrintUsage();
            }
        }



        [Command(Description = "Runs a .lsf file from the file system")]
        private static void Exec(string fileName)
        {
            try
            {
                CommandConsole.Current.ExecuteLsf(fileName);
            }
            catch (FileNotFoundException)
            {
                LConsole.WriteLine("File not found", ConsoleColor.Red);
            }
        }

        [Command(Description = "Print an object to screen")]
        private static void Echo(object obj)
        {
            LConsole.WriteLine(obj?.ToString() ?? "null");
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
