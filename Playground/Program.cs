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
            var l = new Lexer(@"test (2 * 5 / $asd) (2 + 'asd')

$hello = !asd 2 3").Lex().ToArray();

            var p = new Parser().ParseFile(l);

            var console = new CommandConsole(new ConsoleFrontend(true));
            console.Commands.RegisterCommandsInAllAssemblies();

            new Interpreter(console.CommandRegistry).Run(p.Statements);

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
