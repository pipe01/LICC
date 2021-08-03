using System;

namespace LICC.Console
{
    internal static class BuiltInCommands
    {
        [Command(Description = "Exit the program.")]
        static void Exit()
        {
            Environment.Exit(0);
        }

        [Command(Description = "Set the title of the window.")]
        static void SetWindowTitle(string newTitle)
        {
            System.Console.Title = newTitle;
        }
    }
}
