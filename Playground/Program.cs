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
    public class TestClass
    {
        public int Test = 69;

        public object DoTest(object asd)
        {
            LConsole.WriteLine("Function called: " + asd);
            return this;
        }
    }

    class Program
    {
        private static CommandConsole Console;

        static void Main(string[] args)
        {
            ConsoleFrontend.StartDefault(out Console, "cfg", true);
        }

        [Command]
        public static object Test(int num, object str = null)
        {
            LConsole.WriteLine($"Hello {num}, {str} ({str?.GetType()})", ConsoleColor.Blue);

            if (num == 42)
                FrontendManager.Frontend = new PlainTextConsoleFrontend();

            return new TestClass();
        }
    }
}
