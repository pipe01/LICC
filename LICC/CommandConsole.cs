using LICC.API;
using LICC.Exceptions;
using LICC.Internal;
using System;
using Environment = LICC.Internal.Environment;

namespace LICC
{
    public delegate void LsfExecutedDelegate(string path);
    public delegate void CommandExecutedDelegate(string command);

    /// <summary>
    /// The main console class.
    /// </summary>
    public sealed class CommandConsole
    {
        internal static CommandConsole Current { get; private set; }

        public event LsfExecutedDelegate LsfExecuted = delegate { };
        public event CommandExecutedDelegate CommandExecutedExternal = delegate { };
        public event CommandExecutedDelegate CommandExecutedInternal = delegate { };

        /// <summary>
        /// Command registry instance, used to register commands.
        /// </summary>
        public ICommandRegistry Commands => CommandRegistry;

        internal readonly IShell Shell;
        internal readonly ConsoleConfiguration Config;
        internal readonly IFileSystem FileSystem;
        internal readonly ICommandRegistryInternal CommandRegistry;

        internal CommandConsole(Frontend frontend, IValueConverter valueConverter, IFileSystem fileSystem,
            IShell shell, ICommandRegistryInternal commandRegistry, ICommandExecutor commandExecutor,
            ConsoleConfiguration config)
        {
            Current = this;

            this.Config = config ?? new ConsoleConfiguration();
            this.FileSystem = fileSystem;
            this.CommandRegistry = commandRegistry;
            // The history cast here is kind of bad, but the history should only ever be set by LICC, thus it works.
            this.Shell = shell ?? new Shell(valueConverter, (IWriteableHistory) frontend.History, fileSystem, new CommandFinder(commandRegistry),
                new Environment(), commandExecutor, null, this.Config);

            frontend.LineInput += Frontend_LineInput;

            Commands.RegisterCommandsIn(this.GetType().Assembly);
            Commands.RegisterCommandsIn(frontend.GetType().Assembly);

            if (this.Config.RegisterAllCommandsOnStartup)
                Commands.RegisterCommandsInAllAssemblies();
        }

        /// <summary>
        /// Instantiate a new <see cref="CommandConsole"/> instance.
        /// </summary>
        /// <param name="frontend">The frontend to use for this console.</param>
        /// <param name="valueConverter">The value converter to use for command arguments.</param>
        /// <param name="fileSystem">The file system for commands like exec.</param>
        /// <param name="objectProvider">The object provider for injecting dependencies into methods.</param>
        /// <param name="config">The console configuration.</param>
        public CommandConsole(Frontend frontend, IValueConverter valueConverter, IFileSystem fileSystem, ObjectProviderDelegate objectProvider, ConsoleConfiguration config = null)
            : this(frontend, valueConverter, fileSystem, null, new CommandRegistry(), new CommandExecutor(objectProvider), config ?? new ConsoleConfiguration())
        {
        }

        /// <summary>
        /// Instantiate a new <see cref="CommandConsole"/> instance.
        /// </summary>
        /// <param name="frontend">The frontend to use for this console.</param>
        /// <param name="filesRootPath">The root folder for commands like exec.</param>
        /// <param name="objectProvider">The object provider for injecting dependencies into methods.</param>
        /// <param name="config">The console configuration.</param>
        public CommandConsole(Frontend frontend, string filesRootPath, ObjectProviderDelegate objectProvider = null, ConsoleConfiguration config = null)
            : this(frontend, new DefaultValueConverter(), new SystemIOFilesystem(filesRootPath), objectProvider, config)
        {
        }

        /// <summary>
        /// Instantiate a new <see cref="CommandConsole"/> instance.
        /// </summary>
        /// <param name="frontend">The frontend to use for this console.</param>
        /// <param name="objectProvider">The object provider for injecting dependencies into methods.</param>
        /// <param name="config">The console configuration.</param>
        public CommandConsole(Frontend frontend, ObjectProviderDelegate objectProvider = null, ConsoleConfiguration config = null)
            : this(frontend, new DefaultValueConverter(), null, objectProvider, config)
        {
        }

        /// <summary>
        /// Runs the autoexec.lsf file if it exists.
        /// </summary>
        public void RunAutoexec()
        {
            const string autoExecFileName = "autoexec.lsf";

            if (FileSystem != null)
            {
                if (FileSystem.FileExists(autoExecFileName))
                    ExecuteLsf(autoExecFileName);
                else
                    FileSystem.CreateFile(autoExecFileName);
            }
        }

        public void ExecuteLsf(string path)
        {
            LsfExecuted(path);
            Shell.ExecuteLsf(path);
        }

        public void RunCommand(string cmd, bool addToHistory = false)
        {
            CommandExecutedExternal(cmd);
            Shell.ExecuteLine(cmd, addToHistory);
        }

        private void Frontend_LineInput(string line)
        {
            CommandExecutedInternal(line);
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
                LConsole.PrintException(ex, "An error occurred when executing this command:");
            }
        }
    }
}
