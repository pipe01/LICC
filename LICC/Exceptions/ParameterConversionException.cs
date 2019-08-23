using System;

namespace LICC.Exceptions
{
    internal sealed class ParameterConversionException : Exception
    {
        public string ParamName { get; }
        public Type ParamType { get; }

        public override string Message => $"Cannot convert argument '{ParamName}' into a {ParamType.Name}";

        public ParameterConversionException(string paramName, Type paramType)
        {
            this.ParamName = paramName;
            this.ParamType = paramType;
        }
    }
}
