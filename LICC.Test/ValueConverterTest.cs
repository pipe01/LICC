using NUnit.Framework;
using System;

namespace LICC.Test
{
    public class ValueConverterTest
    {
        private static readonly object[] TestData =
        {
            new object[] { typeof(string), "hello", "hello", true },

            new object[] { typeof(int), "123", 123, true },
            new object[] { typeof(int), "asd", default, false },

            new object[] { typeof(long), "123", 123, true },
            new object[] { typeof(long), "asd", default, false },

            new object[] { typeof(short), "123", 123, true },
            new object[] { typeof(short), "asd", default, false },

            new object[] { typeof(byte), "123", 123, true },
            new object[] { typeof(byte), "asd", default, false },

            new object[] { typeof(float), "123.4", 123.4f, true },
            new object[] { typeof(float), "asd", default, false },

            new object[] { typeof(double), "123.4", 123.4d, true },
            new object[] { typeof(double), "asd", default, false },

            new object[] { typeof(bool), "true", true, true },
            new object[] { typeof(bool), "asd", default, false },
        };

        [TestCaseSource(nameof(TestData))]
        public void ConvertTest(Type type, string str, object expectedValue, bool mustSucceed)
        {
            var valueConverter = new DefaultValueConverter();

            var (success, obj) = valueConverter.TryConvertValue(type, str);

            Assert.AreEqual(success, mustSucceed);

            if (mustSucceed)
            {
                if (type == typeof(float))
                    Assert.AreEqual((float)expectedValue, (float)obj, 0.1);
                else if (type == typeof(double))
                    Assert.AreEqual((double)expectedValue, (double)obj, 0.1);
                else
                    Assert.AreEqual(obj, expectedValue);
            }
        }
    }
}
