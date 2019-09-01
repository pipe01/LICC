using System.Linq;

namespace LICC.Internal.Parsing.Data
{
    internal abstract class Expression
    {
        public virtual bool CanStandAlone => false;
    }

    internal class NumberLiteralExpression : Expression
    {
        public float Value { get; }

        public NumberLiteralExpression(float value)
        {
            this.Value = value;
        }

        public override string ToString() => Value.ToString();
    }

    internal class StringLiteralExpression : Expression
    {
        public string Value { get; }

        public StringLiteralExpression(string value)
        {
            this.Value = value;
        }

        public override string ToString() => Value;
    }

    internal class FunctionCallExpression : Expression
    {
        public override bool CanStandAlone => true;

        public string FunctionName { get; }
        public Expression[] Arguments { get; }

        public FunctionCallExpression(string functionName, Expression[] arguments)
        {
            this.FunctionName = functionName;
            this.Arguments = arguments;
        }

        public override string ToString() => $"{FunctionName}({string.Join(", ", Arguments.Select(o => o.ToString()))})";
    }

    internal class BinaryOperatorExpression : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }
        public Operator Operator { get; }

        public BinaryOperatorExpression(Expression left, Expression right, Operator @operator)
        {
            this.Left = left;
            this.Right = right;
            this.Operator = @operator;
        }

        public override string ToString() => $"({Left} {Operator} {Right})";
    }
}
