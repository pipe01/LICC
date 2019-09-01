using LICC.Internal.LSF.Parsing.Data;
using LICC.Internal.LSF.Runtime.Data;
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

        public void Run(File file)
        {
            Run(file.Statements);
        }

        private object Run(IEnumerable<Statement> statements, bool pushStack = true)
        {
            SourceLocation loc = default;

            if (pushStack)
                ContextStack.Push();

            try
            {
                foreach (var item in statements)
                {
                    loc = item.Location;

                    if (item is ReturnStatement ret)
                    {
                        return ret.Value == null ? null : Visit(ret.Value);
                    }

                    RunStatement(item);
                }
            }
            catch (Exception ex)
            {
                throw new AggregateException($"At line {loc.Line + 1}", ex);
            }
            finally
            {
                if (pushStack)
                    ContextStack.Pop();
            }

            return null;
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
            else if (statement is IfStatement ifStatement)
            {
                ContextStack.Push();

                try
                {
                    object condition = Visit(ifStatement.Condition);

                    if (condition is bool b ? b : (condition != null))
                        Run(ifStatement.Body);
                }
                finally
                {
                    ContextStack.Pop();
                }
            }
            else if (statement is WhileStatement whileStatement)
            {
                ContextStack.Push();

                try
                {
                    while (Visit<bool>(whileStatement.Condition))
                    {
                        Run(whileStatement.Body);
                    }
                }
                finally
                {
                    ContextStack.Pop();
                }
            }
            else if (statement is ForStatement forStatement)
            {
                DoForStatement(forStatement);
            }
            else if (statement is FunctionDeclarationStatement funcDeclare)
            {
                Context.Functions[funcDeclare.Name] = new Function(funcDeclare.Body.ToArray(), funcDeclare.Parameters.ToArray());
            }
        }

        private void DoForStatement(ForStatement forStatement)
        {
            ContextStack.Push();

            try
            {
                int from = forStatement.From == null ? 0 : Visit<int>(forStatement.From);
                int to = Visit<int>(forStatement.To);

                for (int i = to > from ? from : from - 1; to > from ? i < to : i >= to; i += to > from ? 1 : -1)
                {
                    ContextStack.SetVariable(forStatement.VariableName, i);

                    Run(forStatement.Body, false);
                }
            }
            finally
            {
                ContextStack.Pop();
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

        private T Visit<T>(Expression expr)
        {
            object val = Visit(expr);

            try
            {
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch (Exception ex)
            {
                throw new RuntimeException($"failed to convert '{val}' to a {typeof(T).Name}", ex);
            }
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
            else if (expr is UnaryOperatorExpression unary)
                return VisitUnaryOperator(unary);
            else if (expr is VariableAccessExpression varAcc)
                return VisitVariableAccess(varAcc);
            else if (expr is VariableAssignmentExpression varAss)
                return VisitVariableAssignment(varAss);
            else if (expr is FunctionCallExpression funcCall)
                return VisitFunctionCall(funcCall);

            throw null;
        }

        private object VisitFunctionCall(FunctionCallExpression funcCall)
        {
            if (!ContextStack.TryGetFunction(funcCall.FunctionName, out var func))
                throw new RuntimeException($"function with name '{funcCall.FunctionName}' not found");

            if (funcCall.Arguments.Length != func.Parameters.Length)
                throw new RuntimeException($"function '{funcCall.FunctionName}' expects {func.Parameters.Length} parameters but {funcCall.Arguments.Length} were found");

            ContextStack.Push();

            try
            {
                for (int i = 0; i < func.Parameters.Length; i++)
                {
                    object value = Visit(funcCall.Arguments[i]);
                    Context.Variables[func.Parameters[i].Name] = value;
                }

                return Run(func.Statements, false);
            }
            finally
            {
                ContextStack.Pop();
            }
        }

        private object VisitBinaryOperator(BinaryOperatorExpression expr)
        {
            if (expr.Operator == Operator.And)
            {
                if (!(Visit(expr.Left) is bool leftB))
                    throw new RuntimeException("invalid left operand type, expected boolean");

                if (!leftB)
                    return false;

                if (!(Visit(expr.Right) is bool rightB))
                    throw new RuntimeException("invalid right operand type, expected boolean");

                return rightB;
            }
            
            if (expr.Operator == Operator.Or)
            {
                if (!(Visit(expr.Left) is bool leftB))
                    throw new RuntimeException("invalid left operand type, expected boolean");

                if (leftB)
                    return true;

                if (!(Visit(expr.Right) is bool rightB))
                    throw new RuntimeException("invalid right operand type, expected boolean");

                return rightB;
            }

            object left = Visit(expr.Left);
            object right = Visit(expr.Right);

            if (expr.Operator == Operator.Equals)
            {
                return left == null ? right == null : left.Equals(right);
            }

            if (left is string leftStr)
            {
                switch (expr.Operator)
                {
                    case Operator.Divide:
                    case Operator.Subtract:
                        throw new RuntimeException("invalid operation");
                    case Operator.Multiply when (right is float f):
                        return string.Join("", Enumerable.Repeat(leftStr, (int)f));
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
                        throw new RuntimeException("invalid operation");
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

            throw new RuntimeException("invalid operator");
        }

        private object VisitUnaryOperator(UnaryOperatorExpression unary)
        {
            object operand = Visit(unary.Operand);

            switch (unary.Operator)
            {
                case Operator.Negate:
                    if (operand is bool b)
                        return !b;
                    else
                        throw new RuntimeException($"cannot negate '{operand}'");
            }

            throw new RuntimeException("invalid operator");
        }

        private object VisitVariableAccess(VariableAccessExpression expr)
        {
            if (ContextStack.TryGetVariable(expr.VariableName, out var val))
                return val;
            else
                return null; //Maybe throw instead?
        }

        private object VisitVariableAssignment(VariableAssignmentExpression expr)
        {
            var value = Visit(expr.Value);

            ContextStack.SetVariable(expr.VariableName, value);
            return value;
        }
    }
}
