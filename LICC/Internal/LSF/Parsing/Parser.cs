using LICC.Internal.LSF.Parsing.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal.LSF.Parsing
{
    internal class Parser
    {
        private static readonly IDictionary<LexemeKind, Operator> LexemeOperatorMap = new Dictionary<LexemeKind, Operator>
        {
            [LexemeKind.Plus] = Operator.Add,
            [LexemeKind.Minus] = Operator.Subtract,
            [LexemeKind.Multiply] = Operator.Multiply,
            [LexemeKind.Divide] = Operator.Divide,
            [LexemeKind.AndAlso] = Operator.And,
            [LexemeKind.OrElse] = Operator.Or,
            [LexemeKind.Equals] = Operator.Equal,
            [LexemeKind.NotEqual] = Operator.NotEqual,
            [LexemeKind.Less] = Operator.Less,
            [LexemeKind.LessOrEqual] = Operator.LessOrEqual,
            [LexemeKind.More] = Operator.More,
            [LexemeKind.MoreOrEqual] = Operator.MoreOrEqual,
            [LexemeKind.Percentage] = Operator.Modulo,
        };
        private static readonly int MaxOperatorValue = ((Operator[])Enum.GetValues(typeof(Operator))).Max(o => (int)o);

        private int Index;
        private Lexeme Current => Lexemes[Index];
        private SourceLocation Location => Current.Begin;

        private Lexeme[] Lexemes;
        private Stack<int> IndexStack = new Stack<int>();

        #region Utils
        [DebuggerStepThrough]
        private void Advance()
        {
            Index++;

            if (Index >= Lexemes.Length)
                Index = Lexemes.Length - 1;
        }

        [DebuggerStepThrough]
        private void AdvanceUntil(LexemeKind kind)
        {
            while (Current.Kind != kind)
                Advance();
        }

        [DebuggerStepThrough]
        private void Back()
        {
            Index--;
        }

        [DebuggerStepThrough]
        private void Push() => IndexStack.Push(Index);

        [DebuggerStepThrough]
        private void Pop(bool set = true)
        {
            int i = IndexStack.Pop();
            if (set)
                Index = i;
        }

        [DebuggerStepThrough]
        private void SkipWhitespaces(bool alsoNewlines = true)
        {
            while (Current.Kind == LexemeKind.Whitespace || (Current.Kind == LexemeKind.NewLine && alsoNewlines))
                Advance();
        }

        [DebuggerStepThrough]
        private Lexeme TakeAny()
        {
            var token = Current;
            Advance();
            return token;
        }

        [DebuggerStepThrough]
        private Lexeme Take(LexemeKind lexemeKind, string expected = null, bool ignoreWhitespace = true)
        {
            if (ignoreWhitespace)
                SkipWhitespaces();

            if (Current.Kind != lexemeKind)
            {
                var lexemeChar = lexemeKind.GetCharacter();
                Error($"expected {expected ?? lexemeKind.ToString()}{(lexemeChar != null ? $" '{lexemeChar}'" : "")}, found '{Current.Content}' ({Current.Kind})");
            }

            return TakeAny();
        }

        [DebuggerStepThrough]
        private bool Take(LexemeKind lexemeKind, out Lexeme lexeme, bool ignoreWhitespace = true)
        {
            Push();

            if (ignoreWhitespace)
                SkipWhitespaces();

            if (Current.Kind == lexemeKind)
            {
                lexeme = TakeAny();
                Pop(false);
                return true;
            }

            lexeme = null;
            Pop();
            return false;
        }

        [DebuggerStepThrough]
        private bool Peek(LexemeKind kind, bool ignoreWhitespace = true)
        {
            if (ignoreWhitespace)
                SkipWhitespaces();

            return Index < Lexemes.Length - 1 && Lexemes[Index + 1].Kind == kind;
        }

        [DebuggerStepThrough]
        private Lexeme TakeKeyword(string keyword, bool ignoreWhitespace = true, bool @throw = true, string msg = null)
        {
            if (ignoreWhitespace)
                SkipWhitespaces();

            if (Current.Kind != LexemeKind.Keyword || Current.Content != keyword)
            {
                if (@throw)
                    Error(msg ?? $"expected keyword '{keyword}', found {Current}");
                else
                    return null;
            }

            return TakeAny();
        }

        [DebuggerStepThrough]
        private bool TakeKeyword(string keyword, out Lexeme lexeme, bool ignoreWhitespace = true)
        {
            if (ignoreWhitespace)
                SkipWhitespaces();

            if (Current.Kind == LexemeKind.Keyword && Current.Content == keyword)
            {
                lexeme = Current;
                TakeAny();
                return true;
            }

            lexeme = null;
            return false;
        }

        private void Error(string msg) => Error(msg, Location);
        private void Error(string msg, SourceLocation location)
        {
            throw new ParseException(new Error(location, msg, Severity.Error));
        }
        #endregion

        public File ParseFile(Lexeme[] lexemes)
        {
            Index = 0;
            Lexemes = lexemes;

            var statements = new List<Statement>();

            while (Current.Kind != LexemeKind.EndOfFile)
            {
                SkipWhitespaces();

                var st = GetStatement(false);

                if (st != null && !(st is CommentStatement))
                    statements.Add(st);
            }

            return new File(statements);
        }

        private Statement GetStatement(bool pardonInvalid = true)
        {
            Statement ret = null;
            var loc = Current.Begin;

            SkipWhitespaces();

            switch (Current.Kind)
            {
                case LexemeKind.EndOfFile:
                    return null;

                case LexemeKind.Hashtag:
                    AdvanceUntil(LexemeKind.NewLine);
                    ret = new CommentStatement();
                    break;

                case LexemeKind.Keyword when Current.Content == "return":
                    Advance();
                    SkipWhitespaces(false);

                    ret = new ReturnStatement(Current.Kind == LexemeKind.NewLine ? null : DoExpression());
                    break;

                case LexemeKind.Keyword when Current.Content == "function":
                    ret = DoFunction();
                    break;
                    
                case LexemeKind.Keyword when Current.Content == "if":
                    ret = DoIfStatement();
                    break;
                    
                case LexemeKind.Keyword when Current.Content == "while":
                    ret = DoWhileStatement();
                    break;
                    
                case LexemeKind.Keyword when Current.Content == "for":
                    ret = DoForStatement();
                    break;

                case LexemeKind.Keyword:
                    Error("unexpected keyword: " + Current.Content);
                    break;

                case LexemeKind.String:
                    SkipWhitespaces();

                    if (Current.Kind != LexemeKind.String && Current.Kind != LexemeKind.QuotedString)
                        return null;

                    ret = Peek(LexemeKind.Exclamation)
                            ? new ExpressionStatement(DoFunctionCall())
                            : new ExpressionStatement(DoCommandExpression());

                    break;

                case LexemeKind.Dollar:
                    Advance();
                    ret = new ExpressionStatement(DoVariableAssign());
                    break;
            }

            if (ret != null)
                ret.Location = loc;
            else if (!pardonInvalid)
                Error($"unexpected {Current.Kind} found");

            return ret;
        }

        private FunctionDeclarationStatement DoFunction()
        {
            TakeKeyword("function");

            string name = Take(LexemeKind.String, "function name").Content;
            //Take(LexemeKind.Whitespace, "whitespace after function name", false);

            var parameters = new List<Parameter>();

            Take(LexemeKind.LeftParenthesis, "parameter list opening");

            bool anyOptionalParam = false;
            while (true)
            {
                SkipWhitespaces();
                if (Current.Kind != LexemeKind.String)
                    break;

                var param = DoParameter();

                if (!param.IsOptional && anyOptionalParam)
                    Error("required parameters must appear before optional parameters");

                if (param.IsOptional)
                    anyOptionalParam = true;

                parameters.Add(param);

                SkipWhitespaces();
                if (Current.Kind != LexemeKind.Comma)
                    break;
                else
                    Advance();
            }

            Take(LexemeKind.RightParenthesis, "parameter list closing");
            SkipWhitespaces();

            var statements = DoStatementBlock(true);

            return new FunctionDeclarationStatement(name, statements, parameters);
        }

        private IfStatement DoIfStatement()
        {
            TakeKeyword("if");
            Take(LexemeKind.LeftParenthesis, "condition opening");

            var condition = DoExpression();

            Take(LexemeKind.RightParenthesis, "condition closing");

            var body = DoStatementBlock();
            ElseStatement @else = null;

            if (TakeKeyword("else", out _))
            {
                if (TakeKeyword("if", out _))
                {
                    Back();
                    var @if = DoIfStatement();
                    @else = new ElseIfStatement(@if.Condition, @if.Body, @if.Else);
                }
                else
                {
                    @else = new ElseStatement(DoStatementBlock());
                }
            }

            return new IfStatement(condition, body, @else);
        }
        
        private WhileStatement DoWhileStatement()
        {
            TakeKeyword("while");
            Take(LexemeKind.LeftParenthesis, "condition opening");

            var condition = DoExpression();

            Take(LexemeKind.RightParenthesis, "condition opening");

            var body = DoStatementBlock();

            return new WhileStatement(condition, body);
        }

        private ForStatement DoForStatement()
        {
            TakeKeyword("for");
            Take(LexemeKind.LeftParenthesis);
            Take(LexemeKind.Dollar, "variable indicator");

            string varName = Take(LexemeKind.String, "variable name", false).Content;

            Expression from = null;

            if (TakeKeyword("from", out _))
            {
                from = DoExpression();
            }

            TakeKeyword("to");
            var to = DoExpression();

            Take(LexemeKind.RightParenthesis);

            var body = DoStatementBlock();

            return new ForStatement(varName, from, to, body);
        }

        private IEnumerable<Statement> DoStatementBlock(bool forceBlock = false)
        {
            if (!forceBlock && !Take(LexemeKind.LeftBrace, out _))
            {
                return GetStatement().Yield();
            }

            var statements = new List<Statement>();

            if (forceBlock)
                Take(LexemeKind.LeftBrace, "block body opening");

            SkipWhitespaces();

            Statement statement;
            while ((statement = GetStatement()) != null)
            {
                if (!(statement is CommentStatement))
                    statements.Add(statement);
            }

            Take(LexemeKind.RightBrace, "block body closing");

            return statements;
        }

        private Parameter DoParameter()
        {
            string name = Take(LexemeKind.String, "parameter name").Content;
            Expression defaultValue = null;

            if (Take(LexemeKind.EqualsAssign, out _))
            {
                defaultValue = DoExpression();
            }

            return new Parameter(name, defaultValue);
        }

        private CommandCallExpression DoCommandExpression(string cmdName = null)
        {
            cmdName = cmdName ?? Take(LexemeKind.String).Content;
            bool parenthesised = Take(LexemeKind.LeftParenthesis, out _);
            var args = DoArguments();

            if (parenthesised)
                Take(LexemeKind.RightParenthesis, "parameter list closing");

            if (Current.Kind == LexemeKind.Semicolon)
                Advance();
            else if (Current.Kind == LexemeKind.Hashtag)
                AdvanceUntil(LexemeKind.NewLine);

            return new CommandCallExpression(cmdName, args.ToArray());
        }



        private IEnumerable<Expression> DoArguments()
        {
            var args = new List<Expression>();

            Expression expr;
            while (Current.Kind != LexemeKind.NewLine && (expr = DoExpression()) != null)
                args.Add(expr);

            return args;
        }

        private Expression DoExpression(bool doOperator = true)
        {
            Expression ret = null;

            if (Take(LexemeKind.LeftParenthesis, out _))
            {
                ret = DoExpression();

                if (Take(LexemeKind.QuestionMark, out _))
                    ret = DoTernaryOperator(ret);

                Take(LexemeKind.RightParenthesis, "closing parentheses");
            }
            else if (Take(LexemeKind.String, out var str))
            {
                if (float.TryParse(str.Content, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                {
                    ret = new NumberLiteralExpression(f);
                }
                else if (Current.Kind == LexemeKind.Exclamation)
                {
                    ret = DoFunctionCall(str.Content);
                }
                else
                {
                    if (Current.Kind == LexemeKind.LeftParenthesis)
                    {
                        ret = DoCommandExpression(str.Content);
                    }
                    else
                    {
                        Error($"unxpected string '{str.Content}' found");
                    }
                }
            }
            else if (Take(LexemeKind.QuotedString, out var quotedStr))
            {
                ret = new StringLiteralExpression(quotedStr.Content);
            }
            else if (Take(LexemeKind.Exclamation, out _))
            {
                ret = DoNegateOperator();
            }
            else if (Take(LexemeKind.Keyword, out var keyword))
            {
                if (keyword.Content == "null")
                    ret = new NullExpression();
                else if (keyword.Content == "true")
                    ret = new BooleanLiteralExpression(true);
                else if (keyword.Content == "false")
                    ret = new BooleanLiteralExpression(false);
                else
                    Error($"unexpected keyword: '{keyword.Content}'");
            }
            else if (Take(LexemeKind.Dollar, out _))
            {
                Push();

                if (Take(LexemeKind.String, out _) && Take(LexemeKind.EqualsAssign, out _))
                {
                    Pop();

                    ret = DoVariableAssign();
                }
                else
                {
                    Pop();

                    string name = Take(LexemeKind.String, "variable name").Content;

                    ret = new VariableAccessExpression(name);

                    if (Take(LexemeKind.Increment, out _))
                        ret = new UnaryOperatorExpression(Operator.IncrementByOne, ret);
                    else if (Take(LexemeKind.Decrement, out _))
                        ret = new UnaryOperatorExpression(Operator.DecrementByOne, ret);
                }
            }

            if (ret != null && doOperator)
            {
                if (Take(LexemeKind.QuestionMark, out _))
                {
                    return DoTernaryOperator(ret);
                }

                return DoOperatorChain(ret) ?? ret;
            }

            return ret;
        }

        private UnaryOperatorExpression DoNegateOperator()
        {
            var operand = DoExpression();

            return new UnaryOperatorExpression(Operator.Negate, operand);
        }

        private Expression DoOperatorChain(Expression first)
        {
            var items = new List<object>();
            Operator? op;

            do
            {
                op = null;

                foreach (var item in LexemeOperatorMap)
                {
                    if (Take(item.Key, out _))
                    {
                        op = item.Value;
                        break;
                    }
                }

                if (op != null)
                {
                    items.Add(op.Value);
                    items.Add(DoExpression(false));
                }

            } while (op != null);

            if (items.Count > 0)
            {
                items.Insert(0, first);

                for (int i = MaxOperatorValue; i >= 0; i--)
                {
                    for (int j = 0; j < items.Count; j++)
                    {
                        var item = items[j];

                        if (item is Operator o && o == (Operator)i)
                        {
                            items[j - 1] = new BinaryOperatorExpression(items[j - 1] as Expression, items[j + 1] as Expression, o);
                            items.RemoveAt(j);
                            items.RemoveAt(j);

                            j--;
                        }
                    }
                }

                if (items.Count > 1)
                    Error("invalid operator chain");

                return items[0] as Expression;
            }
            else
            {
                return null;
            }
        }

        private TernaryOperatorExpression DoTernaryOperator(Expression condition)
        {
            var ifTrue = DoExpression();
            Take(LexemeKind.Colon, "ternary operator separator");
            var ifFalse = DoExpression();

            return new TernaryOperatorExpression(condition, ifTrue, ifFalse);
        }

        private FunctionCallExpression DoFunctionCall(string funcName = null)
        {
            funcName = funcName ?? Take(LexemeKind.String, "function name", false).Content;
            Take(LexemeKind.Exclamation, "exclamation");

            Take(LexemeKind.LeftParenthesis, "parameter list opening");
            var args = DoArguments();
            Take(LexemeKind.RightParenthesis, "parameter list closing");

            return new FunctionCallExpression(funcName, args.ToArray());
        }

        private Expression DoVariableAssign()
        {
            string name = Take(LexemeKind.String, "variable name").Content;
            Operator? op = null;

            if (Take(LexemeKind.Plus, out _))
                op = Operator.Add;
            else if (Take(LexemeKind.Minus, out _))
                op = Operator.Subtract;
            else if (Take(LexemeKind.Multiply, out _))
                op = Operator.Multiply;
            else if (Take(LexemeKind.Divide, out _))
                op = Operator.Divide;
            else if (Take(LexemeKind.Increment, out _))
                op = Operator.IncrementByOne;
            else if (Take(LexemeKind.Decrement, out _))
                op = Operator.DecrementByOne;

            if (op == Operator.IncrementByOne || op == Operator.DecrementByOne)
            {
                return new UnaryOperatorExpression(op.Value, new VariableAccessExpression(name));
            }
            else
            {
                Take(LexemeKind.EqualsAssign);

                var value = DoExpression();

                return new VariableAssignmentExpression(name, value, op);
            }
        }
    }
}
