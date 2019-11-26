using System;
using System.Globalization;

namespace LICC
{
    public class DefaultValueConverter : IValueConverter
    {
        public (bool Success, object Value) TryConvertValue(Type targetType, string arg)
        {
            if (targetType == typeof(object))
                return (true, ParseObj());

            else if (targetType == typeof(string))
                return T(true, arg);

            else if (targetType == typeof(int))
                return T(int.TryParse(arg, out var i), i);

            else if (targetType == typeof(long))
                return T(long.TryParse(arg, out var i), i);

            else if (targetType == typeof(short))
                return T(short.TryParse(arg, out var i), i);

            else if (targetType == typeof(byte))
                return T(byte.TryParse(arg, out var i), i);

            else if (targetType == typeof(float))
                return T(float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var i), i);

            else if (targetType == typeof(double))
                return T(double.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var i), i);

            else if (targetType == typeof(bool))
                return T(bool.TryParse(arg, out var i), i);

            return (false, null);

            (bool, object) T(bool success, object val) => (success, val);

            object ParseObj()
            {
                if (int.TryParse(arg, out var i))
                    return i;
                else if (float.TryParse(arg, out var f))
                    return f;
                else if (bool.TryParse(arg, out var b))
                    return b;

                return arg;
            }
        }
    }
}
