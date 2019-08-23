using System;
using System.Threading;

namespace LICC.API
{
    public static class LConsole
    {
        internal static Frontend Frontend { get; set; }

        internal static void Write(string str) => Frontend.Write(str);
        internal static void Write(string str, Color color) => Frontend.Write(str, color);
        internal static void Write(string format, params object[] args) => Write(string.Format(format, args));
        internal static void Write(string format, Color color, params object[] args) => Write(string.Format(format, args), color);

        public static LineWriter BeginWrite() => new LineWriter();

        public static void WriteLine() => WriteLine("");

        public static void WriteLine(string str)
        {
            Frontend.PauseInput();
            Frontend.WriteLine(str);
            Frontend.ResumeInput();
        }

        public static void WriteLine(string str, Color color)
        {
            Frontend.PauseInput();
            Frontend.WriteLine(str, color);
            Frontend.ResumeInput();
        }

        public static void WriteLine(string format, params object[] args) => WriteLine(string.Format(format, args));
        public static void WriteLine(string format, Color color, params object[] args) => WriteLine(string.Format(format, args), color);
    }

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

        public LineWriter Write(string str) => RunIfNotDisposed(() => LConsole.Write(str));
        public LineWriter Write(string str, Color color) => RunIfNotDisposed(() => LConsole.Write(str, color));
        public LineWriter Write(string format, params object[] args) => RunIfNotDisposed(() => LConsole.Write(format, args));
        public LineWriter Write(string format, Color color, params object[] args) => RunIfNotDisposed(() => LConsole.Write(format, color, args));
    }
}
