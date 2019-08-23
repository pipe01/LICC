using LICC;
using LICC.API;
using LICC.Console;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleFrontend.StartDefault("cfg");
        }

        [Command]
        public static void Test(string str = "default")
        {
            LConsole.WriteLine($"Hello {str}", Color.Blue);
        }
    }
}
