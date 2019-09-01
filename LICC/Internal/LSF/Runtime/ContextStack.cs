using LICC.Internal.LSF.Runtime.Data;
using System.Collections.Generic;
using System.Linq;

namespace LICC.Internal.LSF.Runtime
{
    internal class ContextStack : Stack<IRunContext>
    {
        public void Push() => Push(new RunContext());

        public bool TryGetVariable(string name, out object value)
        {
            foreach (var item in this.Reverse())
            {
                if (item.TryGetVariable(name, out value))
                    return true;
            }

            value = null;
            return false;
        }

        public void SetVariable(string name, object value)
        {
            IRunContext contextWithVariable = null;

            foreach (var item in this.Reverse())
            {
                if (item.HasVariable(name))
                {
                    contextWithVariable = item;
                    break;
                }
            }

            (contextWithVariable ?? this.Last()).SetVariable(name, value);
        }

        public bool TryGetFunction(string name, out Function func)
        {
            foreach (var item in this.Reverse())
            {
                if (item.TryGetFunction(name, out func))
                    return true;
            }

            func = null;
            return false;
        }
    }
}
