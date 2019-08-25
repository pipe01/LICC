namespace LICC
{
    /// <summary>
    /// Configures how the console and shell behave.
    /// </summary>
    public sealed class ConsoleConfiguration
    {
        /// <summary>
        /// If true, commands will ignore case, e.g. a command named "test" will be executed when typing "TeSt".
        /// </summary>
        public bool CaseSensitiveCommandNames { get; set; } = false;

        /// <summary>
        /// If true, the autoexec file will be run after the frontend is initialised.
        /// </summary>
        public bool RunAutoExecAtStartup { get; set; } = true;

        public bool EnableVariables { get; set; } = true;

        public bool RegisterAllCommandsOnStartup { get; set; } = true;
    }
}
