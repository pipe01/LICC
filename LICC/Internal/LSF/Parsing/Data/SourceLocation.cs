﻿namespace LICC.Internal.LSF.Parsing.Data
{
    public readonly struct SourceLocation
    {
        public int Line { get; }
        public int Column { get; }

        public SourceLocation(int line, int column)
        {
            this.Line = line;
            this.Column = column;
        }
    }
}