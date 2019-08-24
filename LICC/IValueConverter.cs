using System;

namespace LICC
{
    /// <summary>
    /// Interface for a class that can conver a value from its string representation to an object.
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// Tries to convert the value of type <paramref name="targetType"/> represented in the string <paramref name="arg"/>
        /// to an object.
        /// </summary>
        /// <param name="targetType">The type of the value.</param>
        /// <param name="arg">The string representation of the value.</param>
        (bool Success, object Value) TryConvertValue(Type targetType, string arg);
    }
}
