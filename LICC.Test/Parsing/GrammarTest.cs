using LICC.Internal.Parsing;
using NUnit.Framework;
using Sprache;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LICC.Test.Parsing
{
    public class GrammarTest
    {
        public static string[] ValidIdentifiers =
        {
            "test",
            "asd123",
            "asd_123",
            "_123"
        };

        [TestCaseSource(nameof(ValidIdentifiers))]
        public void ValidIdentifier(string str)
        {
            var identifier = Grammar.Identifier.Parse(str);
            Assert.AreEqual(str, identifier);
        }


        [TestCase("123")]
        [TestCase("123asd")]
        public void InvalidIdentifier(string str)
        {
            Assert.Throws<ParseException>(() => Grammar.Identifier.Parse(str));
        }


        public static string[][] ValidParameters =
        {
            new[] { "Int32 test", "Int32", "test" },
            new[] { "  String  asd ", "String", "asd" },
            new[] { "  Int32   _asd    ", "Int32", "_asd" },
            new[] { "Type a32d", "Type", "a32d" }
        };

        [TestCaseSource(nameof(ValidParameters))]
        public void ValidParameter(string str, string expectedType, string expectedName)
        {
            var param = Grammar.Parameter.Parse(str);

            Assert.AreEqual(expectedType, param.Type);
            Assert.AreEqual(expectedName, param.Name);
        }


        public static string[] InvalidParameters =
        {
            "1Test asd",
            "Typeasd",
            "Type 1asd"
        };

        [TestCaseSource(nameof(InvalidParameters))]
        public void InvalidParameter(string str)
        {
            Assert.Throws<ParseException>(() => Grammar.Parameter.Parse(str));
        }


        public static object[] ValidParameterLists =
        {
            new object[]{ "Type1 name1, Type2 name2", new[] { new Parameter("Type1", "name1"), new Parameter("Type2", "name2") } },
            new object[]{ "  Type1  name1   ,  Type2   name2   ", new[] { new Parameter("Type1", "name1"), new Parameter("Type2", "name2") } },
            new object[]{ "Type1 name1", new[] { new Parameter("Type1", "name1") } },
            new object[]{ "   Type1    name1   ", new[] { new Parameter("Type1", "name1") } },
        };

        [TestCaseSource(nameof(ValidParameterLists))]
        public void ValidParameterList(string str, IEnumerable<object> expected)
        {
            var @params = Grammar.ParameterList.Parse(str).ToArray();

            CollectionAssert.AreEqual(expected, @params);
        }


        public static object[] ValidFunctions =
        {
            new object[] { "function test(Int32 param1)", "test", new[] { new Parameter("Int32", "param1") } },
            new object[] { "function test123(Int32 param1, String param2)", "test123", new[] { new Parameter("Int32", "param1"), new Parameter("String", "param2") } },
            new object[] { "   function    test(   Int32    param1   ,    String    param2   )   ", "test", new[] { new Parameter("Int32", "param1"), new Parameter("String", "param2") } },
        };

        [TestCaseSource(nameof(ValidFunctions))]
        public void ValidFunction(string str, string expectedName, object[] expectedParameters)
        {
            var f = Grammar.Function.Parse(str);

            Assert.AreEqual(expectedName, f.Name);
            CollectionAssert.AreEqual(expectedParameters, f.Parameters);
        }


        public static object[] ValidCommandCalls =
        {
            new object[] { "cmd hello", "cmd", new[] { "hello" } },
            new object[] { "cmd 'hello'", "cmd", new[] { "hello" } },
            new object[] { "cmd \"hello\"", "cmd", new[] { "hello" } },
            new object[] { "123cmd", "123cmd", new string[] { } },
        };

        [TestCaseSource(nameof(ValidCommandCalls))]
        public void ValidCommandCall(string str, string expectedCmdName, IEnumerable<string> expectedArgs)
        {
            var c = Grammar.CommandCall.Parse(str);

            Assert.AreEqual(expectedCmdName, c.CommandName);
            CollectionAssert.AreEqual(expectedArgs, c.Arguments);
        }
    }
}
