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

$hello = true
$hello = !$hello

if ($iuh)
{
    echo 'yes'
}

echo $hello
echo !true

function repeat(str, count) {
    return $str * $count
}

echo (!repeat 'hello ' 3)

function myFunc(param) {
    echo ('inside function! ' + $param)
    echo 'this is great' #comment

    function innerFunc(asd) {
        echo 'first: ' + $param
        echo 'second: ' + $asd
    }

    !innerFunc 123
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
