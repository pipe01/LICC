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

                //Thread.Sleep(2000);
                //LConsole.WriteLine("Starting...");

                //using (var writer = LConsole.BeginWrite())
                //{
                //    Color[] colors = { Color.Blue, Color.Red, Color.Yellow, Color.Magenta, Color.Green };
                //    for (int i = 0; i < 5; i++)
                //    {
                //        Thread.Sleep(1000);
                //        writer.Write(i + " ", colors[i]);
                //    }
                //}
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
