namespace LICC.API
{
    internal delegate void LineInputDelegate(string line);

    /// <summary>
    /// Represents a way for the console to output data and get input from the user.
    /// </summary>
    public abstract class Frontend
    {
        internal event LineInputDelegate LineInput;

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
        /// Pause and clear input, only needed for one-screen frontends like <see cref="System.Console"/>.
        /// </summary>
        protected internal virtual void PauseInput() { }
        /// <summary>
        /// Resume input, only needed for one-screen frontends like <see cref="System.Console"/>.
        /// </summary>
        protected internal virtual void ResumeInput() { }

        /// <summary>
        /// Writes an uncolored string to the output.
        /// </summary>
        /// <param name="str">The string to write</param>
        public virtual void Write(string str) => Write(str, Color.White);
        /// <summary>
        /// Writes a colored string to the output.
        /// </summary>
        /// <param name="str">The string to write</param>
        /// <param name="color">The color to give to the string.</param>
        public abstract void Write(string str, Color color);

        /// <summary>
        /// Writes a newline-terminated uncolored string to the output.
        /// </summary>
        /// <param name="str">The line to write.</param>
        public virtual void WriteLine(string str) => Write(str + "\n");
        /// <summary>
        /// Writes a newline-terminated colored string to the output.
        /// </summary>
        /// <param name="str">The line to write.</param>
        /// <param name="color">The color to give to the line.</param>
        public virtual void WriteLine(string str, Color color) => Write(str + "\n", color);
    }
}
