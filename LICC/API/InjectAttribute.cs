using System;

namespace LICC.API
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class InjectAttribute : Attribute
    {
    }
}
