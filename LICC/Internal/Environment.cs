using LICC.Internal.LSF.Runtime;
using LICC.Internal.LSF.Runtime.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LICC.Internal
{
    internal interface IEnvironment : IRunContext
    {
        void Set(string name, string value);
        void Remove(string name);
        (bool Exists, string Value) TryGet(string name);
        IReadOnlyDictionary<string, string> GetAll();
    }

    internal class Environment : IEnvironment
    {
        private readonly IDictionary<string, string> Variables = new Dictionary<string, string>();
        private readonly IDictionary<string, Function> Functions = new Dictionary<string, Function>();

        public RunContextType Type => RunContextType.Document;
        public string Descriptor => null;


        public IReadOnlyDictionary<string, string> GetAll()
            => new ReadOnlyDictionary<string, string>(Variables);

        public void Remove(string name)
        {
            if (Variables.ContainsKey(name))
                Variables.Remove(name);
        }

        public void Set(string name, string value) => Variables[name] = value;

        public (bool Exists, string Value) TryGet(string name)
        {
            if (Variables.TryGetValue(name, out var val))
                return (true, val);

            return (false, null);
        }

        bool IRunContext.TryGetFunction(string name, out Function func) => Functions.TryGetValue(name, out func);

        void IRunContext.SetFunction(string name, Function func) => Functions[name] = func;

        bool IRunContext.HasVariable(string name) => Variables.ContainsKey(name);

        void IRunContext.SetVariable(string name, object value) => Variables[name] = value?.ToString();

        bool IRunContext.TryGetVariable(string name, out object value)
        {
            if (Variables.TryGetValue(name, out var str))
            {
                value = str;
                return true;
            }

            value = null;
            return false;
        }
    }
}
