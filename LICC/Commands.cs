using LICC.API;
using System.Linq;

#pragma warning disable IDE0051

namespace LICC
{
    internal static class Commands
    {
        [Command("help", Description = "Lists all commands or prints help for a command")]
        private static void HelpCommand(string command = null)
        {
            if (command == null)
            {
                var cmds = CommandConsole.Current.Commands.GetCommands();
                int maxLength = cmds.Max(o => o.Name.Length);

                LConsole.WriteLine("Available commands:", Color.Magenta);
                foreach (var cmd in cmds)
                {
                    using (var writer = LConsole.BeginWrite())
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
                if (!CommandConsole.Current.Commands.TryGetCommand(command, out var cmd, !CommandConsole.Current.Config.CaseSensitiveCommandNames))
                {
                    LConsole.WriteLine($"Cannot find command with name '{command}'", Color.Red);
                    return;
                }

                cmd.PrintUsage();
            }
        }

        [Command]
        private static void Exec(string fileName)
        {
            CommandConsole.Current.Shell.ExecuteLsf(fileName);
        }
    }
}
