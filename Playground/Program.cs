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
            var l = new Lexer(@"! hello 'first' 2 ""third""

function asd (Type name, Type2 name2) {
    hello 123 ('nice')
}


this 123 'is' (""gret"")").Lex().ToArray();

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
