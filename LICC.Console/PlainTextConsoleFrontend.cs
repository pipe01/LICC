using LICC.API;
using System;
using SConsole = System.Console;

namespace LICC.Console
{
    public class PlainTextConsoleFrontend : Frontend
    {
        public void BeginRead()
        {
            while (true)
            {
                SConsole.Write("> ");
                string line = SConsole.ReadLine();
                OnLineInput(line);
            }
        }

        public override void Write(string str, Color color)
        {
            var prev = SConsole.ForegroundColor;
            SConsole.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color.ToString());
            SConsole.Write(str);
            SConsole.ForegroundColor = prev;
        }

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
