using LICC.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LICC
{
    public delegate void LineOutputDelegate(string line);
    public delegate void ColoredLineOutputDelegate(IReadOnlyList<(string Text, CColor? Color)> segments);

    /// <summary>
    /// Replacement for <see cref="Console"/>, meant to provide a frontend-independent way of outputting
    /// colorful text.
    /// </summary>
    public static class LConsole
    {
        public static event LineOutputDelegate LineWritten = delegate { };
        public static event ColoredLineOutputDelegate ColoredLineWritten = delegate { };

        private static object LineLock = new object();

        internal static Frontend Frontend { get; set; }

        internal static void OnLineWritten(string line) => LineWritten(line);
        internal static void OnColoredLineWritten(IReadOnlyList<(string Text, CColor? Color)> segments) => ColoredLineWritten(segments);
        private static void OnColoredLineWritten(string Text, CColor? Color) => ColoredLineWritten(new (string Text, CColor? Color)[] {(Text, Color)});

        internal static void Write(string str) => Frontend?.Write(str);
        internal static void Write(string str, CColor color) => Frontend?.Write(str, color);
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
            lock (LineLock)
            {
                LineWritten(str);
                OnColoredLineWritten(str, null);

                if (Frontend != null)
                {
                    Frontend.PauseInput();
                    Frontend.WriteLine(str);
                    Frontend.ResumeInput();
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
            lock (LineLock)
            {
                LineWritten(str);
                OnColoredLineWritten(str, color);

                if (Frontend != null)
                {
                    Frontend.PauseInput();
                    Frontend.WriteLine(str, color);
                    Frontend.ResumeInput();
                }
            }
        }

        /// <summary>
        /// Writes a colored string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="obj">The line to write.</param>
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
    }

    /// <summary>
    /// Class used to write a message containing multiple differently-colored segments.
    /// </summary>
    public struct LineWriter : IDisposable
    {
        private static readonly SemaphoreSlim WriteSemaphore = new SemaphoreSlim(1);

        public static LineWriter Start() => new LineWriter(new List<(string Text, CColor? Color)>());

        private bool Disposed;
        private readonly bool UsingPartial;
        private readonly List<(string Text, CColor? Color)> TextRegions;

        private LineWriter(List<(string Text, CColor? Color)> textRegions) : this()
        {
            this.TextRegions = textRegions ?? throw new ArgumentNullException(nameof(textRegions));

            if (LConsole.Frontend != null && LConsole.Frontend.SupportsPartialLines)
            {
                UsingPartial = true;
                WriteSemaphore.Wait();
                LConsole.Frontend.PauseInput();
            }
        }

        void IDisposable.Dispose() => End();

        /// <summary>
        /// Finishes and writes the line.
        /// </summary>
        public void End()
        {
            if (Disposed) throw new InvalidOperationException("Writer is ended");

            LConsole.OnLineWritten(string.Concat(TextRegions.Select(o => o.Text)));

            if (LConsole.Frontend != null)
            {
                if (UsingPartial)
                {
                    WriteSemaphore.Release();
                    LConsole.Frontend.WriteLine("");
                    LConsole.Frontend.ResumeInput();
                }
                else
                {
                    LConsole.Frontend.WriteLineWithRegions(TextRegions.Select(o => (o.Text, o.Color ?? LConsole.Frontend.DefaultForeground)).ToArray());
                }
            }

            LConsole.OnColoredLineWritten(TextRegions);

            Disposed = true;
        }

        private LineWriter RunIfNotDisposed(Action partialAction, Func<(string, CColor?)> nonPartialRegionGetter)
        {
            if (Disposed) throw new InvalidOperationException("Writer is ended");

            if (UsingPartial)
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
