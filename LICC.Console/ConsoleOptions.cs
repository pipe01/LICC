namespace LICC.Console
{
    public class ConsoleOptions
    {
        internal static ConsoleOptions Default { get; } = new ConsoleOptions();

        public bool UseColoredOutput { get; set; } = true;

        public bool ShowPrompt { get; set; } = true;
    }
}
