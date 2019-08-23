using LICC.API;
using LICC.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LICC
{
    public sealed class CommandConsole
    {
        private static CommandConsole Current;

        public CommandRegistry Commands { get; } = new CommandRegistry();

        private readonly Shell Shell;
        private readonly ConsoleConfiguration Config;

        public CommandConsole(Frontend frontend, IValueConverter valueConverter, ConsoleConfiguration config = null)
        {
            LConsole.Frontend = frontend;

            var history = new History();
            frontend.History = history;

            this.Config = config ?? new ConsoleConfiguration();
            this.Shell = new Shell(valueConverter, history, Commands, this.Config);

            frontend.LineInput += Frontend_LineInput;

            Commands.RegisterCommand(typeof(CommandConsole).GetMethod(nameof(HelpCommand), BindingFlags.NonPublic | BindingFlags.Static), true);
        }

        public CommandConsole(Frontend frontend, ConsoleConfiguration config = null) : this(frontend, new DefaultValueConverter(), config)
        {
        }

        [Command("help", Description = "Lists all commands or prints help for a command")]
        private static void HelpCommand(string command = null)
        {
            if (command == null)
            {
                var cmds = Current.Commands.GetCommands();
                int maxLength = cmds.Max(o => o.Name.Length);

                LConsole.WriteLine("Available commands:", Color.Magenta);
                foreach (var cmd in cmds)
                {
                    LConsole.Write(cmd.Name.PadLeft(maxLength), Color.Blue);
                    
                    if (cmd.Description != null)
                    {
                        LConsole.Write(": ", Color.DarkGray);
                        LConsole.WriteLine(cmd.Description, Color.DarkYellow);
                    }
                    else
                    {
                        LConsole.WriteLine("");
                    }
                }
            }
            else
            {
                if (!Current.Commands.TryGetCommand(command, out var cmd, !Current.Config.CaseSensitiveCommandNames))
                {
                    LConsole.WriteLine($"Cannot find command with name '{command}'", Color.Red);
                    return;
                }

                cmd.PrintUsage();
            }
        }

        private void Frontend_LineInput(string line)
        {
            Current = this;

            try
            {
                Shell.ExecuteLine(line);
            }
            catch (CommandNotFoundException ex)
            {
                LConsole.WriteLine(ex.Message, Color.Red);
            }
            catch (ParameterMismatchException ex)
            {
                LConsole.WriteLine(ex.Message, Color.Red);
            }
            catch (ParameterConversionException ex)
            {
                LConsole.WriteLine(ex.Message, Color.Red);
            }
            finally
            {
                Current = null;
            }
        }
    }
}
