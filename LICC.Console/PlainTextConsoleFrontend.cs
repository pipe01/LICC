using LICC.API;
using System;
using System.Drawing;
using SConsole = System.Console;

namespace LICC.Console
{
    public class PlainTextConsoleFrontend : Frontend, ILineReader
    {
        /// <summary>
        /// Blocks the current thread and begins reading input.
        /// </summary>
        public void BeginRead()
        {
            while (true)
            {
                SConsole.Write("> ");
                string line = SConsole.ReadLine();
                OnLineInput(line);
            }
        }

        public override void Write(string str, CColor color)
        {
            var prev = SConsole.ForegroundColor;
            SConsole.ForegroundColor = color.ToConsoleColor();
            SConsole.Write(str);
            SConsole.ForegroundColor = prev;
        }

        /// <summary>
        /// Fires up a console with the default settings.
        /// </summary>
        public static void StartDefault(string fileSystemRoot = null)
        {
            var frontend = new PlainTextConsoleFrontend();
            var console = fileSystemRoot == null ? new CommandConsole(frontend) : new CommandConsole(frontend, fileSystemRoot);
            console.Commands.RegisterCommandsInAllAssemblies();

            console.RunAutoexec();

            frontend.BeginRead();
        }
    }
}
