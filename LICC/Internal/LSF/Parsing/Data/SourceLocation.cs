namespace LICC.Internal.LSF.Parsing.Data
{
    public readonly struct SourceLocation
    {
        public int Line { get; }
        public int Column { get; }
        public string FileName { get; }

        public SourceLocation(int line, int column, string fileName)
        {
            this.Line = line;
            this.Column = column;
            this.FileName = fileName;
        }

        public override string ToString() => $"{FileName}:{Line + 1}";
    }
}
