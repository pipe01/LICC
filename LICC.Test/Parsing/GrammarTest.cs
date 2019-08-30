using LICC.Internal.Parsing;
using NUnit.Framework;
using Sprache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            new[] { "String asd", "String", "asd" },
            new[] { "Int32 _asd", "Int32", "_asd" },
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

        //public static string[] InvalidIdentifiers =
        //{
        //    "1test",
        //    "@asdads",
        //    "dsad@d",
        //    "asd\"asd"
        //};

        //[TestCaseSource(nameof(ValidIdentifiers))]
        //public void InvalidIdentifier(string str)
        //{
        //    Assert.Throws<ParseException>(() =>
        //    {
        //        var a = Grammar.Identifier.Parse(str);
        //    });
        //}
    }
}
