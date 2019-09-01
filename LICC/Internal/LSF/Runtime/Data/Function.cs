using LICC.Internal.LSF.Parsing.Data;

namespace LICC.Internal.LSF.Runtime.Data
{
    internal class Function
    {
        public Statement[] Statements { get; }
        public Parameter[] Parameters { get; }

        public Function(Statement[] statements, Parameter[] parameters)
        {
            this.Statements = statements;
            this.Parameters = parameters;
        }
    }
}
