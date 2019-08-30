using Sprache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal.Parsing
{
    public class Parameter : IEquatable<Parameter>
    {
        public string Type { get; }
        public string Name { get; }

        public Parameter(string type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Parameter);
        }

        public bool Equals(Parameter other)
        {
            return other != null &&
                   this.Type == other.Type &&
                   this.Name == other.Name;
        }

        public override int GetHashCode()
        {
            var hashCode = -1979447941;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Name);
            return hashCode;
        }

        public override string ToString() => $"{Type} {Name}";
    }

    public class Function : IEquatable<Function>
    {
        public string Name { get; }
        public IEnumerable<Parameter> Parameters { get; }
        public IEnumerable<IStatement> Statements { get; }

        public Function(string name, IEnumerable<Parameter> parameters, IEnumerable<IStatement> statements)
        {
            this.Name = name;
            this.Parameters = parameters;
            this.Statements = statements;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Function);
        }

        public bool Equals(Function other)
        {
            return other != null &&
                   this.Name == other.Name &&
                   this.Parameters.SequenceEqual(other.Parameters);
        }

        public override int GetHashCode()
        {
            var hashCode = 497090031;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<Parameter>>.Default.GetHashCode(this.Parameters);
            return hashCode;
        }
    }

    public interface IStatement
    {
    }

    public class CommandCall : IStatement
    {
        public string CommandName { get; }
        public IEnumerable<string> Arguments { get; }

        public CommandCall(string commandName, IEnumerable<string> arguments)
        {
            this.CommandName = commandName;
            this.Arguments = arguments;
        }
    }

    public class Comment : IStatement
    {
    }

    public static class Grammar
    {
        public static readonly Parser<string> Identifier =
            Parse.Identifier(Parse.Letter.Or(Parse.Char('_')), Parse.LetterOrDigit.Or(Parse.Char('_')));

        public static readonly Parser<string> QuotedString =
            from lQuote in Parse.Chars('"', '\'')
            from str in Parse.CharExcept(lQuote).Many().Text()
            from rQuote in Parse.Char(lQuote)
            select str;

        public static readonly Parser<string> String =
            (from lQuote in Parse.Chars('"', '\'')
             from str in Parse.CharExcept(lQuote).Many().Text()
             from rQuote in Parse.Char(lQuote)
             select str).XOr
            (
             from str in Parse.CharExcept(" {}").Except(LineEnd).Many().Text()
             select str
            );

        public static Parser<string> LineEnd => Parse.LineTerminator.Or(Parse.Char(';').Select(o => o.ToString()));

        public static readonly Parser<Parameter> Parameter =
            (from type in Identifier
             from space in Parse.WhiteSpace.Many()
             from name in Identifier
             select new Parameter(type, name)).Token();

        public static readonly Parser<IEnumerable<Parameter>> ParameterList =
            (from first in Parameter.Once()
             from others in (
                from comma in Parse.Char(',').Token()
                from param in Parameter.Token()
                select param
             ).Many()
             select first.Concat(others));

        public static readonly Parser<Function> Function =
            from functionKeyword in Parse.String("function")
            from space in Parse.WhiteSpace.Many()
            from name in Identifier.Token()
            from lParen in Parse.Char('(').Token()
            from parameters in ParameterList
            from rParen in Parse.Char(')').Token()
            from lBrace in Parse.Char('{').Token()
            from lines in Statement.Many()
            from rBrace in Parse.Char('}').Token()
            select new Function(name, parameters, lines);

        public static readonly Parser<IStatement> Statement =
            from statement in Comment.Or<IStatement>(CommandCall)
            select statement;

        public static Parser<Comment> Comment =>
            from space in Parse.WhiteSpace.Many()
            from startHash in Parse.Char('#')
            from str in Parse.AnyChar.Except(LineEnd).Many().Text()
            from end in LineEnd
            select new Comment();

        public static Parser<IEnumerable<string>> ArgumentList =>
            from startSpace in Parse.WhiteSpace.Many()
            from first in String.Once()
            from others in (
                from space in Parse.WhiteSpace.Many()
                from str in String
                select str
            ).Until(LineEnd)
            select first.Concat(others);

        public static Parser<CommandCall> CommandCall =>
            from startSpace in Parse.WhiteSpace.Many()
            from cmdName in Identifier
            from space in Parse.WhiteSpace.XAtLeastOnce()
            from args in ArgumentList.Optional()
            from end in Parse.Char(';').Optional()
            from newline in LineEnd.Optional()
            select new CommandCall(cmdName, args.GetOrElse(Enumerable.Empty<string>()));
    }
}
