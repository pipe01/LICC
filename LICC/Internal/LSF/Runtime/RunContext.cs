using LICC.Internal.LSF.Runtime.Data;
using System.Collections.Generic;

namespace LICC.Internal.LSF.Runtime
{
    internal class RunContext
    {
        public IDictionary<string, object> Variables = new Dictionary<string, object>();
        public IDictionary<string, Function> Functions = new Dictionary<string, Function>();
    }
}
