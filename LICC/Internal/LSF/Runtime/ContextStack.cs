using System.Collections.Generic;
using System.Linq;

namespace LICC.Internal.LSF.Runtime
{
    internal class ContextStack : Stack<RunContext>
    {
        public void Push() => Push(new RunContext());

        public bool TryGetValue(string name, out object value)
        {
            foreach (var item in this.Reverse())
            {
                if (item.Variables.TryGetValue(name, out value))
                    return true;
            }

            value = null;
            return false;
        }
    }
}
