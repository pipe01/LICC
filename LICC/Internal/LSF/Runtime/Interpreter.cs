using LICC.Internal.LSF.Parsing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal.LSF.Runtime
{
    internal class Interpreter
    {
        private readonly ICommandRegistryInternal CommandRegistry;

        public Interpreter(ICommandRegistryInternal commandRegistry)
        {
            this.CommandRegistry = commandRegistry;
        }

        public void Run(IEnumerable<IStatement> statements)
        {
            foreach (var item in statements)
            {
                RunStatement(item);
            }
        }

        private void RunStatement(IStatement statement)
        {
            if (statement is CommandStatement cmd)
            {
                RunCommand(cmd);
            }
        }

        private void RunCommand(CommandStatement statement)
        {
            if (!CommandRegistry.TryGetCommand(statement.CommandName, out var cmd))
                throw null;

            if (statement.Arguments.Length < cmd.Params.Count(o => !o.Optional))
                throw null;

            object[] args = Enumerable.Repeat(Type.Missing, cmd.Params.Length).ToArray();

            for (int i = 0; i < statement.Arguments.Length; i++)
            {
                object argValue = Visit(statement.Arguments[i]);

                try
                {
                    args[i] = Convert.ChangeType(argValue, cmd.Params[i].Type);
                }
                catch (Exception)
                {
                    throw null;
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
    }
}
