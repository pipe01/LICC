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
        public ElseStatement Else { get; }

        public IfStatement(Expression condition, IEnumerable<Statement> body, ElseStatement @else)
        {
            this.Condition = condition;
            this.Body = body;
            this.Else = @else;
        }
    }

    internal class ElseIfStatement : ElseStatement
    {
        public Expression Condition { get; }
        public ElseStatement Else { get; }

        public ElseIfStatement(Expression condition, IEnumerable<Statement> body, ElseStatement @else) : base(body)
        {
            this.Condition = condition;
            this.Else = @else;
        }
    }

    internal class ElseStatement : Statement
    {
        public IEnumerable<Statement> Body { get; }

        public ElseStatement(IEnumerable<Statement> body)
        {
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

    internal class WhileStatement : Statement
    {
        public Expression Condition { get; }
        public IEnumerable<Statement> Body { get; }

        public WhileStatement(Expression condition, IEnumerable<Statement> body)
        {
            this.Condition = condition;
            this.Body = body;
        }
    }
}
