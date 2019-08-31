﻿using LICC.Internal.Parsing.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal.Parsing
{
    internal class Lexer
    {
        private static readonly string[] Keywords = { "function" };

        private int Column;
        private int Index;
        private int Line;
        private char Char => Source[Index];

        private SourceLocation Location => new SourceLocation(Line, Column);
        private readonly StringBuilder Buffer = new StringBuilder();

        private bool IsEOF => Char == '\0';
        private bool IsNewLine => Char == '\n';
        private bool IsSymbol => "{}()+-;#".Contains(Char);
        private bool IsWhitespace => Char == ' ' || Char == '\t';
        private bool IsKeyword => Keywords.Contains(Buffer.ToString());

        public ErrorSink Errors { get; } = new ErrorSink();

        private readonly string Source;

        public Lexer(string source)
        {
            this.Source = source.Replace("\r\n", "\n");

            if (this.Source[this.Source.Length - 1] != '\0')
                this.Source += "\0";
        }

        public IEnumerable<Lexeme> Lex()
        {
            while (!IsEOF)
            {
                var lexeme = GetLexeme();

                if (lexeme != null)
                    yield return lexeme;
            }

            yield return Lexeme(LexemeKind.EndOfFile);
        }

        private void Advance()
        {
            Index++;
            Column++;
        }

        private void NewLine()
        {
            Line++;
            Column = 0;
            Index++;
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

        private void Error(string msg, Severity severity)
        {
            var error = new Error(Location, msg, severity);
            Errors.Add(error);

            if (severity == Severity.Error)
                throw new ParseException(error);
        }

        private Lexeme Lexeme(LexemeKind kind, string content = null)
        {
            content = content ?? Buffer.ToString();
            Buffer.Clear();

            var begin = Location;
            var end = new SourceLocation(Line, Column + content.Length);

            return new Lexeme(kind, begin, end, content);
        }

        private Lexeme GetLexeme()
        {
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
                var lexeme = Lexeme(LexemeKind.NewLine);
                Advance();
                return lexeme;
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
                case '+':
                    return Lexeme(LexemeKind.Plus);
                case '-':
                    return Lexeme(LexemeKind.Minus);
                case ';':
                    return Lexeme(LexemeKind.Semicolon);
                case '#':
                    return Lexeme(LexemeKind.Hashtag);
            }

            return null;
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

            while (!IsEOF && !IsNewLine && (isQuoted ? Char != quote : !IsWhitespace))
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

        private Lexeme DoKeyword()
        {
            throw new NotImplementedException();
        }
    }
}