using System.Diagnostics;

namespace LICC.Internal.Parsing.Data
{
    internal enum LexemeKind
    {
        String,
        QuotedString,
        Keyword,
        Whitespace,

        LeftParenthesis,
        RightParenthesis,
        LeftBrace,
        RightBrace,
        Plus,
        Minus,

        NewLine,
        Semicolon,
        EndOfFile,
    }

    [DebuggerDisplay("{Kind}: {Content}")]
    internal sealed class Lexeme
    {
        public LexemeKind Kind { get; }
        public SourceLocation Begin { get; }
        public SourceLocation End { get; }
        public string Content { get; }

        public Lexeme(LexemeKind kind, SourceLocation begin, SourceLocation end, string content)
        {
            this.Kind = kind;
            this.Begin = begin;
            this.End = end;
            this.Content = content;
        }
    }
}
