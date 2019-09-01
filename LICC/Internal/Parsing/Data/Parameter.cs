namespace LICC.Internal.Parsing.Data
{
    internal class Parameter
    {
        public string Type { get; }
        public string Name { get; }

        public Parameter(string type, string name)
        {
            this.Type = type;
            this.Name = name;
        }
    }
}
