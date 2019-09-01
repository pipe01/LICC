using LICC.Internal.LSF.Parsing.Data;
using System;

namespace LICC.Internal.LSF.Parsing
{
    public class ParseException : Exception
    {
        public Error Error { get; }

        public override string Message => $"An error occurred on line {Error.Location.Line + 1} while parsing file '{Error.Location.FileName}': {Error.Message}";

        public ParseException(Error error)
        {
            this.Error = error;
        }
    }
}
