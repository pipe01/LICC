using System.Collections;
using System.Collections.Generic;

namespace LICC.Internal.LSF.Data
{
    public class Error
    {
        public SourceLocation Location { get; }
        public string Message { get; }
        public Severity Severity { get; }

        internal Error(SourceLocation location, string message, Severity severity)
        {
            this.Location = location;
            this.Message = message;
            this.Severity = severity;
        }
    }

    public class ErrorSink : IEnumerable<Error>
    {
        private readonly List<Error> List = new List<Error>();

        internal ErrorSink()
        {
        }

        public IEnumerator<Error> GetEnumerator()
        {
            return ((IEnumerable<Error>)this.List).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Error>)this.List).GetEnumerator();
        }

        internal void Add(Error error) => List.Add(error);
    }

    public enum Severity
    {
        Message,
        Warning,
        Error
    }
}
