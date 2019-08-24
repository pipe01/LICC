using LICC.Internal;
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
                var cmds = CommandConsole.Current.CommandRegistry.GetCommands();
                int maxLength = cmds.Max(o => o.Name.Length);

                LConsole.WriteLine("Available commands:", Color.Magenta);
                foreach (var cmd in cmds)
                {
                    using (var writer = LConsole.BeginLine())
                    {
                        writer.Write(cmd.Name.PadLeft(maxLength), Color.Blue);

                        if (cmd.Description != null)
                        {
                            writer.Write(": ", Color.DarkGray);
                            writer.Write(cmd.Description, Color.DarkYellow);
                        }
                    }
                }
            }
            else
            {
                if (!CommandConsole.Current.CommandRegistry.TryGetCommand(command, out var cmd, !CommandConsole.Current.Config.CaseSensitiveCommandNames))
                {
                    LConsole.WriteLine($"Cannot find command with name '{command}'", Color.Red);
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
                LConsole.WriteLine("File not found", Color.Red);
            }
        }

        [Command(Description = "Print a string to screen")]
        private static void Echo(string str)
        {
            LConsole.WriteLine(str);
        }

        [Command("printex", Description = "Prints detailed info about the last exception")]
        private static void PrintException()
        {
            var ex = CommandConsole.Current.Shell.LastException;

            if (ex == null)
            {
                LConsole.WriteLine("No exception has occurred so far!", Color.Green);
            }
            else
            {
                LConsole.WriteLine(ex.ToString());
            }
        }
    }
}
