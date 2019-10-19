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

    internal class MemberAccessExpression : Expression
    {
        public Expression Object { get; }
        public string MemberName { get; }

        public MemberAccessExpression(Expression obj, string memberName)
        {
            this.Object = obj;
            this.MemberName = memberName;
        }

        public override string ToString() => $"{Object}.{MemberName}";
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

    internal class MemberCallExpression : Expression
    {
        public Expression FunctionExpression { get; }
        public Expression[] Arguments { get; }

        public MemberCallExpression(Expression functionExpression, Expression[] arguments)
        {
            this.FunctionExpression = functionExpression;
            this.Arguments = arguments;
        }

        public override string ToString() => $"{FunctionExpression}({string.Join(", ", Arguments.Select(o => o.ToString()))})";
    }

    internal class CommandCallExpression : Expression
    {
        public string CommandName { get; }
        public Expression[] Arguments { get; }

        public CommandCallExpression(string commandName, Expression[] arguments)
        {
            this.CommandName = commandName;
            this.Arguments = arguments;
        }

        public override string ToString() => $"{CommandName}({string.Join(", ", Arguments.Select(o => o.ToString()))})";
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

    internal class TernaryOperatorExpression : Expression
    {
        public Expression Condition { get; }
        public Expression IfTrue { get; }
        public Expression IfFalse { get; }

        public TernaryOperatorExpression(Expression condition, Expression ifTrue, Expression ifFalse)
        {
            this.Condition = condition;
            this.IfTrue = ifTrue;
            this.IfFalse = ifFalse;
        }

        public override string ToString() => $"{Condition} ? {IfTrue} : {IfFalse}";
    }

    internal class VariableAssignmentExpression : Expression
    {
        public override bool CanStandAlone => true;

        public string VariableName { get; }
        public Expression Value { get; }
        public Operator? Operator { get; }

        public VariableAssignmentExpression(string variableName, Expression value, Operator? @operator)
        {
            this.VariableName = variableName;
            this.Value = value;
            this.Operator = @operator;
        }

        public override string ToString() => $"(${VariableName} {Operator}= {Value})";
    }
}
