using LICC.Internal.LSF.Runtime.Data;
using System.Collections.Generic;

namespace LICC.Internal.LSF.Runtime
{
    internal enum RunContextType
    {
        Document,
        Function,
        Block
    }

    internal interface IRunContext
    {
        RunContextType Type { get; }
        string Descriptor { get; }

        bool TryGetVariable(string name, out object value);
        bool HasVariable(string name);
        void SetVariable(string name, object value);

        bool TryGetFunction(string name, out Function func);
        void SetFunction(string name, Function func);
    }

    internal class RunContext : IRunContext
    {
        private readonly IDictionary<string, object> Variables = new Dictionary<string, object>();
        private readonly IDictionary<string, Function> Functions = new Dictionary<string, Function>();

        public RunContextType Type { get; }
        public string Descriptor { get; }

        public RunContext(RunContextType type, string descriptor)
        {
            this.Type = type;
            this.Descriptor = descriptor;
        }

        public bool HasVariable(string name) => Variables.ContainsKey(name);

        public void SetVariable(string name, object value) => Variables[name] = value;

        public bool TryGetVariable(string name, out object value) => Variables.TryGetValue(name, out value);

        public bool TryGetFunction(string name, out Function func) => Functions.TryGetValue(name, out func);

        public void SetFunction(string name, Function func) => Functions[name] = func;
    }
}
