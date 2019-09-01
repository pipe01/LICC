using System.Collections.Generic;

namespace LICC.Internal.LSF.Parsing.Data
{
    internal abstract class Statement
    {
        public SourceLocation Location { get; set; }
    }

    internal class CommentStatement : Statement
    {
    }

    internal class FunctionDeclarationStatement : Statement
    {
        public string Name { get; }
        public IEnumerable<Statement> Statements { get; }
        public IEnumerable<Parameter> Parameters { get; }

        public FunctionDeclarationStatement(string name, IEnumerable<Statement> statements, IEnumerable<Parameter> parameters)
        {
            this.Name = name;
            this.Statements = statements;
            this.Parameters = parameters;
        }
    }

    internal class CommandStatement : Statement
    {
        public string CommandName { get; }
        public Expression[] Arguments { get; }

        public CommandStatement(string commandName, Expression[] arguments)
        {
            this.CommandName = commandName;
            this.Arguments = arguments;
        }
    }

    internal class ExpressionStatement : Statement
    {
        public Expression Expression { get; }

        public ExpressionStatement(Expression expression)
        {
            this.Expression = expression;
        }
    }

    internal class ReturnStatement : Statement
    {
        public Expression Value { get; }

        public ReturnStatement(Expression value)
        {
            this.Value = value;
        }
    }
}
