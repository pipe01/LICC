using LICC.API;
using SimpleConsoleColor;
using System;
using System.Linq;
using System.Reflection;
using SConsole = System.Console;

namespace LICC.Console
{
    public class ConsoleImplementation : Frontend
    {
        public void BeginRead()
        {
            (int X, int Y) startPos;
            string buffer;
            int cursorPos;

            while (true)
            {
                ConsoleKeyInfo key;

                buffer = "";
                cursorPos = 0;

                Write("> ", ConsoleColor.DarkYellow);
                startPos = (SConsole.CursorLeft, SConsole.CursorTop);

                while ((key = SConsole.ReadKey(true)).Key != ConsoleKey.Enter)
                {
                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (buffer.Length > 0 && cursorPos > 0)
                        {
                            int charsToDelete = 1;

                            if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                            {
                                int spaceIndex = buffer.LastIndexOf(' ', cursorPos - 1);

                                charsToDelete = spaceIndex != -1 ? buffer.Length - spaceIndex : cursorPos;
                            }

                            SConsole.MoveBufferArea(SConsole.CursorLeft, SConsole.CursorTop, SConsole.BufferWidth - SConsole.CursorLeft, 1, SConsole.CursorLeft - charsToDelete, SConsole.CursorTop);

                            SConsole.CursorLeft -= charsToDelete;
                            cursorPos -= charsToDelete;

                            buffer = buffer.Substring(0, buffer.Length - charsToDelete);
                        }
                    }
                    else if (key.Key == ConsoleKey.Delete)
                    {
                        if (buffer.Length > 0)
                        {
                            int charsToDelete = 1;

                            if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                            {
                                int spaceIndex = buffer.IndexOf(' ', cursorPos);

                                charsToDelete = spaceIndex != -1 ? spaceIndex - cursorPos : buffer.Length - cursorPos;
                            }

                            buffer = buffer.Substring(0, cursorPos) + buffer.Substring(cursorPos + charsToDelete);
                            SConsole.MoveBufferArea(SConsole.CursorLeft + charsToDelete, SConsole.CursorTop, SConsole.BufferWidth - SConsole.CursorLeft - charsToDelete, 1, SConsole.CursorLeft, SConsole.CursorTop);
                        }
                    }
                    else if (key.Key == ConsoleKey.LeftArrow)
                    {
                        if (cursorPos > 0)
                        {
                            int charsToMove = 1;

                            if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                            {
                                int spaceIndex = buffer.LastIndexOf(' ', cursorPos - 2);

                                charsToMove = spaceIndex != -1 ? cursorPos - spaceIndex - 1 : cursorPos;
                            }

                            cursorPos -= charsToMove;
                            SConsole.CursorLeft -= charsToMove;
                        }
                    }
                    else if (key.Key == ConsoleKey.RightArrow)
                    {
                        if (cursorPos < buffer.Length)
                        {
                            int charsToMove = 1;

                            if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                            {
                                int spaceIndex = buffer.IndexOf(' ', cursorPos + 1);

                                charsToMove = spaceIndex != -1 ? spaceIndex - cursorPos : buffer.Length - cursorPos;
                            }

                            cursorPos += charsToMove;
                            SConsole.CursorLeft += charsToMove;
                        }
                    }
                    else if (key.Key == ConsoleKey.UpArrow)
                    {
                        string histItem = History.GetPrevious();

                        if (histItem != null)
                            RewriteBuffer(histItem);
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        string histItem = History.GetNext();

                        if (histItem != null)
                            RewriteBuffer(histItem);
                    }
                    else if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
                    {
                        SConsole.MoveBufferArea(SConsole.CursorLeft, SConsole.CursorTop, SConsole.BufferWidth - 1 - SConsole.CursorLeft, 1, SConsole.CursorLeft + 1, SConsole.CursorTop);

                        using (buffer.IndexOf(' ', 0, cursorPos) == -1 ? SimpleConsoleColors.Yellow : SimpleConsoleColors.Cyan)
                            Write(key.KeyChar);

                        buffer = buffer.Insert(cursorPos, key.KeyChar.ToString());

                        cursorPos++;
                    }
                }

                SConsole.WriteLine();
                OnLineInput(buffer);
            }

            void RewriteBuffer(string newStr)
            {
                string prevBuffer = buffer;
                buffer = newStr;

                int spaceIndex = newStr.IndexOf(' ');
                string cmdName = spaceIndex == -1 ? newStr : newStr.Substring(0, spaceIndex);
                string rest = spaceIndex == -1 ? "" : newStr.Substring(spaceIndex);

                SConsole.SetCursorPosition(startPos.X, startPos.Y);
                Write(cmdName, ConsoleColor.Yellow);
                Write(rest, ConsoleColor.Cyan);

                if (newStr.Length < prevBuffer.Length)
                    SConsole.Write(new string(' ', prevBuffer.Length - newStr.Length));

                cursorPos = newStr.Length;
                SConsole.SetCursorPosition(startPos.X + cursorPos, startPos.Y);
            }

            void Write(object obj, ConsoleColor? color = null)
            {
                ConsoleColor prev = SConsole.ForegroundColor;
                if (color != null)
                {
                    SConsole.ForegroundColor = color.Value;
                }
                SConsole.Write(obj);
                SConsole.ForegroundColor = prev;
            }
        }

        public override void Write(string str, Color color)
        {
            var prev = SConsole.ForegroundColor;
            SConsole.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color.ToString());
            SConsole.Write(str);
            SConsole.ForegroundColor = prev;
        }

        public static void StartDefault()
        {
            var frontend = new ConsoleImplementation();
            var console = new CommandConsole(frontend);
            console.Commands.RegisterCommandsIn(Assembly.GetCallingAssembly());

            frontend.BeginRead();
        }
    }
}
