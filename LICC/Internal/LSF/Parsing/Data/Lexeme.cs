using System.Diagnostics;

namespace LICC.Internal.LSF.Parsing.Data
{
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
