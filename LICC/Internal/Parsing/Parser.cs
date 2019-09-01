using LICC.Internal.Parsing.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private void AdvanceUntil(LexemeKind kind)
        {
            while (Current.Kind != kind)
                Advance();
        }

        private void Back()
        {
            Index--;
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

                var st = GetStatement();

                if (!(st is CommentStatement))
                    statements.Add(st);
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
                    SkipWhitespaces();

                    if (Current.Kind != LexemeKind.String && Current.Kind != LexemeKind.QuotedString)
                        return null;

                    return DoCommand();
                case LexemeKind.Hashtag:
                    AdvanceUntil(LexemeKind.NewLine);
                    return new CommentStatement();
                case LexemeKind.Exclamation:
                    Advance();
                    return new ExpressionStatement(DoFunctionCall());
            }

            return null;
        }

        private FunctionDeclarationStatement DoFunction()
        {
            TakeKeyword("function");

            string name = Take(LexemeKind.String, "function name").Content;
            //Take(LexemeKind.Whitespace, "whitespace after function name", false);

            var statements = new List<IStatement>();
            var parameters = new List<Parameter>();

            Take(LexemeKind.LeftParenthesis, "parameter list opening");

            while (true)
            {
                SkipWhitespaces();
                if (Current.Kind != LexemeKind.String)
                    break;

                parameters.Add(DoParameter());

                SkipWhitespaces();
                if (Current.Kind != LexemeKind.Comma)
                    break;
                else
                    Advance();
            }

            Take(LexemeKind.RightParenthesis, "parameter list closing");
            SkipWhitespaces();

            Take(LexemeKind.LeftBrace, "function body opening");
            SkipWhitespaces();

            IStatement statement;
            while ((statement = GetStatement()) != null)
            {
                if (statement is FunctionDeclarationStatement)
                    Error("cannot declare function inside function");

                if (!(statement is CommentStatement))
                    statements.Add(statement);
            }

            Take(LexemeKind.RightBrace, "function body closing");

            return new FunctionDeclarationStatement(name, statements, parameters);
        }

        private Parameter DoParameter()
        {
            string type = Take(LexemeKind.String, "parameter type").Content;
            string name = Take(LexemeKind.String, "parameter name").Content;

            return new Parameter(type, name);
        }

        private CommandStatement DoCommand()
        {
            string cmdName = Take(LexemeKind.String).Content;
            var args = DoArguments();

            if (Current.Kind == LexemeKind.Semicolon)
                Advance();
            else if (Current.Kind == LexemeKind.Hashtag)
                AdvanceUntil(LexemeKind.NewLine);

            return new CommandStatement(cmdName, args.ToArray());
        }

        private IEnumerable<Expression> DoArguments()
        {
            var args = new List<Expression>();

            Expression expr;
            while ((expr = DoExpression()) != null)
                args.Add(expr);

            return args;
        }

        private Expression DoExpression()
        {
            if (Take(LexemeKind.LeftParenthesis, out _))
            {
                var expr = DoExpression();

                Take(LexemeKind.RightParenthesis, "closing parentheses");

                return expr;
            }


            if (Take(LexemeKind.String, out var str))
            {
                if (float.TryParse(str.Content, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                    return new NumberLiteralExpression(f);
            }
            else if (Take(LexemeKind.QuotedString, out var quotedStr))
            {
                return new StringLiteralExpression(quotedStr.Content);
            }
            else if (Take(LexemeKind.Exclamation, out _))
            {
                return DoFunctionCall();
            }
            else if (Take(LexemeKind.Keyword, out var keyword))
            {
                if (keyword.Content != "true" && keyword.Content != "false")
                    Error($"unexpected keyword: '{keyword.Content}'");

                return new BooleanLiteralExpression(keyword.Content == "true");
            }

            return null;
        }

        private FunctionCallExpression DoFunctionCall()
        {
            string funcName = Take(LexemeKind.String, "function name", false).Content;
            var args = DoArguments();

            return new FunctionCallExpression(funcName, args.ToArray());
        }
    }
}
