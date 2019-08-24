using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LICC.Internal
{
    internal interface IEnvironment
    {
        void Set(string name, string value);
        void Remove(string name);
        (bool Exists, string Value) TryGet(string name);
        IReadOnlyDictionary<string, string> GetAll();
    }

    internal class Environment : IEnvironment
    {
        private readonly IDictionary<string, string> Variables = new Dictionary<string, string>();

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
    }
}
