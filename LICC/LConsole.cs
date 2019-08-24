﻿using LICC.API;
using System;
using System.Drawing;
using System.Threading;

namespace LICC
{
    /// <summary>
    /// Replacement for <see cref="Console"/>, meant to provide a frontend-independant way of outputting
    /// colorful text.
    /// </summary>
    public static class LConsole
    {
        internal static Frontend Frontend { get; set; }

        internal static void Write(string str) => Frontend.Write(str);
        internal static void Write(string str, Color color) => Frontend.Write(str, color);
        internal static void Write(string str, ConsoleColor color) => Frontend.Write(str, color);
        internal static void Write(string format, params object[] args) => Write(string.Format(format, args));
        internal static void Write(string format, Color color, params object[] args) => Write(string.Format(format, args), color);
        internal static void Write(string format, ConsoleColor color, params object[] args) => Write(string.Format(format, args), color);

        /// <summary>
        /// Makes and returns a new line writer. This must be used if you want to write a line with different
        /// colored words.
        /// </summary>
        public static LineWriter BeginLine() => new LineWriter();

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
            Frontend.PauseInput();
            Frontend.WriteLine(str);
            Frontend.ResumeInput();
        }

        /// <summary>
        /// Writes a colored string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="str">The line to write.</param>
        /// <param name="color">The color to write this line in.</param>
        public static void WriteLine(string str, Color color)
        {
            Frontend.PauseInput();
            Frontend.WriteLine(str, color);
            Frontend.ResumeInput();
        }

        /// <summary>
        /// Writes a colored string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="str">The line to write.</param>
        /// <param name="color">The color to write this line in.</param>
        public static void WriteLine(string str, ConsoleColor color)
        {
            Frontend.PauseInput();
            Frontend.WriteLine(str, color);
            Frontend.ResumeInput();
        }

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
        public static void WriteLine(string format, Color color, params object[] args) => WriteLine(string.Format(format, args), color);

        /// <summary>
        /// Writes a colored formatted string, delimited by a newline separator at the end.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="color">The color to write this line in.</param>
        /// <param name="args">The arguments to format the string with.</param>
        public static void WriteLine(string format, ConsoleColor color, params object[] args) => WriteLine(string.Format(format, args), color);
    }

    /// <summary>
    /// Class used to write a line containing multiple differently-colored segments.
    /// </summary>
    public class LineWriter : IDisposable
    {
        private static readonly SemaphoreSlim WriteSemaphore = new SemaphoreSlim(1);

        private bool Disposed;

        internal LineWriter()
        {
            WriteSemaphore.Wait();
            LConsole.Frontend.PauseInput();
        }

        void IDisposable.Dispose() => End();

        /// <summary>
        /// Finishes and writes the line.
        /// </summary>
        public void End()
        {
            if (Disposed) throw new InvalidOperationException("Writer is ended");
            LConsole.Frontend.WriteLine("");
            LConsole.Frontend.ResumeInput();
            Disposed = true;

            WriteSemaphore.Release();
        }

        private LineWriter RunIfNotDisposed(Action action)
        {
            if (Disposed) throw new InvalidOperationException("Writer is ended");

            action();
            return this;
        }

        public LineWriter Write(object obj) => Write(obj?.ToString());
        public LineWriter Write(string str) => RunIfNotDisposed(() => LConsole.Write(str));

        public LineWriter Write(object obj, Color color) => Write(obj?.ToString(), color);
        public LineWriter Write(object obj, ConsoleColor color) => Write(obj?.ToString(), color);

        public LineWriter Write(string str, Color color) => RunIfNotDisposed(() => LConsole.Write(str, color));
        public LineWriter Write(string str, ConsoleColor color) => RunIfNotDisposed(() => LConsole.Write(str, color));

        public LineWriter Write(string format, params object[] args) => RunIfNotDisposed(() => LConsole.Write(format, args));

        public LineWriter Write(string format, Color color, params object[] args) => RunIfNotDisposed(() => LConsole.Write(format, color, args));
        public LineWriter Write(string format, ConsoleColor color, params object[] args) => RunIfNotDisposed(() => LConsole.Write(format, color, args));
    }
}
