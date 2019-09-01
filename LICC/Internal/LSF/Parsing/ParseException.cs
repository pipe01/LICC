using LICC.Internal.LSF.Parsing.Data;
using System;

namespace LICC.Internal.LSF.Parsing
{
    public class ParseException : Exception
    {
        public Error Error { get; }

        public override string Message => $"An error occurred while parsing on line {Error.Location.Line + 1}: {Error.Message}";

        public ParseException(Error error)
        {
            this.Error = error;
        }
    }
}
