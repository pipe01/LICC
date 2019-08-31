using LICC.Internal.Parsing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal.Parsing
{
    internal class Parser
    {
        private int Index;
        private Lexeme Current => Lexemes[Index];
        private SourceLocation Location => Current.Begin;

        private Lexeme[] Lexemes;

        #region Utils
        private void Advance()
        {
            Index++;

            if (Index >= Lexemes.Length)
                Index = Lexemes.Length - 1;
        }

        private void SkipWhitespaces(bool alsoNewlines = true)
        {
            while (Current.Kind == LexemeKind.Whitespace || (Current.Kind == LexemeKind.NewLine && alsoNewlines))
                Advance();
        }

        private Lexeme TakeAny()
        {
            var token = Current;
            Advance();
            return token;
        }

        private Lexeme Take(LexemeKind lexemeKind, string expected = null, bool ignoreWhitespace = true)
        {
            if (ignoreWhitespace)
                SkipWhitespaces();

            if (Current.Kind != lexemeKind)
                Error($"Unexpected lexeme: expected {expected ?? lexemeKind.ToString()}, found {Current.Kind}");

            return TakeAny();
        }

        private bool Take(LexemeKind lexemeKind, out Lexeme lexeme, bool ignoreWhitespace = true)
        {
            if (ignoreWhitespace)
                SkipWhitespaces();

            if (Current.Kind == lexemeKind)
            {
                lexeme = TakeAny();
                return true;
            }

            lexeme = null;
            return false;
        }

        private Lexeme TakeKeyword(string keyword, bool ignoreWhitespace = true, bool @throw = true, string msg = null)
        {
            if (ignoreWhitespace)
                SkipWhitespaces();

            if (Current.Kind != LexemeKind.Keyword || Current.Content != keyword)
            {
                if (@throw)
                    Error(msg ?? $"Unexpected lexeme: expected '{keyword}' keyword, found {Current}");
                else
                    return null;
            }

            return TakeAny();
        }

        private void Error(string msg) => Error(msg, Location);
        private void Error(string msg, SourceLocation location)
        {
            throw new ParseException(new Error(location, msg, Severity.Error));
        }
        #endregion

        public File ParseFile(Lexeme[] lexemes)
        {
            Index = 0;
            Lexemes = lexemes;

            var statements = new List<IStatement>();

            while (Current.Kind != LexemeKind.EndOfFile)
            {
                SkipWhitespaces();

                statements.Add(GetStatement());
            }

            return new File(statements);
        }

        private IStatement GetStatement()
        {
            switch (Current.Kind)
            {
                case LexemeKind.Keyword:
                    return DoFunction();
                case LexemeKind.String:
                    return DoStatement();
            }

            return null;
        }

        private FunctionDeclarationStatement DoFunction()
        {
            TakeKeyword("function");

            string name = Take(LexemeKind.String, "function name").Content;
            Take(LexemeKind.Whitespace, "whitespace after function name", false);

            var statements = new List<IStatement>();

            Take(LexemeKind.LeftParenthesis, "parameters list opening");

            Take(LexemeKind.RightParenthesis, "parameters list closing");
            SkipWhitespaces();
            Take(LexemeKind.LeftBrace, "function body opening");
            SkipWhitespaces();

            IStatement statement;
            while ((statement = DoStatement()) != null)
                statements.Add(statement);

            Take(LexemeKind.RightBrace, "function body closing");

            return new FunctionDeclarationStatement(name, statements);
        }

        private IStatement DoStatement()
        {
            SkipWhitespaces();

            if (Current.Kind != LexemeKind.String && Current.Kind != LexemeKind.QuotedString)
                return null;

            if (Current.Content.Length > 0 && Current.Content[0] == '$')
                return DoVariable();
            else
                return DoCommand();
        }

        private CommandStatement DoCommand()
        {
            string cmdName = Take(LexemeKind.String).Content;
            var args = new List<string>();

            while (Current.Kind != LexemeKind.NewLine && Current.Kind != LexemeKind.Semicolon)
            {
                if (Take(LexemeKind.String, out var l) || Take(LexemeKind.QuotedString, out l))
                    args.Add(l.Content);
            }

            if (Current.Kind == LexemeKind.Semicolon)
                Advance();

            return new CommandStatement(cmdName, args.ToArray());
        }

        private IStatement DoVariable()
        {
            throw new NotImplementedException();
        }
    }
}
