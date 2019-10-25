using LICC.Internal.LSF.Parsing.Data;
using LICC.Internal.LSF.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        private SourceLocation Location;
        private bool StrictMode = false;
        private int FunctionCallDepth = 0;

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
            try
            {
                Run(file.Statements, false);
            }
            catch (ReturnException)
            {
            }
        }

        private void Run(IEnumerable<Statement> statements, bool isFunc, bool pushStack = true)
        {
            if (pushStack)
                ContextStack.Push();

            try
            {
                foreach (var item in statements)
                {
                    Location = item.Location;

                    RunStatement(item);
                }
            }
            catch (Exception ex) when (!(ex is ReturnException) && !(ex is RuntimeException))
            {
                throw Error(ex.Message, ex);
            }
            finally
            {
                if (pushStack)
                    ContextStack.Pop();
            }
        }

        private Exception Error(string msg, Exception innerException = null)
        {
            var callStack = new StringBuilder();

            callStack.Append("  at ").AppendLine(Location.ToString());

            string prev = null, desc;
            int sameContextCount = 0;
            foreach (var item in ContextStack)
            {
                if (item.Type == RunContextType.Function)
                {
                    desc = "  at " + item.Descriptor;

                    if (desc == prev)
                    {
                        sameContextCount++;

                        if (sameContextCount < 4)
                            callStack.AppendLine(desc);
                    }
                    else if (sameContextCount > 4)
                    {
                        callStack.Append("  ...x").Append(sameContextCount - 4).AppendLine();
                        sameContextCount = 0;
                    }
                    else
                    {
                        callStack.AppendLine(desc);
                    }

                    prev = desc;
                }
            }

            return new RuntimeException($"Fatal runtime exception occurred: " + msg + System.Environment.NewLine + callStack, innerException);
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
                throw new ReturnException(ret.Value == null ? null : Visit(ret.Value));
            }
            else if (statement is DirectiveStatement dir)
            {
                DoDirective(dir);
            }
        }

        private void DoDirective(DirectiveStatement dir)
        {
            switch (dir.Name)
            {
                case "use":
                    if (dir.Arguments.Length == 1 && dir.Arguments[0] == "strict")
                        StrictMode = true;
                    else
                        throw Error("invalid mode");
                    break;
                default:
                    throw Error("invalid directive");
            }
        }

        private void DoWhileStatement(WhileStatement whileStatement)
        {
            ContextStack.Push();

            try
            {
                while (IsTruthy(Visit(whileStatement.Condition)))
                {
                    Run(whileStatement.Body, false);
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
                    Run(ifStatement.Body, false);
                }
                else
                {
                    if (ifStatement.Else is ElseIfStatement elseIf)
                    {
                        DoIfStatement(new IfStatement(elseIf.Condition, elseIf.Body, elseIf.Else));
                    }
                    else if (ifStatement.Else is ElseStatement @else)
                    {
                        Run(@else.Body, false);
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

                    Run(forStatement.Body, false, false);
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
                throw Error($"failed to convert '{val}' to a {typeof(T).Name}", ex);
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
            else if (expr is MemberAccessExpression memAccess)
                return VisitMemberAccess(memAccess);
            else if (expr is MemberCallExpression memCall)
                return VisitMemberCall(memCall);

            throw Error("invalid expression?");
        }

        private object VisitMemberCall(MemberCallExpression memCall)
        {
            if (!(memCall.FunctionExpression is MemberAccessExpression member))
                throw Error("expected a method access expression");

            string funcName = member.MemberName;
            var obj = Visit(member.Object);

            var args = new object[memCall.Arguments.Length];

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = Visit(memCall.Arguments[i]);
            }

            return obj.GetType().GetMethod(funcName).Invoke(obj, args);
        }

        private object VisitMemberAccess(MemberAccessExpression memAccess)
        {
            var obj = Visit(memAccess.Object);

            if (obj == null)
                throw Error($"null reference to '{memAccess.Object}'");

            var type = obj.GetType();

            var prop = type.GetProperty(memAccess.MemberName);
            if (prop != null)
                return prop.GetValue(obj);

            var field = type.GetField(memAccess.MemberName);
            if (field != null)
                return field.GetValue(obj);

            var method = type.GetMethod(memAccess.MemberName);
            if (method != null)
                return method;

            throw Error($"field or property '{memAccess.MemberName}' not found");
        }

        private object VisitCommandCall(CommandCallExpression statement)
        {
            if (!CommandRegistry.TryGetCommand(statement.CommandName, statement.Arguments.Length, out var cmd))
                throw Error($"command with name '{statement.CommandName}' and {statement.Arguments.Length} parameters not found");

            if (statement.Arguments.Length < cmd.Params.Count(o => !o.Optional))
                throw Error("argument count mismatch");

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
                    throw Error($"failed to convert parameter {cmd.Params[i].Name}'s value", ex);
                }
            }

            return CommandExecutor.Execute(cmd, args);
        }

        private object VisitFunctionCall(FunctionCallExpression funcCall)
        {
            if (FunctionCallDepth >= 700)
                throw Error("max call stack size reached");

            if (!ContextStack.TryGetFunction(funcCall.FunctionName, out var func))
                throw Error($"function with name '{funcCall.FunctionName}' not found");

            int requiredParamCount = func.Parameters.Count(o => !o.IsOptional);
            int totalParamCount = func.Parameters.Length;

            if (funcCall.Arguments.Length < requiredParamCount)
                throw Error($"function '{funcCall.FunctionName}' expects {(requiredParamCount == totalParamCount ? requiredParamCount.ToString() : $"between {requiredParamCount} and {totalParamCount}")} parameters but {funcCall.Arguments.Length} were found");

            ContextStack.Push(new RunContext(RunContextType.Function,
                funcCall.FunctionName + "!(" +
                string.Join(", ", func.Parameters.Select(o => o.Name)) +
                ") in " + Location));

            try
            {
                for (int i = 0; i < func.Parameters.Length; i++)
                {
                    object value = i < funcCall.Arguments.Length
                        ? Visit(funcCall.Arguments[i])
                        : Visit(func.Parameters[i].DefaultValue);

                    Context.SetVariable(func.Parameters[i].Name, value);
                }

                object retValue = null;

                FunctionCallDepth++;

                try
                {
                    Run(func.Statements, false);
                }
                catch (ReturnException ex)
                {
                    retValue = ex.Value;
                }
                finally
                {
                    FunctionCallDepth--;
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
                    throw Error("invalid left operand type, expected boolean");

                if (!leftB)
                    return false;

                if (!(Visit(expr.Right) is bool rightB))
                    throw Error("invalid right operand type, expected boolean");

                return rightB;
            }
            
            if (expr.Operator == Operator.Or)
            {
                if (!(Visit(expr.Left) is bool leftB))
                    throw Error("invalid left operand type, expected boolean");

                if (leftB)
                    return true;

                if (!(Visit(expr.Right) is bool rightB))
                    throw Error("invalid right operand type, expected boolean");

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
                        throw Error("invalid operation");
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
                        throw Error("invalid operation");
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

            throw Error("invalid operator or operand types");
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
                        throw Error($"cannot negate '{operand}'");
                case Operator.IncrementByOne:
                case Operator.DecrementByOne:
                    if (!(expr.Operand is VariableAccessExpression var))
                    {
                        throw Error("expression on the left side of an increment or decrement operator must be a variable reference");
                    }
                    else
                    {
                        return VisitVariableAssignment(
                           new VariableAssignmentExpression(var.VariableName,
                           new BinaryOperatorExpression(var,
                           new NumberLiteralExpression(expr.Operator == Operator.IncrementByOne ? 1 : -1), Operator.Add), null));
                    }
            }

            throw Error("invalid operator");
        }

        private object VisitTernaryOperator(TernaryOperatorExpression expr)
        {
            object condValue = Visit(expr.Condition);

            return IsTruthy(condValue) ? Visit(expr.IfTrue) : Visit(expr.IfFalse);
        }

        private object VisitVariableAccess(VariableAccessExpression expr)
        {
            if (ContextStack.TryGetVariable(expr.VariableName, out var val))
            {
                return val;
            }
            else
            {
                if (StrictMode)
                    throw Error($"tried to access undeclared variable ${expr.VariableName}");
                else
                    return null;
            }
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

        private static bool IsTruthy(object obj) => obj is bool b ? b : obj != null;
    }
}
