using LICC.API;
using LICC.Exceptions;
using System;
using System.Linq;
using System.Reflection;

namespace LICC
{
    public sealed class CommandConsole
    {
        private static CommandConsole Current;

        public CommandRegistry Commands { get; } = new CommandRegistry();

        private readonly Shell Shell;
        private readonly ConsoleConfiguration Config;
        private readonly IFileSystem FileSystem;

        public CommandConsole(Frontend frontend, IValueConverter valueConverter, IFileSystem fileSystem, ConsoleConfiguration config = null)
        {
            Current = this;
            LConsole.Frontend = frontend;

            var history = new History();
            frontend.History = history;

            this.Config = config ?? new ConsoleConfiguration();
            this.FileSystem = fileSystem;
            this.Shell = new Shell(valueConverter, history, fileSystem, Commands, this.Config);

            frontend.LineInput += Frontend_LineInput;

            Commands.RegisterCommandsIn(this.GetType().Assembly);

            frontend.Init();
        }

        public CommandConsole(Frontend frontend, ConsoleConfiguration config = null)
            : this(frontend, new DefaultValueConverter(), null, config)
        {
        }
        
        public CommandConsole(Frontend frontend, string filesRootPath, ConsoleConfiguration config = null)
            : this(frontend, new DefaultValueConverter(), new SystemIOFilesystem(filesRootPath), config)
        {
        }

        public void RunAutoexec()
        {
            if (FileSystem?.FileExists("autoexec.lsf") ?? false)
                Shell.ExecuteLsf("autoexec.lsf");
        }

        private void Frontend_LineInput(string line)
        {
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
                ex.Command.PrintUsage();
            }
            catch (ParameterConversionException ex)
            {
                LConsole.WriteLine(ex.Message, Color.Red);
            }
            catch (ParserException ex)
            {
                LConsole.WriteLine("Error when parsing command: " + ex.Message, Color.Red);
            }
            catch (Exception ex)
            {
                LConsole.WriteLine("An error occurred when executing this command:", Color.Red);
                LConsole.WriteLine(ex.ToString(), Color.Red);
            }
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
                if (!Current.Commands.TryGetCommand(command, out var cmd, !Current.Config.CaseSensitiveCommandNames))
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
            Current.Shell.ExecuteLsf(fileName);
        }
    }
}
