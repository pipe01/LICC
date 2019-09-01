using LICC.API;
using LICC.Internal.LSF.Parsing;
using LICC.Internal.LSF.Parsing.Data;
using LICC.Internal.LSF.Runtime;
using System;
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
        private readonly IPreprocessor Preprocessor;
        public LsfRunner(IEnvironment environment, ICommandRegistryInternal commandRegistry, IFileSystem fileSystem)
        {
            this.Interpreter = new Interpreter(commandRegistry, environment);
            this.Preprocessor = new Preprocessor(fileSystem);
        }

        public void Run(string fileContents)
        {
            string processed;

            try
            {
                processed = Preprocessor.Process(fileContents);
            }
            catch (PreprocessorException ex)
            {
                LConsole.WriteLine("Error while preprocessing the file: " + ex.Message, ConsoleColor.Red);
                return;
            }

            var lexemes = Lexer.Lex(processed).ToArray();
            File ast;

            try
            {
                ast = Parser.ParseFile(lexemes);
            }
            catch (ParseException ex)
            {
                LConsole.WriteLine(ex.Message, ConsoleColor.Red);
                return;
            }

            Interpreter.Run(ast);
        }
    }
}
