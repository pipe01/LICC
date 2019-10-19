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
        static void Main(string[] args)
        {
            ConsoleFrontend.StartDefault("cfg", true);
        }

        [Command]
        public static object Test(int num, string str = "default")
        {
            LConsole.WriteLine($"Hello {num}, {str}", ConsoleColor.Blue);
            return new TestClass();
        }
    }
}
