using LICC.API;
using LICC.Exceptions;
using System;
using System.Linq;
using System.Reflection;

namespace LICC
{
    public sealed class CommandConsole
    {
        internal static CommandConsole Current { get; private set; }

        public CommandRegistry Commands { get; } = new CommandRegistry();

        internal readonly Shell Shell;
        internal readonly ConsoleConfiguration Config;
        internal readonly IFileSystem FileSystem;

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
    }
}
