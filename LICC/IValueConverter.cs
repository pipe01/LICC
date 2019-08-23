using System;

namespace LICC
{
    public interface IValueConverter
    {
        (bool Success, object Value) TryConvertValue(Type targetType, string arg);
    }
}
