using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal.LSF.Data
{
    internal interface IStatement
    {
    }

    internal class CommentStatement : IStatement
    {
    }

    internal class FunctionDeclarationStatement : IStatement
    {
        public string Name { get; }
        public IEnumerable<IStatement> Statements { get; }
        public IEnumerable<Parameter> Parameters { get; }

        public FunctionDeclarationStatement(string name, IEnumerable<IStatement> statements, IEnumerable<Parameter> parameters)
        {
            this.Name = name;
            this.Statements = statements;
            this.Parameters = parameters;
        }
    }

    internal class CommandStatement : IStatement
    {
        public string CommandName { get; }
        public Expression[] Arguments { get; }

        public CommandStatement(string commandName, Expression[] arguments)
        {
            this.CommandName = commandName;
            this.Arguments = arguments;
        }
    }

    internal class ExpressionStatement : IStatement
    {
        public Expression Expression { get; }

        public ExpressionStatement(Expression expression)
        {
            this.Expression = expression;
        }
    }
}
