using System.Collections.Generic;

namespace LICC.Internal.Parsing.Data
{
    internal class File
    {
        public IEnumerable<IStatement> Statements { get; }

        public File(IEnumerable<IStatement> statements)
        {
            this.Statements = statements;
        }
    }
}
