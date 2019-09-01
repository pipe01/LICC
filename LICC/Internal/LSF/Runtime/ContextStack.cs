using LICC.Internal.LSF.Runtime.Data;
using System.Collections.Generic;
using System.Linq;

namespace LICC.Internal.LSF.Runtime
{
    internal class ContextStack : Stack<RunContext>
    {
        public void Push() => Push(new RunContext());

        public bool TryGetVariable(string name, out object value)
        {
            foreach (var item in this.Reverse())
            {
                if (item.Variables.TryGetValue(name, out value))
                    return true;
            }

            value = null;
            return false;
        }

        public void SetVariable(string name, object value)
        {
            foreach (var item in this.Reverse())
            {
                if (item.Variables.ContainsKey(name))
                {
                    item.Variables[name] = value;
                    return;
                }
            }

            this.Last().Variables[name] = value;
        }

        public bool TryGetFunction(string name, out Function func)
        {
            foreach (var item in this.Reverse())
            {
                if (item.Functions.TryGetValue(name, out func))
                    return true;
            }

            func = null;
            return false;
        }
    }
}
