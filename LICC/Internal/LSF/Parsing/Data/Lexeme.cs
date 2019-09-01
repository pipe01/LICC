using System.Diagnostics;

namespace LICC.Internal.LSF.Parsing.Data
{
    internal enum LexemeKind
    {
        String,
        QuotedString,
        Keyword,
        Whitespace,
        Comma,

        Dollar,
        EqualsAssign,
        Hashtag,
        AtSign,
        Exclamation,
        QuestionMark,
        Colon,

        Equals,
        LessOrEqual,
        Less,
        MoreOrEqual,
        More,
        NotEqual,

        LeftParenthesis,
        RightParenthesis,
        LeftBrace,
        RightBrace,

        Plus,
        Minus,
        Multiply,
        Divide,

        And,
        AndAlso,
        Or,
        OrElse,

        NewLine,
        Semicolon,
        EndOfFile,
    }

    [DebuggerDisplay("{Kind}: {Content}")]
    internal sealed class Lexeme
    {
        public LexemeKind Kind { get; }
        public SourceLocation Begin { get; }
        public string Content { get; }

        public Lexeme(LexemeKind kind, SourceLocation begin, string content)
        {
            this.Kind = kind;
            this.Begin = begin;
            this.Content = content;
        }

        public override string ToString() => $"'{Content}' ({Kind})";
    }
}
