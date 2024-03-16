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
                OnLineInput(line);
            }
        }

        public override void Write(string str, CColor color)
        {
            if (Options.UseColoredOutput)
            {
                var prev = SConsole.ForegroundColor;
                SConsole.ForegroundColor = color.ToConsoleColor();
                SConsole.Write(str);
                SConsole.ForegroundColor = prev;
            }
            else
            {
                SConsole.Write(str);
            }
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
