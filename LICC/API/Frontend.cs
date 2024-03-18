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

        public virtual CColor DefaultForeground => ConsoleColor.Gray;

        public readonly object Lock = new object();

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

        protected internal virtual void Stop() => throw new NotSupportedException("The current frontend does not support switching");

        /// <summary>
        /// Pause and clear input, only needed for one-screen frontends like <see cref="Console"/>.
        /// </summary>
        public virtual void PauseInput() { }
        /// <summary>
        /// Resume input, only needed for one-screen frontends like <see cref="Console"/>.
        /// </summary>
        public virtual void ResumeInput() { }

        /// <summary>
        /// Writes an uncolored string to the output.
        /// </summary>
        /// <param name="str">The string to write</param>
        public virtual void Write(string str) => Write(str, DefaultForeground);
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
        public virtual void WriteLine(string str, CColor color) => Write(str + "\n", color);

        public virtual void PrintException(Exception ex, string prefix = null)
        {
            if (prefix == null)
            {
                WriteLine(ex.Message, ConsoleColor.DarkRed);
            }
            else
            {
                WriteLineWithRegions(new (string Text, CColor Color)[]
                    {
                        (prefix + " ", ConsoleColor.Red),
                        (ex.Message, ConsoleColor.DarkRed)
                    }
                );
            }
        }

        public virtual void WriteLineWithRegions((string Text, CColor Color)[] regions)
        {
            if (SupportsPartialLines)
            {
                foreach (var (text, color) in regions)
                {
                    Write(text, color);
                }
                WriteLine("");
            }
            else
            {
                throw new InvalidOperationException($"{GetType().Name} doesn't support partial lines and didn't override the {nameof(WriteLineWithRegions)} method");
            }
        }
    }
}
