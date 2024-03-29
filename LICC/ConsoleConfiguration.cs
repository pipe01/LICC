﻿namespace LICC
{
    /// <summary>
    /// Configures how the console and shell behave.
    /// </summary>
    public sealed class ConsoleConfiguration
    {
        /// <summary>
        /// If true, the autoexec file will be run after the frontend is initialised.
        /// </summary>
        public bool RunAutoExecAtStartup { get; set; } = true;

        public bool EnableVariables { get; set; } = true;

        public bool RegisterAllCommandsOnStartup { get; set; } = true;
    }
}
