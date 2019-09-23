using System;

namespace LICC.API
{
    public interface IObjectProvider
    {
        object Get(Type type);
    }
}
