using Sprache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal.Parsing
{
    internal class Parameter : IEquatable<Parameter>
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

    internal class Function
    {
        public string Name { get; }
        public IEnumerable<Parameter> Parameters { get; }

        public Function(string name, IEnumerable<Parameter> parameters)
        {
            this.Name = name;
            this.Parameters = parameters;
        }
    }

    internal static class Grammar
    {
        public static readonly Parser<string> Identifier =
            Parse.Identifier(Parse.Letter.Or(Parse.Char('_')), Parse.LetterOrDigit.Or(Parse.Char('_')));

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
            (from functionKeyword in Parse.String("function")
             from space in Parse.WhiteSpace
             from name in Identifier.Token()
             from lParen in Parse.Char('(')
             from parameters in ParameterList
             from rParen in Parse.Char(')')
             select new Function(name, parameters));
    }
}
