using LICC;
using LICC.API;
using LICC.Console;
using LICC.Internal.LSF.Parsing;
using LICC.Internal.LSF.Runtime;
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
            var l = new Lexer(@"test (200 * 5 / ($asd = 123)) (2 + 'asd')

$hello = 123

function myFunc(param) {
    echo ('inside function! ' + $param)
}

!myFunc 'a parameter'

test $hello 'asd' * 2").Lex().ToArray();

            var p = new Parser().ParseFile(l);

            var console = new CommandConsole(new ConsoleFrontend(true));
            console.Commands.RegisterCommandsInAllAssemblies();

            new Interpreter(console.CommandRegistry).Run(p);

            //ConsoleFrontend.StartDefault("cfg", true);
            Console.ReadKey(true);
        }

        [Command]
        public static void Test(int num, string str = "default")
        {
            LConsole.WriteLine($"Hello {num}, {str}", ConsoleColor.Blue);
        }
    }
}
