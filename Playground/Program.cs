using LICC;
using LICC.API;
using LICC.Console;
using System;
using System.Linq;
using System.Reflection;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleImplementation.StartDefault();
        }

        [Command]
        public static void Test(int number, string str = "default")
        {
            LConsole.WriteLine($"Hello {number} {str}", Color.Blue);
        }
    }
}
