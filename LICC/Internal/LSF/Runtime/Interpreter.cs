using LICC.Internal.LSF.Parsing.Data;
using LICC.Internal.LSF.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LICC.Internal.LSF.Runtime
{
    internal class Interpreter
    {
        private class ReturnException : Exception
        {
            public readonly object Value;

            public ReturnException(object value)
            {
                this.Value = value;
            }
        }

        private readonly ContextStack ContextStack = new ContextStack();

        private IRunContext Context => ContextStack.Peek();


        private readonly ICommandRegistryInternal CommandRegistry;

        public Interpreter(ICommandRegistryInternal commandRegistry, IEnvironment environment)
        {
            this.CommandRegistry = commandRegistry;

            ContextStack.Push(environment);
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

                    RunStatement(item);
                }
            }
            catch (Exception ex) when (!(ex is ReturnException))
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
                DoIfStatement(ifStatement);
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
                Context.SetFunction(funcDeclare.Name, new Function(funcDeclare.Body.ToArray(), funcDeclare.Parameters.ToArray()));
            }
            else if (statement is ReturnStatement ret)
            {
                throw new ReturnException(ret.Value == null ? null : Visit(ret.Value));
            }
        }

        private void DoIfStatement(IfStatement ifStatement)
        {
            ContextStack.Push();

            try
            {
                object condition = Visit(ifStatement.Condition);
                bool isTrue = condition is bool b ? b : (condition != null);

                if (isTrue)
                {
                    Run(ifStatement.Body);
                }
                else
                {
                    if (ifStatement.Else is ElseIfStatement elseIf)
                    {
                        DoIfStatement(new IfStatement(elseIf.Condition, elseIf.Body, elseIf.Else));
                    }
                    else if (ifStatement.Else is ElseStatement @else)
                    {
                        Run(@else.Body);
                    }
                }
            }
            finally
            {
                ContextStack.Pop();
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
                    ContextStack.SetVariable(forStatement.VariableName, (float)i);

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
            else if (expr is NullExpression)
                return null;
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

            throw new RuntimeException("invalid expression?");
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
                    Context.SetVariable(func.Parameters[i].Name, value);
                }

                object retValue;

                try
                {
                    retValue = Run(func.Statements, false);
                }
                catch (ReturnException ex)
                {
                    retValue = ex.Value;
                }

                return retValue;
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

            if (expr.Operator == Operator.Equal)
            {
                return left == null ? right == null : left.Equals(right);
            }

            if (expr.Operator == Operator.Equal)
                return left == null ? right == null : left.Equals(right);
            else if (expr.Operator == Operator.NotEqual)
                return left == null ? right != null : !left.Equals(right);

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
            else if (left is float leftNum && right is float rightNum)
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

                    case Operator.Less:
                        return leftNum < rightNum;
                    case Operator.LessOrEqual:
                        return leftNum <= rightNum;
                    case Operator.More:
                        return leftNum > rightNum;
                    case Operator.MoreOrEqual:
                        return leftNum >= rightNum;
                }
            }

            throw new RuntimeException("invalid operator or operand types");
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
