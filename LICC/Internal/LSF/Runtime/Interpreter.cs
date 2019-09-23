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
        private bool InFunction;


        private readonly ICommandRegistryInternal CommandRegistry;
        private readonly ICommandExecutor CommandExecutor;

        public Interpreter(ICommandRegistryInternal commandRegistry, IEnvironment environment, ICommandExecutor commandExecutor)
        {
            this.CommandRegistry = commandRegistry;
            this.CommandExecutor = commandExecutor;

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
            catch (RuntimeException ex)
            {
                throw new RuntimeException($"At line {loc.Line + 1}: " + ex.Message);
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
            if (statement is ExpressionStatement exprStatement)
            {
                Visit(exprStatement.Expression);
            }
            else if (statement is IfStatement ifStatement)
            {
                DoIfStatement(ifStatement);
            }
            else if (statement is WhileStatement whileStatement)
            {
                DoWhileStatement(whileStatement);
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
                if (InFunction)
                    throw new ReturnException(ret.Value == null ? null : Visit(ret.Value));
                else
                    throw new RuntimeException("unexpected return statement");
            }
        }

        private void DoWhileStatement(WhileStatement whileStatement)
        {
            ContextStack.Push();

            try
            {
                while (IsTruthy(Visit(whileStatement.Condition)))
                {
                    Run(whileStatement.Body);
                }
            }
            finally
            {
                ContextStack.Pop();
            }
        }

        private void DoIfStatement(IfStatement ifStatement)
        {
            ContextStack.Push();

            try
            {
                object condition = Visit(ifStatement.Condition);

                if (IsTruthy(condition))
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
            else if (expr is TernaryOperatorExpression ternary)
                return VisitTernaryOperator(ternary);
            else if (expr is VariableAccessExpression varAcc)
                return VisitVariableAccess(varAcc);
            else if (expr is VariableAssignmentExpression varAss)
                return VisitVariableAssignment(varAss);
            else if (expr is FunctionCallExpression funcCall)
                return VisitFunctionCall(funcCall);
            else if (expr is CommandCallExpression cmdCall)
                return VisitCommandCall(cmdCall);

            throw new RuntimeException("invalid expression?");
        }

        private object VisitCommandCall(CommandCallExpression statement)
        {
            if (!CommandRegistry.TryGetCommand(statement.CommandName, statement.Arguments.Length, out var cmd))
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

            return CommandExecutor.Execute(cmd, args);
        }

        private object VisitFunctionCall(FunctionCallExpression funcCall)
        {
            if (!ContextStack.TryGetFunction(funcCall.FunctionName, out var func))
                throw new RuntimeException($"function with name '{funcCall.FunctionName}' not found");

            int requiredParamCount = func.Parameters.Count(o => !o.IsOptional);
            int totalParamCount = func.Parameters.Length;

            if (funcCall.Arguments.Length < requiredParamCount)
                throw new RuntimeException($"function '{funcCall.FunctionName}' expects {(requiredParamCount == totalParamCount ? requiredParamCount.ToString() : $"between {requiredParamCount} and {totalParamCount}")} parameters but {funcCall.Arguments.Length} were found");

            ContextStack.Push();
            InFunction = true;

            try
            {
                for (int i = 0; i < func.Parameters.Length; i++)
                {
                    object value;

                    if (i < funcCall.Arguments.Length)
                        value = Visit(funcCall.Arguments[i]);
                    else
                        value = Visit(func.Parameters[i].DefaultValue);

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
                InFunction = false;
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
                    case Operator.Modulo:
                        return leftNum % rightNum;

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

        private object VisitUnaryOperator(UnaryOperatorExpression expr)
        {
            object operand = Visit(expr.Operand);

            switch (expr.Operator)
            {
                case Operator.Negate:
                    if (operand is bool b)
                        return !b;
                    else
                        throw new RuntimeException($"cannot negate '{operand}'");
                case Operator.IncrementByOne:
                case Operator.DecrementByOne:
                    if (!(expr.Operand is VariableAccessExpression var))
                    {
                        throw new RuntimeException("expression on the left side of an increment operator must be a variable reference");
                    }
                    else
                    {
                        return VisitVariableAssignment(
                           new VariableAssignmentExpression(var.VariableName,
                           new BinaryOperatorExpression(var,
                           new NumberLiteralExpression(expr.Operator == Operator.IncrementByOne ? 1 : -1), Operator.Add), null));
                    }
            }

            throw new RuntimeException("invalid operator");
        }

        private object VisitTernaryOperator(TernaryOperatorExpression expr)
        {
            object condValue = Visit(expr.Condition);

            return IsTruthy(condValue) ? Visit(expr.IfTrue) : Visit(expr.IfFalse);
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

            if (expr.Operator != null)
            {
                value = VisitBinaryOperator(new BinaryOperatorExpression(new VariableAccessExpression(expr.VariableName), expr.Value, expr.Operator.Value));
            }

            ContextStack.SetVariable(expr.VariableName, value);
            return value;
        }

        private static bool IsTruthy(object obj) => (obj is bool b && b) && obj != null;
    }
}
