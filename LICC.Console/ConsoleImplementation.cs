using LICC.API;
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
            while (true)
            {
                string buffer = "";
                ConsoleKeyInfo key;
                int cursorPos = 0;

                Write("> ", ConsoleColor.DarkYellow);

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
                    else if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
                    {
                        SConsole.MoveBufferArea(SConsole.CursorLeft, SConsole.CursorTop, SConsole.BufferWidth - 1 - SConsole.CursorLeft, 1, SConsole.CursorLeft + 1, SConsole.CursorTop);
                        Write(key.KeyChar, buffer.IndexOf(' ', 0, cursorPos) == -1 ? ConsoleColor.Yellow : ConsoleColor.Cyan);

                        buffer = buffer.Insert(cursorPos, key.KeyChar.ToString());

                        cursorPos++;
                    }
                }

                SConsole.WriteLine();
                OnLineInput(buffer);
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
