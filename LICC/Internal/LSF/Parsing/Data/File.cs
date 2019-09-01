using System.Collections.Generic;
using System.Linq;

namespace LICC.Internal.LSF.Parsing.Data
{
    internal class File
    {
        public Statement[] Statements { get; }

        public File(IEnumerable<Statement> statements)
        {
            this.Statements = statements.ToArray();
        }

        public File(Statement[] statements)
        {
            this.Statements = statements;
        }
    }
}
