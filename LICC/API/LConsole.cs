namespace LICC.API
{
    public static class LConsole
    {
        internal static Frontend Frontend { get; set; }

        public static void Write(string str) => Frontend.Write(str);
        public static void Write(string str, Color color) => Frontend.Write(str, color);
        public static void Write(string format, params object[] args) => Frontend.Write(string.Format(format, args));
        public static void Write(string format, Color color, params object[] args) => Frontend.Write(string.Format(format, args), color);

        public static void WriteLine() => Frontend.WriteLine("");
        public static void WriteLine(string str) => Frontend.WriteLine(str);
        public static void WriteLine(string str, Color color) => Frontend.WriteLine(str, color);
        public static void WriteLine(string format, params object[] args) => Frontend.WriteLine(string.Format(format, args));
        public static void WriteLine(string format, Color color, params object[] args) => Frontend.WriteLine(string.Format(format, args), color);
    }
}
