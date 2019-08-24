using LICC.API;
using LICC.Exceptions;
using LICC.Internal;
using System;
using System.Linq;
using System.Reflection;
using Environment = LICC.Internal.Environment;

namespace LICC
{
    /// <summary>
    /// The main console class.
    /// </summary>
    public sealed class CommandConsole
    {
        internal static CommandConsole Current { get; private set; }

        /// <summary>
        /// Command registry instance, used to register commands.
        /// </summary>
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
            this.Shell = shell ?? new Shell(valueConverter, history, fileSystem, commandRegistry, new Environment(), config);

            frontend.LineInput += Frontend_LineInput;

            Commands.RegisterCommandsIn(this.GetType().Assembly);

            frontend.Init();
        }

        /// <summary>
        /// Instantiate a new <see cref="CommandConsole"/> instance.
        /// </summary>
        /// <param name="frontend">The frontend to use for this console.</param>
        /// <param name="valueConverter">The value converter to use for command arguments.</param>
        /// <param name="fileSystem">The file system for commands like exec.</param>
        /// <param name="config">The console configuration.</param>
        public CommandConsole(Frontend frontend, IValueConverter valueConverter, IFileSystem fileSystem, ConsoleConfiguration config = null)
            : this(frontend, valueConverter, fileSystem, null, new CommandRegistry(), config ?? new ConsoleConfiguration())
        {
        }

        /// <summary>
        /// Instantiate a new <see cref="CommandConsole"/> instance.
        /// </summary>
        /// <param name="frontend">The frontend to use for this console.</param>
        /// <param name="filesRootPath">The root folder for commands like exec.</param>
        /// <param name="config">The console configuration.</param>
        public CommandConsole(Frontend frontend, string filesRootPath, ConsoleConfiguration config = null)
            : this(frontend, new DefaultValueConverter(), new SystemIOFilesystem(filesRootPath), config)
        {
        }

        /// <summary>
        /// Instantiate a new <see cref="CommandConsole"/> instance.
        /// </summary>
        /// <param name="frontend">The frontend to use for this console.</param>
        /// <param name="config">The console configuration.</param>
        public CommandConsole(Frontend frontend, ConsoleConfiguration config = null)
            : this(frontend, new DefaultValueConverter(), null, config)
        {
        }

        /// <summary>
        /// Runs the autoexec.lsf file if it exists.
        /// </summary>
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
                LConsole.WriteLine(ex.Message, ConsoleColor.Red);
            }
            catch (ParameterMismatchException ex)
            {
                LConsole.WriteLine(ex.Message, ConsoleColor.Red);
                ex.Command.PrintUsage();
            }
            catch (ParameterConversionException ex)
            {
                LConsole.WriteLine(ex.Message, ConsoleColor.Red);
            }
            catch (ParserException ex)
            {
                LConsole.WriteLine("Error when parsing command: " + ex.Message, ConsoleColor.Red);
            }
            catch (Exception ex)
            {
                LConsole.WriteLine("An error occurred when executing this command:", ConsoleColor.Red);
                LConsole.WriteLine(ex.ToString(), ConsoleColor.Red);
            }
        }
    }
}
