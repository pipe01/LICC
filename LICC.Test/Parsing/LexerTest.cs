using LICC.Internal.LSF;
using LICC.Internal.LSF.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Test.Parsing
{
    public class LexerTest
    {
        private void Check(string source, (LexemeKind Kind, string Content)[] expected, bool appendEof = true)
        {
            var lexemes = Lexer.Lex(source).ToArray();

            if (appendEof && expected[expected.Length - 1].Kind != LexemeKind.EndOfFile)
                expected = expected.Append((LexemeKind.EndOfFile, null)).ToArray();

            Assert.AreEqual(expected.Length, lexemes.Length, "Lexeme count mistmatch");

            for (int i = 0; i < lexemes.Length; i++)
            {
                Assert.AreEqual(expected[i].Kind, lexemes[i].Kind);

                if (expected[i].Content != null)
                    Assert.AreEqual(expected[i].Content, lexemes[i].Content);
            }
        }

        [Test]
        public void SimpleString()
        {
            Check("hello", new[] {
                (LexemeKind.String, "hello")
            });
        }

        [TestCase("'hello'", "hello")]
        [TestCase("\"hello\"", "hello")]
        [TestCase("'he\"llo'", "he\"llo")]
        [TestCase("\"he'llo\"", "he'llo")]
        [TestCase("'he llo'", "he llo")]
        [TestCase("\"he llo\"", "he llo")]
        public void SimpleQuotedStringValid(string str, string expected)
        {
            Check(str, new[] {
                (LexemeKind.QuotedString, expected)
            });
        }

        [TestCase("'hello")]
        [TestCase("\"hello")]
        [TestCase("'hel\"lo")]
        [TestCase("\"hel'lo")]
        public void SimpleQuotedStringMissingEndInvalid(string str)
        {
            Assert.Throws<ParseException>(() => Check(str, null));
        }

        [Test]
        public void MultipleUnquotedStringsWithSpaces()
        {
            Check("hello this is me", new[]
            {
                (LexemeKind.String, "hello"),
                (LexemeKind.Whitespace, " "),
                (LexemeKind.String, "this"),
                (LexemeKind.Whitespace, " "),
                (LexemeKind.String, "is"),
                (LexemeKind.Whitespace, " "),
                (LexemeKind.String, "me"),
            });
        }

        [Test]
        public void MultipleQuotedStringsWithSpaces()
        {
            Check("'hel\"lo' \"th'is\" 'i\"s' \"m'e\"", new[]
            {
                (LexemeKind.QuotedString, "hel\"lo"),
                (LexemeKind.Whitespace, " "),
                (LexemeKind.QuotedString, "th'is"),
                (LexemeKind.Whitespace, " "),
                (LexemeKind.QuotedString, "i\"s"),
                (LexemeKind.Whitespace, " "),
                (LexemeKind.QuotedString, "m'e"),
            });
        }

        [Test]
        public void MultipleMixedStringsWithSpaces()
        {
            Check("'hel\"lo' th'is 'i\"s' m'e", new[]
            {
                (LexemeKind.QuotedString, "hel\"lo"),
                (LexemeKind.Whitespace, " "),
                (LexemeKind.String, "th'is"),
                (LexemeKind.Whitespace, " "),
                (LexemeKind.QuotedString, "i\"s"),
                (LexemeKind.Whitespace, " "),
                (LexemeKind.String, "m'e"),
            });
        }
    }
}
