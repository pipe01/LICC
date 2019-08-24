using LICC.API;
using SimpleConsoleColor;
using System;
using System.Collections.Generic;
using SConsole = System.Console;

namespace LICC.Console
{
    public class ConsoleFrontend : Frontend
    {
        private (int X, int Y) StartPos;
        private string Buffer;
        private int CursorPos;

        private readonly Queue<ConsoleKeyInfo> QueuedKeys = new Queue<ConsoleKeyInfo>();

        private bool IsInputPaused;

        protected override void Init()
        {
            SConsole.TreatControlCAsInput = true;

            Buffer = "";
            StartPos = (SConsole.CursorLeft, SConsole.CursorTop);
            RewriteBuffer("");
        }

        /// <summary>
        /// Blocks the current thread and begins reading input.
        /// </summary>
        public void BeginRead()
        {
            while (true)
            {
                var key = SConsole.ReadKey(true);

                if (IsInputPaused)
                    QueuedKeys.Enqueue(key);
                else
                    HandleKey(key);
            }
        }

        private void HandleKey(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (Buffer.Length > 0 && CursorPos > 0)
                    {
                        int charsToDelete = 1;

                        if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                        {
                            int spaceIndex = Buffer.LastIndexOf(' ', CursorPos - 1);

                            charsToDelete = spaceIndex != -1 ? Buffer.Length - spaceIndex : CursorPos;
                        }

                        Buffer = Buffer.Remove(CursorPos - 1, charsToDelete);

                        SConsole.MoveBufferArea(SConsole.CursorLeft, SConsole.CursorTop, SConsole.BufferWidth - SConsole.CursorLeft, 1, SConsole.CursorLeft - charsToDelete, SConsole.CursorTop);
                        SConsole.CursorLeft -= charsToDelete;
                        CursorPos -= charsToDelete;
                    }
                    break;

                case ConsoleKey.Delete:
                    if (Buffer.Length > 0)
                    {
                        int charsToDelete = 1;

                        if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                        {
                            int spaceIndex = Buffer.IndexOf(' ', CursorPos);

                            charsToDelete = spaceIndex != -1 ? spaceIndex - CursorPos : Buffer.Length - CursorPos;
                        }

                        Buffer = Buffer.Remove(CursorPos, charsToDelete);
                        SConsole.MoveBufferArea(SConsole.CursorLeft + charsToDelete, SConsole.CursorTop, SConsole.BufferWidth - SConsole.CursorLeft - charsToDelete, 1, SConsole.CursorLeft, SConsole.CursorTop);
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (CursorPos > 0)
                    {
                        int charsToMove = 1;

                        if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                        {
                            int spaceIndex = Buffer.LastIndexOf(' ', CursorPos - 2);

                            charsToMove = spaceIndex != -1 ? CursorPos - spaceIndex - 1 : CursorPos;
                        }

                        CursorPos -= charsToMove;
                        SConsole.CursorLeft -= charsToMove;
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (CursorPos < Buffer.Length)
                    {
                        int charsToMove = 1;

                        if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                        {
                            int spaceIndex = Buffer.IndexOf(' ', CursorPos + 1);

                            charsToMove = spaceIndex != -1 ? spaceIndex - CursorPos : Buffer.Length - CursorPos;
                        }

                        CursorPos += charsToMove;
                        SConsole.CursorLeft += charsToMove;
                    }
                    break;

                case ConsoleKey.UpArrow:
                {
                    string histItem = History.GetPrevious();

                    if (histItem != null)
                        RewriteBuffer(histItem);
                    break;
                }

                case ConsoleKey.DownArrow:
                {
                    string histItem = History.GetNext();

                    if (histItem != null)
                        RewriteBuffer(histItem);
                    break;
                }

                case ConsoleKey.Enter:
                case ConsoleKey.C when (key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control:
                    SConsole.WriteLine();
                    StartPos = (SConsole.CursorLeft, SConsole.CursorTop);

                    if (key.Key == ConsoleKey.Enter)
                        OnLineInput(Buffer);

                    RewriteBuffer("");
                    break;

                default:
                    if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
                    {
                        SConsole.MoveBufferArea(SConsole.CursorLeft, SConsole.CursorTop, SConsole.BufferWidth - 1 - SConsole.CursorLeft, 1, SConsole.CursorLeft + 1, SConsole.CursorTop);

                        using (Buffer.IndexOf(' ', 0, CursorPos) == -1 ? SimpleConsoleColors.Yellow : SimpleConsoleColors.Cyan)
                            SConsole.Write(key.KeyChar.ToString());

                        Buffer = Buffer.Insert(CursorPos, key.KeyChar.ToString());

                        CursorPos++;
                    }
                    break;
            }
        }

        private void RewriteBuffer(string newStr)
        {
            string prevBuffer = Buffer;
            Buffer = newStr;

            int spaceIndex = newStr.IndexOf(' ');
            string cmdName = spaceIndex == -1 ? newStr : newStr.Substring(0, spaceIndex);
            string rest = spaceIndex == -1 ? "" : newStr.Substring(spaceIndex);

            SConsole.SetCursorPosition(StartPos.X, StartPos.Y);
            Write("> ", Color.DarkYellow);
            Write(cmdName, Color.Yellow);
            Write(rest, Color.Cyan);

            if (newStr.Length < prevBuffer.Length)
                SConsole.Write(new string(' ', prevBuffer.Length - newStr.Length));

            CursorPos = newStr.Length;
            SConsole.SetCursorPosition(StartPos.X + CursorPos + 2, StartPos.Y);
        }

        protected override void PauseInput()
        {
            IsInputPaused = true;

            SConsole.SetCursorPosition(StartPos.X, StartPos.Y);
            SConsole.Write(new string(' ', Buffer.Length + 2));
            SConsole.SetCursorPosition(StartPos.X, StartPos.Y);
        }

        protected override void ResumeInput()
        {
            IsInputPaused = false;

            StartPos = (SConsole.CursorLeft, SConsole.CursorTop);
            RewriteBuffer(Buffer);

            while (QueuedKeys.Count > 0)
                HandleKey(QueuedKeys.Dequeue());
        }

        public override void WriteLine(string str)
        {
            SConsole.WriteLine(str);
        }

        public override void Write(string str, Color color)
        {
            var prev = SConsole.ForegroundColor;
            SConsole.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color.ToString());
            SConsole.Write(str);
            SConsole.ForegroundColor = prev;
        }

        /// <summary>
        /// Fires up a console with the default settings.
        /// </summary>
        public static void StartDefault(string fileSystemRoot = null)
        {
            var frontend = new ConsoleFrontend();
            var console = fileSystemRoot == null ? new CommandConsole(frontend) : new CommandConsole(frontend, fileSystemRoot);
            console.Commands.RegisterCommandsInAllAssemblies();

            console.RunAutoexec();

            frontend.BeginRead();
        }
    }
}
