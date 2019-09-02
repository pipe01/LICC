using LICC.API;
using LICC.Internal.LSF.Parsing;
using LICC.Internal.LSF.Parsing.Data;
using LICC.Internal.LSF.Runtime;
using System;
using System.Diagnostics;
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
        private readonly IFileSystem FileSystem;

        public LsfRunner(IEnvironment environment, ICommandRegistryInternal commandRegistry, IFileSystem fileSystem)
        {
            this.Interpreter = new Interpreter(commandRegistry, environment);
            this.Preprocessor = new Preprocessor(fileSystem);
            this.FileSystem = fileSystem;
        }

        public void Run(string fileName)
        {
            var lexemes = Lexer.Lex(fileName, FileSystem).ToArray();
            File ast;

            try
            {
                ast = Parser.ParseFile(lexemes);
            }
            catch (ParseException ex) when (!Debugger.IsAttached)
            {
                LConsole.WriteLine(ex.Message, ConsoleColor.Red);
                return;
            }

            Interpreter.Run(ast);
        }
    }
}
