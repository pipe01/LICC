using LICC.Internal.LSF.Parsing.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LICC.Internal.LSF.Runtime
{
    internal class Interpreter
    {
        private readonly ContextStack ContextStack = new ContextStack();

        private RunContext Context => ContextStack.Peek();


        private readonly ICommandRegistryInternal CommandRegistry;

        public Interpreter(ICommandRegistryInternal commandRegistry)
        {
            this.CommandRegistry = commandRegistry;
        }

        public void Run(IEnumerable<Statement> statements)
        {
            SourceLocation loc = default;

            ContextStack.Push();

            try
            {
                foreach (var item in statements)
                {
                    loc = item.Location;
                    RunStatement(item);
                }
            }
            catch (Exception ex)
            {
                throw new AggregateException($"At line {loc.Line + 1}", ex);
            }
            finally
            {
                ContextStack.Pop();
            }
        }

        private void RunStatement(Statement statement)
        {
            if (statement is CommandStatement cmd)
            {
                RunCommand(cmd);
            }
            else if (statement is ExpressionStatement exprStatement)
            {
                Visit(exprStatement.Expression);
            }
        }

        private void RunCommand(CommandStatement statement)
        {
            if (!CommandRegistry.TryGetCommand(statement.CommandName, out var cmd))
                throw new RuntimeException($"command with name '{statement.CommandName}' not found");

            if (statement.Arguments.Length < cmd.Params.Count(o => !o.Optional))
                throw new RuntimeException("argument count mismatch");

            object[] args = Enumerable.Repeat(Type.Missing, cmd.Params.Length).ToArray();

            for (int i = 0; i < statement.Arguments.Length; i++)
            {
                object argValue = Visit(statement.Arguments[i]);

                if (argValue is float && cmd.Params[i].Type == typeof(string))
                {
                    args[i] = argValue.ToString();
                    continue;
                }

                try
                {
                    args[i] = Convert.ChangeType(argValue, cmd.Params[i].Type);
                }
                catch (Exception ex)
                {
                    throw new RuntimeException($"failed to convert parameter {cmd.Params[i].Name}'s value", ex);
                }
            }

            cmd.Method.Invoke(null, args);
        }

        private object Visit(Expression expr)
        {
            if (expr is StringLiteralExpression str)
                return str.Value;
            else if (expr is NumberLiteralExpression num)
                return num.Value;
            else if (expr is BooleanLiteralExpression boo)
                return boo.Value;
            else if (expr is BinaryOperatorExpression bin)
                return VisitBinaryOperator(bin);
            else if (expr is VariableAccessExpression varAcc)
                return VisitVariableAccess(varAcc);
            else if (expr is VariableAssignmentExpression varAss)
                return VisitVariableAssignment(varAss);

            throw null;
        }

        private object VisitBinaryOperator(BinaryOperatorExpression expr)
        {
            object left = Visit(expr.Left);
            object right = Visit(expr.Right);

            if (left is string leftStr)
            {
                switch (expr.Operator)
                {
                    case Operator.Multiply:
                    case Operator.Divide:
                    case Operator.Subtract:
                        throw null;
                    case Operator.Add:
                        return leftStr + right;
                }
            }
            else if (right is string rightStr)
            {
                switch (expr.Operator)
                {
                    case Operator.Multiply:
                    case Operator.Divide:
                    case Operator.Subtract:
                        throw null;
                    case Operator.Add:
                        return left + rightStr;
                }
            }
            else if (left is float leftNum)
            {
                if (right is float rightNum)
                {
                    switch (expr.Operator)
                    {
                        case Operator.Subtract:
                            return leftNum - rightNum;
                        case Operator.Add:
                            return leftNum + rightNum;
                        case Operator.Divide:
                            return leftNum / rightNum;
                        case Operator.Multiply:
                            return leftNum * rightNum;
                    }
                }
            }

            throw null;
        }

        private object VisitVariableAccess(VariableAccessExpression expr)
        {
            if (ContextStack.TryGetValue(expr.VariableName, out var val))
                return val;
            else
                return null; //Maybe throw instead?
        }

        private object VisitVariableAssignment(VariableAssignmentExpression expr)
        {
            var value = Visit(expr.Value);

            return Context.Variables[expr.VariableName] = value;
        }
    }
}
