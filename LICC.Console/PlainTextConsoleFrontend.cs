using System;
using LICC.API;
using SConsole = System.Console;

namespace LICC.Console
{
    public class PlainTextConsoleFrontend : Frontend, ILineReader
    {
        private readonly ConsoleOptions Options;

        public PlainTextConsoleFrontend(ConsoleOptions options = null)
        {
            this.Options = options ?? ConsoleOptions.Default;
        }


        /// <summary>
        /// Blocks the current thread and begins reading input.
        /// </summary>
        public void BeginRead()
        {
            while (true)
            {
                if (Options.ShowPrompt)
                    SConsole.Write("> ");

                string line = SConsole.ReadLine();

                
                // If the stream was closed, dont keep reading it, as that would cause uncontrollable spam. This issue is confirmed to happen on linux when running the Logic World server via 
                // ./Server < /dev/null
                // it likely is however also present on other platforms.
                // The effect of this defect is that OnLineInput keeps getting called with null as a value, which subsequently is then interpreted as a command leading to tons of log output.                
                if (line == null)
                    break;

                OnLineInput(line);
            }
        }

        public override void Write(string str, CColor color)
        {
            switch (Options.ColorMode)
            {
                case ConsoleOptions.ColorLevel.NoColor:
                {
                    SConsole.Write(str);
                    break;
                }
                case ConsoleOptions.ColorLevel.Color:
                {
                    var prev = SConsole.ForegroundColor;
                    SConsole.ForegroundColor = color.ToConsoleColor();
                    SConsole.Write(str);
                    SConsole.ForegroundColor = prev;
                    break;
                }
                case ConsoleOptions.ColorLevel.AnsiRGBColor:
                {
                    // This assumes, that a WriteLine will eventually be called. As only that resets.
                    // Luckily LConsole.LineWriter is using WriteLine at the end of the operation.
                    SConsole.Write(color.ToAnsiRGB() + str);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(Options.ColorMode), Options.ColorMode, $"Invalid {nameof(ConsoleOptions.ColorLevel)}");
            }
        }

        // Overriden solely to append ANSI reset to the end of the line.
        public override void WriteLine(string str, CColor color)
        {
            if (Options.ColorMode != ConsoleOptions.ColorLevel.AnsiRGBColor)
            {
                Write(str + '\n', color);
                return;
            }
            SConsole.WriteLine(color.ToAnsiRGB() + str + CColor.ANSI_RESET);
        }

        /// <summary>
        /// Fires up a console with the default settings.
        /// </summary>
        public static void StartDefault(string fileSystemRoot = null)
        {
            var frontend = new PlainTextConsoleFrontend();
            FrontendManager.Frontend = frontend;
            var console = fileSystemRoot == null ? new CommandConsole(frontend) : new CommandConsole(frontend, fileSystemRoot);
            console.Commands.RegisterCommandsInAllAssemblies();

            console.RunAutoexec();

            frontend.BeginRead();
        }
    }
}
