namespace LICC.Console
{
    public class ConsoleOptions
    {
        internal static ConsoleOptions Default { get; } = new ConsoleOptions();

        public ColorLevel ColorMode { get; set; } = ColorLevel.Color;

        public bool ShowPrompt { get; set; } = true;

        public enum ColorLevel
        {
            NoColor,
            Color,
            AnsiRGBColor,
        }
    }
}
