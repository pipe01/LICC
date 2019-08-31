using System.Collections.Generic;
using System.Linq;

namespace LICC.Internal.Parsing.Data
{
    internal class File
    {
        public IStatement[] Statements { get; }

        public File(IEnumerable<IStatement> statements)
        {
            this.Statements = statements.ToArray();
        }

        public File(IStatement[] statements)
        {
            this.Statements = statements;
        }
    }
}
