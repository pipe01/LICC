using LICC;
using LICC.API;
using LICC.Console;
using LICC.Internal.LSF.Parsing;
using LICC.Internal.LSF.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleFrontend.StartDefault("cfg", RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        }

        [Command]
        public static void Test(int num, string str = "default")
        {
            LConsole.WriteLine($"Hello {num}, {str}", ConsoleColor.Blue);
        }
    }
}
