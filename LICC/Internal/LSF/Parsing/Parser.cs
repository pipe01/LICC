using LICC.Internal.LSF.Parsing.Data;
using System;
using System.Collections.Generic;
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
        };
        private static readonly int MaxOperatorValue = ((Operator[])Enum.GetValues(typeof(Operator))).Max(o => (int)o);

        private int Index;
        private Lexeme Current => Lexemes[Index];
        private SourceLocation Location => Current.Begin;

        private Lexeme[] Lexemes;
        private Stack<int> IndexStack = new Stack<int>();

        #region Utils
        private void Advance()
        {
            Index++;

            if (Index >= Lexemes.Length)
                Index = Lexemes.Length - 1;
        }

        private void AdvanceUntil(LexemeKind kind)
        {
            while (Current.Kind != kind)
                Advance();
        }

        private void Back()
        {
            Index--;
        }

        private void Push() => IndexStack.Push(Index);

        private void Pop(bool set = true)
        {
            int i = IndexStack.Pop();
            if (set)
                Index = i;
        }

        private void SkipWhitespaces(bool alsoNewlines = true)
        {
            while (Current.Kind == LexemeKind.Whitespace || (Current.Kind == LexemeKind.NewLine && alsoNewlines))
                Advance();
        }

        private Lexeme TakeAny()
        {
            var token = Current;
            Advance();
            return token;
        }

        private Lexeme Take(LexemeKind lexemeKind, string expected = null, bool ignoreWhitespace = true)
        {
            if (ignoreWhitespace)
                SkipWhitespaces();

            if (Current.Kind != lexemeKind)
                Error($"expected {expected ?? lexemeKind.ToString()}, found '{Current.Content}' ({Current.Kind})");

            return TakeAny();
        }

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

        private Lexeme TakeKeyword(string keyword, bool ignoreWhitespace = true, bool @throw = true, string msg = null)
        {
            if (ignoreWhitespace)
                SkipWhitespaces();

            if (Current.Kind != LexemeKind.Keyword || Current.Content != keyword)
            {
                if (@throw)
                    Error(msg ?? $"expected '{keyword}' keyword, found {Current}");
                else
                    return null;
            }

            return TakeAny();
        }

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
                case LexemeKind.Hashtag:
                    AdvanceUntil(LexemeKind.NewLine);
                    ret = new CommentStatement();
                    break;

                case LexemeKind.Keyword when Current.Content == "return":
                    Advance();
                    ret = new ReturnStatement(DoExpression());
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

                    ret = DoCommand();
                    break;

                case LexemeKind.Exclamation:
                    Advance();
                    ret = new ExpressionStatement(DoFunctionCall());
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

            Take(LexemeKind.LeftParenthesis, "parameter list opening '('");

            while (true)
            {
                SkipWhitespaces();
                if (Current.Kind != LexemeKind.String)
                    break;

                parameters.Add(DoParameter());

                SkipWhitespaces();
                if (Current.Kind != LexemeKind.Comma)
                    break;
                else
                    Advance();
            }

            Take(LexemeKind.RightParenthesis, "parameter list closing ')'");
            SkipWhitespaces();

            var statements = DoStatementBlock();

            return new FunctionDeclarationStatement(name, statements, parameters);
        }

        private IfStatement DoIfStatement()
        {
            TakeKeyword("if");
            Take(LexemeKind.LeftParenthesis, "condition opening '('");

            var condition = DoExpression();

            Take(LexemeKind.RightParenthesis, "condition closing ')'");

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
            Take(LexemeKind.LeftParenthesis, "condition opening '('");

            var condition = DoExpression();

            Take(LexemeKind.RightParenthesis, "condition opening ')'");

            var body = DoStatementBlock();

            return new WhileStatement(condition, body);
        }

        private ForStatement DoForStatement()
        {
            TakeKeyword("for");
            Take(LexemeKind.LeftParenthesis);
            Take(LexemeKind.Dollar, "variable indicator '$'");

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

        private IEnumerable<Statement> DoStatementBlock()
        {
            var statements = new List<Statement>();

            Take(LexemeKind.LeftBrace, "block body opening '{'");
            SkipWhitespaces();

            Statement statement;
            while ((statement = GetStatement()) != null)
            {
                if (!(statement is CommentStatement))
                    statements.Add(statement);
            }

            Take(LexemeKind.RightBrace, "block body closing '}'");

            return statements;
        }

        private Parameter DoParameter()
        {
            string name = Take(LexemeKind.String, "parameter name").Content;

            return new Parameter(name);
        }

        private CommandStatement DoCommand()
        {
            string cmdName = Take(LexemeKind.String).Content;
            var args = DoArguments();

            if (Current.Kind == LexemeKind.Semicolon)
                Advance();
            else if (Current.Kind == LexemeKind.Hashtag)
                AdvanceUntil(LexemeKind.NewLine);

            return new CommandStatement(cmdName, args.ToArray());
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

                Take(LexemeKind.RightParenthesis, "closing parentheses ')'");
            }
            else if (Take(LexemeKind.String, out var str))
            {
                if (float.TryParse(str.Content, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                    ret = new NumberLiteralExpression(f);
                else
                    Error($"invalid string '{str.Content}'");
            }
            else if (Take(LexemeKind.QuotedString, out var quotedStr))
            {
                ret = new StringLiteralExpression(quotedStr.Content);
            }
            else if (Take(LexemeKind.Exclamation, out var excl))
            {
                Push();

                if (!Take(LexemeKind.String, out _))
                {
                    Pop();

                    ret = DoUnaryOperator(excl);
                }
                else
                {
                    Pop();

                    ret = DoFunctionCall();
                }
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
                }
            }

            if (ret != null && doOperator)
            {
                return DoOperatorChain(ret) ?? ret;
            }

            return ret;
        }

        private UnaryOperatorExpression DoUnaryOperator(Lexeme op)
        {
            var operand = DoExpression();

            if (op.Kind == LexemeKind.Exclamation)
                return new UnaryOperatorExpression(Operator.Negate, operand);

            Error("invalid unary operator");
            return null;
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
                        op = item.Value;
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

        private FunctionCallExpression DoFunctionCall()
        {
            string funcName = Take(LexemeKind.String, "function name", false).Content;
            var args = DoArguments();

            return new FunctionCallExpression(funcName, args.ToArray());
        }

        private VariableAssignmentExpression DoVariableAssign()
        {
            string name = Take(LexemeKind.String, "variable name").Content;
            Take(LexemeKind.EqualsAssign);
            var value = DoExpression();

            return new VariableAssignmentExpression(name, value);
        }
    }
}
