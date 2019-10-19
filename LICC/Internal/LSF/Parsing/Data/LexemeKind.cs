using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace LICC.Internal.LSF.Parsing.Data
{
    internal enum LexemeKind
    {
        [Description(null)] String,
        [Description(null)] QuotedString,
        [Description(null)] Keyword,
        [Description(" ")]  Whitespace,
        [Description(",")]  Comma,
        [Description(".")]  Dot,

        [Description("$")]  Dollar,
        [Description("=")]  EqualsAssign,
        [Description("#")]  Hashtag,
        [Description("@")]  AtSign,
        [Description("!")]  Exclamation,
        [Description("?")]  QuestionMark,
        [Description(":")]  Colon,

        [Description("==")] Equals,
        [Description("<=")] LessOrEqual,
        [Description("<")]  Less,
        [Description(">=")] MoreOrEqual,
        [Description(">")]  More,
        [Description(">=")] NotEqual,

        [Description("(")]  LeftParenthesis,
        [Description(")")]  RightParenthesis,
        [Description("{")]  LeftBrace,
        [Description("}")]  RightBrace,

        [Description("+")]  Plus,
        [Description("-")]  Minus,
        [Description("*")]  Multiply,
        [Description("/")]  Divide,
        [Description("%")]  Percentage,
        [Description("++")] Increment,
        [Description("--")] Decrement,

        [Description("&")]  And,
        [Description("&&")] AndAlso,
        [Description("|")]  Or,
        [Description("||")] OrElse,

        [Description(null)] NewLine,
        [Description(";")]  Semicolon,
        [Description(null)] EndOfFile,
    }

    internal static class LexemeKindExtensions
    {
        private static readonly IDictionary<LexemeKind, string> CharCache = new Dictionary<LexemeKind, string>();

        public static string GetCharacter(this LexemeKind kind)
        {
            return CharCache.TryGetValue(kind, out var c)
                ? c
                : CharCache[kind] = typeof(LexemeKind).GetField(kind.ToString()).GetCustomAttribute<DescriptionAttribute>()?.Description;
        }
    }
}
