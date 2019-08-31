using LICC;
using LICC.API;
using LICC.Console;
using LICC.Internal.Parsing;
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
            var l = new Lexer(@"
function asd () {
    hello
}").Lex().ToArray();

            var p = new Parser().ParseFile(l);

            ConsoleFrontend.StartDefault("cfg", true);
        }

        [Command]
        public static void Test(int num, string str = "default")
        {
            LConsole.WriteLine($"Hello {num}, {str}", ConsoleColor.Blue);
        }
    }
}
