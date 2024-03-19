using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LICC.API;

namespace LICC
{
    public delegate void LineOutputDelegate(string line);
    public delegate void ColoredLineOutputDelegate(IReadOnlyList<(string Text, CColor? Color)> segments);
    public delegate void ExceptionOutputDelegate(Exception exception, string prefix = null);

    /// <summary>
    /// Replacement for <see cref="Console"/>, meant to provide a frontend-independent way of outputting
    /// colorful text.
    /// </summary>
    public static class LConsole
    {
        public static event LineOutputDelegate LineWritten = delegate { };
        public static event ColoredLineOutputDelegate ColoredLineWritten = delegate { };
        public static event ExceptionOutputDelegate ExceptionWritten = delegate { };

        internal static void OnLineWritten(string line) => LineWritten(line);
        internal static void OnColoredLineWritten(IReadOnlyList<(string Text, CColor? Color)> segments) => ColoredLineWritten(segments);
        private static void OnColoredLineWritten(string Text, CColor? Color) => ColoredLineWritten(new (string Text, CColor? Color)[] {(Text, Color)});

        internal static void Write(string str) => FrontendManager.Frontend?.Write(str);
        internal static void Write(string str, CColor color) => FrontendManager.Frontend?.Write(str, color);
        internal static void Write(string format, params object[] args) => Write(string.Format(format, args));
        internal static void Write(string format, CColor color, params object[] args) => Write(string.Format(format, args), color);

        /// <summary>
        /// Makes and returns a new line writer. This must be used if you want to write a line with different
        /// colored words.
        /// </summary>
        public static LineWriter BeginLine() => LineWriter.Start();

        /// <summary>
        /// Writes a newline separator.
        /// </summary>
        public static void WriteLine() => WriteLine("");

        /// <summary>
        /// Writes a string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="str">The line to write.</param>
        public static void WriteLine(string str)
        {
            LineWritten(str);
            OnColoredLineWritten(str, null);

            if (FrontendManager.HasFrontend)
            {
                var frontend = FrontendManager.Frontend;
                lock (frontend.Lock)
                {
                    frontend.PauseInput();
                    frontend.WriteLine(str);
                    frontend.ResumeInput();
                }
            }
        }

        /// <summary>
        /// Writes a string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="obj">The line to write.</param>
        public static void WriteLine(object obj) => WriteLine(obj?.ToString());

        /// <summary>
        /// Writes a colored string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="str">The line to write.</param>
        /// <param name="color">The color to write this line in.</param>
        public static void WriteLine(string str, CColor color)
        {
            LineWritten(str);
            OnColoredLineWritten(str, color);

            if (FrontendManager.HasFrontend)
            {
                var frontend = FrontendManager.Frontend;
                lock (frontend.Lock)
                {
                    frontend.PauseInput();
                    frontend.WriteLine(str, color);
                    frontend.ResumeInput();
                }
            }
        }

        /// <summary>
        /// Writes a colored string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="obj">The line to write.</param>
        /// <param name="color">The color to write this line in.</param>
        public static void WriteLine(object obj, CColor color) => WriteLine(obj?.ToString(), color);

        /// <summary>
        /// Writes a formatted string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The arguments to format the string with.</param>
        public static void WriteLine(string format, params object[] args) => WriteLine(string.Format(format, args));

        /// <summary>
        /// Writes a colored formatted string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="color">The color to write this line in.</param>
        /// <param name="args">The arguments to format the string with.</param>
        public static void WriteLine(string format, CColor color, params object[] args) => WriteLine(string.Format(format, args), color);

        /// <summary>
        /// Prints an exception. How this exception is printed, is up to the Frontend implementation. It can have an optional prefix.
        /// </summary>
        /// <param name="exception">The exception to print.</param>
        /// <param name="messagePrefix">An nullable prefix for the exception, describing circumstances.</param>
        public static void PrintException(Exception exception, string messagePrefix = null)
        {
            ExceptionWritten(exception, messagePrefix);

            if(FrontendManager.HasFrontend)
            {
                FrontendManager.Frontend.PauseInput();
                FrontendManager.Frontend.PrintException(exception, messagePrefix);
                FrontendManager.Frontend.ResumeInput();
            }
        }
    }

    /// <summary>
    /// Class used to write a message containing multiple differently-colored segments.
    /// </summary>
    public struct LineWriter : IDisposable
    {
        public static LineWriter Start() => new LineWriter(new List<(string Text, CColor? Color)>());

        private bool Disposed;
        private readonly Frontend FrontendUsed;
        private readonly List<(string Text, CColor? Color)> TextRegions;

        private LineWriter(List<(string Text, CColor? Color)> textRegions) : this()
        {
            this.TextRegions = textRegions ?? throw new ArgumentNullException(nameof(textRegions));

            FrontendUsed = FrontendManager.Frontend;
            if (FrontendUsed != null && FrontendUsed.SupportsPartialLines)
            {
                // Start critical section, that will block all other threads printing to this frontend.
                Monitor.Enter(FrontendUsed.Lock);
                FrontendUsed.PauseInput();
            }
        }

        void IDisposable.Dispose() => End();

        /// <summary>
        /// Finishes and writes the line.
        /// </summary>
        public void End()
        {
            if (Disposed) throw new InvalidOperationException("Writer is ended");

            if (FrontendUsed != null)
            {
                // If the frontend got replaced or removed, we do not care about the lock of it either.
                if (FrontendUsed.SupportsPartialLines)
                {
                    FrontendUsed.WriteLine("");
                    FrontendUsed.ResumeInput();
                    // Leave critical section, other threads may access this frontend again.
                    Monitor.Exit(FrontendUsed.Lock);
                }
                else
                {
                    lock (FrontendUsed.Lock)
                    {
                        FrontendUsed.PauseInput();
                        var FrontendUsedCopy = FrontendUsed;
                        FrontendUsed.WriteLineWithRegions(TextRegions.Select(o => (o.Text, o.Color ?? FrontendUsedCopy.DefaultForeground)).ToArray());
                        FrontendUsed.ResumeInput();
                    }
                }
            }

            LConsole.OnLineWritten(string.Concat(TextRegions.Select(o => o.Text)));
            LConsole.OnColoredLineWritten(TextRegions);

            Disposed = true;
        }

        private LineWriter RunIfNotDisposed(Action partialAction, Func<(string, CColor?)> nonPartialRegionGetter)
        {
            if (Disposed) throw new InvalidOperationException("Writer is ended");

            if (FrontendUsed != null && FrontendUsed.SupportsPartialLines)
                partialAction();

            TextRegions.Add(nonPartialRegionGetter());

            return this;
        }

        public LineWriter Write(object obj)
            => Write(obj?.ToString());
        public LineWriter Write(string str)
            => RunIfNotDisposed(() => LConsole.Write(str), () => (str, null));

        public LineWriter Write(object obj, CColor color)
            => Write(obj?.ToString(), color);

        public LineWriter Write(string str, CColor color)
            => RunIfNotDisposed(() => LConsole.Write(str, color), () => (str, color));

        public LineWriter Write(string format, params object[] args)
            => RunIfNotDisposed(() => LConsole.Write(format, args), () => (string.Format(format, args), null));

        public LineWriter Write(string format, CColor color, params object[] args)
            => RunIfNotDisposed(() => LConsole.Write(format, color, args), () => (string.Format(format, args), color));


        public LineWriter WriteLine()
            => Write(Environment.NewLine);

        public LineWriter WriteLine(object obj)
            => Write(obj).WriteLine();
        public LineWriter WriteLine(string str)
            => Write(str).WriteLine();

        public LineWriter WriteLine(object obj, CColor color)
            => Write(obj, color).WriteLine();

        public LineWriter WriteLine(string str, CColor color)
            => Write(str, color).WriteLine();

        public LineWriter WriteLine(string format, params object[] args)
            => Write(format, args).WriteLine();

        public LineWriter WriteLine(string format, CColor color, params object[] args)
            => Write(format, color, args).WriteLine();
    }
}
