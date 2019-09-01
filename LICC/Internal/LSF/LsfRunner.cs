using LICC.Internal.LSF.Parsing;
using LICC.Internal.LSF.Runtime;
using System.Linq;

namespace LICC.Internal.LSF
{
    internal interface ILsfRunner
    {
        void Run(string fileContents);
    }

    internal class LsfRunner : ILsfRunner
    {
        private readonly Parser Parser = new Parser();
        private readonly Interpreter Interpreter;

        public LsfRunner(IEnvironment environment, ICommandRegistryInternal commandRegistry)
        {
            this.Interpreter = new Interpreter(commandRegistry, environment);
        }

        public void Run(string fileContents)
        {
            var lexemes = Lexer.Lex(fileContents).ToArray();
            var ast = Parser.ParseFile(lexemes);
            Interpreter.Run(ast);
        }
    }
}
