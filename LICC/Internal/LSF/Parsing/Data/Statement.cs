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
        public IEnumerable<Statement> Body { get; }
        public IEnumerable<Parameter> Parameters { get; }

        public FunctionDeclarationStatement(string name, IEnumerable<Statement> body, IEnumerable<Parameter> parameters)
        {
            this.Name = name;
            this.Body = body;
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

    internal class IfStatement : Statement
    {
        public Expression Condition { get; }
        public IEnumerable<Statement> Body { get; }

        public IfStatement(Expression condition, IEnumerable<Statement> body)
        {
            this.Condition = condition;
            this.Body = body;
        }
    }

    internal class ForStatement : Statement
    {
        public string VariableName { get; }
        public Expression From { get; }
        public Expression To { get; }
        public IEnumerable<Statement> Body { get; }

        public ForStatement(string variableName, Expression from, Expression to, IEnumerable<Statement> body)
        {
            this.VariableName = variableName;
            this.From = from;
            this.To = to;
            this.Body = body;
        }
    }
}
