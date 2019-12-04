using System;

namespace LICC.API
{
    internal delegate void LineInputDelegate(string line);

    /// <summary>
    /// Represents a way for the console to output data and get input from the user.
    /// </summary>
    public abstract class Frontend
    {
        internal event LineInputDelegate LineInput;

        public virtual bool SupportsPartialLines { get; } = true;

        public virtual bool PreferOneLine { get; } = false;

        public virtual CColor DefaultForeground => ConsoleColor.Gray;

        /// <summary>
        /// Command history for the current session.
        /// </summary>
        protected internal IHistory History { get; internal set; }

        /// <summary>
        /// Submits a line command to the shell.
        /// </summary>
        /// <param name="line">The line containing the command name and arguments.</param>
        protected void OnLineInput(string line) => LineInput?.Invoke(line);

        /// <summary>
        /// Initialises the frontend.
        /// </summary>
        protected internal virtual void Init() { }

        /// <summary>
        /// Exits the application. Uses <see cref="Environment.Exit(int)"/> by default.
        /// </summary>
        protected internal virtual void Exit() => Environment.Exit(0);

        protected internal virtual void Stop() => throw new NotSupportedException("The current frontend does not support switching");

        /// <summary>
        /// Pause and clear input, only needed for one-screen frontends like <see cref="Console"/>.
        /// </summary>
        protected internal virtual void PauseInput() { }
        /// <summary>
        /// Resume input, only needed for one-screen frontends like <see cref="Console"/>.
        /// </summary>
        protected internal virtual void ResumeInput() { }

        /// <summary>
        /// Writes an uncolored string to the output.
        /// </summary>
        /// <param name="str">The string to write</param>
        public virtual void Write(string str) => this.Write(str, DefaultForeground);
        /// <summary>
        /// Writes a colored string to the output.
        /// </summary>
        /// <param name="str">The string to write</param>
        /// <param name="color">The color to give to the string.</param>
        public virtual void Write(string str, CColor color)
        {
            if (SupportsPartialLines)
                throw new NotImplementedException("Missing Write method override");
        }

        /// <summary>
        /// Writes a newline-terminated uncolored string to the output.
        /// </summary>
        /// <param name="str">The line to write.</param>
        public virtual void WriteLine(string str) => WriteLine(str, DefaultForeground);

        /// <summary>
        /// Writes a newline-terminated colored string to the output.
        /// </summary>
        /// <param name="str">The line to write.</param>
        /// <param name="color">The color to give to the line.</param>
        public virtual void WriteLine(string str, CColor color) => Write(str + Environment.NewLine, color);

        public virtual void PrintException(Exception ex)
        {
            LConsole.BeginLine()
                .Write("An exception occurred while executing the command: ", ConsoleColor.Red)
                .Write(ex.Message, ConsoleColor.DarkRed)
                .End();
        }

        public virtual void WriteLineWithRegions((string Text, CColor Color)[] regions)
            => throw new InvalidOperationException($"This class doesn't support partial lines but doesn't override the {nameof(WriteLineWithRegions)} method");
    }
}
