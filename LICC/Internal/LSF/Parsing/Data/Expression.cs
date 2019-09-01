using System.Linq;

namespace LICC.Internal.LSF.Parsing.Data
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

    internal class BooleanLiteralExpression : Expression
    {
        public bool Value { get; }

        public BooleanLiteralExpression(bool value)
        {
            this.Value = value;
        }

        public override string ToString() => Value ? "true" : "false";
    }

    internal class NullExpression : Expression
    {
    }

    internal class VariableAccessExpression : Expression
    {
        public string VariableName { get; }

        public VariableAccessExpression(string variableName)
        {
            this.VariableName = variableName;
        }

        public override string ToString() => "$" + VariableName;
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

    internal class UnaryOperatorExpression : Expression
    {
        public Operator Operator { get; }
        public Expression Operand { get; }

        public UnaryOperatorExpression(Operator @operator, Expression operand)
        {
            this.Operator = @operator;
            this.Operand = operand;
        }

        public override string ToString() => $"({Operator} {Operand})";
    }

    internal class VariableAssignmentExpression : Expression
    {
        public override bool CanStandAlone => true;

        public string VariableName { get; }
        public Expression Value { get; }

        public VariableAssignmentExpression(string variableName, Expression value)
        {
            this.VariableName = variableName;
            this.Value = value;
        }

        public override string ToString() => $"(${VariableName} = {Value})";
    }
}
