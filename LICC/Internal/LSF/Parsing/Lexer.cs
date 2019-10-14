using LICC.API;
using LICC.Internal.LSF.Parsing.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LICC.Internal.LSF.Parsing
{
    internal class Lexer
    {
        private static readonly string[] Keywords = { "function", "true", "false", "null", "return", "if", "else", "for", "from", "to", "while" };

        private int Column;
        private int Index;
        private int Line;
        private char Char => Source[Index];

        private SourceLocation Location => new SourceLocation(Line, Column, FileName);

        private readonly StringBuilder Buffer = new StringBuilder();

        private bool IsEOF => Char == '\0';
        private bool IsNewLine => Char == '\n';
        private bool IsSymbol => "{}()<>+-*/;#,!$=&|@?:%".Contains(Char);
        private bool IsWhitespace => Char == ' ' || Char == '\t';
        private bool IsKeyword => Keywords.Contains(Buffer.ToString());

        public ErrorSink Errors { get; } = new ErrorSink();

        private readonly string Source;
        private readonly string FileName;
        private readonly IFileSystem FileSystem;

        public Lexer(string source, string fileName, IFileSystem fileSystem)
        {
            this.Source = source.Replace("\r\n", "\n");
            this.FileName = fileName;
            this.FileSystem = fileSystem;

            if (this.Source[this.Source.Length - 1] != '\0')
                this.Source += "\0";
        }

        public IEnumerable<Lexeme> Lex()
        {
            Lexeme prev = null;

            while (!IsEOF)
            {
                int beginIndex = Index;
                var lexeme = GetLexeme();

                if (lexeme != null)
                {
                    if (FileSystem != null && (prev == null || prev.Kind == LexemeKind.NewLine) && lexeme.Kind == LexemeKind.AtSign)
                    {
                        foreach (var item in TryIncludeFile())
                        {
                            yield return item;
                        }
                    }
                    else
                    {
                        yield return lexeme;
                        prev = lexeme;
                    }
                }
            }

            yield return Lexeme(LexemeKind.EndOfFile);

            IEnumerable<Lexeme> TryIncludeFile()
            {
                var directiveLexeme = GetLexeme();

                if (directiveLexeme.Kind == LexemeKind.String && directiveLexeme.Content == "include")
                {
                    GetLexeme(); //Skip whitespace

                    var fileNameLexeme = GetLexeme();

                    if (fileNameLexeme.Kind == LexemeKind.QuotedString)
                    {
                        if (!FileSystem.FileExists(fileNameLexeme.Content))
                            Error($"cannot find file on '{fileNameLexeme.Content}'");

                        foreach (var item in Lex(fileNameLexeme.Content, FileSystem))
                        {
                            if (item.Kind == LexemeKind.EndOfFile)
                                break;

                            yield return item;
                        }
                    }
                    else
                    {
                        Error($"expected quoted file path, found {fileNameLexeme}");
                    }
                }
                else
                {
                    Error($"invalid preprocessor directive '{directiveLexeme.Content}'");
                }
            }
        }

        public static IEnumerable<Lexeme> Lex(string fileName, IFileSystem fileSystem)
        {
            return new Lexer(fileSystem.ReadAllText(fileName), fileName, fileSystem).Lex();
        }

        private void Advance()
        {
            Index++;
            Column++;
        }

        private void Back()
        {
            Index--;
            Column--;

            if (Column < 0)
            {
                Column = 0;
                Line--;
            }
        }

        private char Consume()
        {
            Buffer.Append(Char);
            return Take();
        }

        private char Take()
        {
            char c = Char;
            Advance();
            return c;
        }

        private void Error(string msg, Severity severity = Severity.Error)
        {
            var error = new Error(Location, msg, severity);
            Errors.Add(error);

            if (severity == Severity.Error)
                throw new ParseException(error);
        }

        private Lexeme Lexeme(LexemeKind kind, string content = null)
        {
            if (kind == LexemeKind.NewLine)
            {
                Column = 0;
                Line++;
            }

            content = content ?? kind.GetCharacter() ?? Buffer.ToString();
            Buffer.Clear();

            return new Lexeme(kind, Location, content);
        }

        private Lexeme GetLexeme()
        {
            if (IsEOF)
            {
                return Lexeme(LexemeKind.EndOfFile);
            }
            if (IsWhitespace)
            {
                return DoWhitespace();
            }
            if (IsSymbol)
            {
                return DoSymbol();
            }
            else if (IsNewLine)
            {
                Advance();
                return Lexeme(LexemeKind.NewLine);
            }
            else
            {
                return DoString();
            }
        }

        private Lexeme DoWhitespace()
        {
            while (IsWhitespace)
                Consume();

            return Lexeme(LexemeKind.Whitespace);
        }

        private Lexeme DoSymbol()
        {
            switch (Take())
            {
                case '(':
                    return Lexeme(LexemeKind.LeftParenthesis);
                case ')':
                    return Lexeme(LexemeKind.RightParenthesis);
                case '{':
                    return Lexeme(LexemeKind.LeftBrace);
                case '}':
                    return Lexeme(LexemeKind.RightBrace);
                case '*':
                    return Lexeme(LexemeKind.Multiply);
                case '/':
                    return Lexeme(LexemeKind.Divide);
                case ';':
                    return Lexeme(LexemeKind.Semicolon);
                case '#':
                    return Lexeme(LexemeKind.Hashtag);
                case ',':
                    return Lexeme(LexemeKind.Comma);
                case '$':
                    return Lexeme(LexemeKind.Dollar);
                case '@':
                    return Lexeme(LexemeKind.AtSign);
                case '?':
                    return Lexeme(LexemeKind.QuestionMark);
                case ':':
                    return Lexeme(LexemeKind.Colon);
                case '%':
                    return Lexeme(LexemeKind.Percentage);
                case '+':
                    return TwoCharOperator("++", LexemeKind.Plus, LexemeKind.Increment);
                case '-':
                    return TwoCharOperator("--", LexemeKind.Minus, LexemeKind.Decrement);
                case '&':
                    return TwoCharOperator("&&", LexemeKind.And, LexemeKind.AndAlso);
                case '|':
                    return TwoCharOperator("||", LexemeKind.Or, LexemeKind.OrElse);
                case '!':
                    return TwoCharOperator("!=", LexemeKind.Exclamation, LexemeKind.NotEqual);
                case '=':
                    return TwoCharOperator("==", LexemeKind.EqualsAssign, LexemeKind.Equals);
                case '<':
                    return TwoCharOperator("<=", LexemeKind.Less, LexemeKind.LessOrEqual);
                case '>':
                    return TwoCharOperator(">=", LexemeKind.More, LexemeKind.MoreOrEqual);
            }

            return null;

            Lexeme TwoCharOperator(string full, LexemeKind firstKind, LexemeKind secondKind)
            {
                if (Consume() == full[1])
                    return Lexeme(secondKind, full);

                Back();
                return Lexeme(firstKind, full[0].ToString());
            }
        }

        private Lexeme DoString()
        {
            char quote = '\0';
            bool isQuoted = false;

            if (Char == '"' || Char == '\'')
            {
                quote = Char;
                isQuoted = true;
                Advance();
            }

            while (!IsEOF && !IsNewLine && (isQuoted ? Char != quote : (!IsWhitespace && !IsSymbol)))
            {
                if (Consume() == '\\' && !IsEOF)
                    Consume();
            }

            if (isQuoted)
            {
                if (IsEOF || Char != quote)
                    Error("missing closing quote", Severity.Error);

                Advance();
            }

            return Lexeme(isQuoted
                ? LexemeKind.QuotedString
                : (IsKeyword
                    ? LexemeKind.Keyword
                    : LexemeKind.String));
        }
    }
}
