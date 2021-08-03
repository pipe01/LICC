using System;

namespace LICC.Console
{
    internal static class BuiltInCommands
    {
        [Command]
        private static void Exit()
        {
            Environment.Exit(0);
        }
    }
}
