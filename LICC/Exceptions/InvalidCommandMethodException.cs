using System;

namespace LICC.Exceptions
{
    internal sealed class InvalidCommandMethodException : Exception
    {
        public InvalidCommandMethodException()
        {
        }

        public InvalidCommandMethodException(string message) : base(message)
        {
        }

        public InvalidCommandMethodException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
