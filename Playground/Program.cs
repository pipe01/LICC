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
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    LConsole.WriteLine("hello");
                }
            })
            {
                IsBackground = true
            }.Start();

            ConsoleImplementation.StartDefault();
        }

        [Command]
        public static void Test(int number, string str = "default")
        {
            LConsole.WriteLine($"Hello {number} {str}", Color.Blue);
        }
    }
}
