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

        public ICommandRegistry Commands => CommandRegistry;

        internal readonly IShell Shell;
        internal readonly ConsoleConfiguration Config;
        internal readonly IFileSystem FileSystem;
        internal readonly ICommandRegistryInternal CommandRegistry;

        internal CommandConsole(Frontend frontend, IValueConverter valueConverter, IFileSystem fileSystem,
            IShell shell, ICommandRegistryInternal commandRegistry, ConsoleConfiguration config)
        {
            Current = this;
            LConsole.Frontend = frontend;

            var history = new History();
            frontend.History = history;

            this.Config = config ?? new ConsoleConfiguration();
            this.FileSystem = fileSystem;
            this.CommandRegistry = commandRegistry;
            this.Shell = shell ?? new Shell(valueConverter, history, fileSystem, commandRegistry, config);

            frontend.LineInput += Frontend_LineInput;

            Commands.RegisterCommandsIn(this.GetType().Assembly);

            frontend.Init();
        }

        public CommandConsole(Frontend frontend, IValueConverter valueConverter, IFileSystem fileSystem, ConsoleConfiguration config = null)
            : this(frontend, valueConverter, fileSystem, null, new CommandRegistry(), config ?? new ConsoleConfiguration())
        {
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
