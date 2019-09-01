namespace LICC.Internal.LSF.Parsing.Data
{
    internal class Parameter
    {
        public string Name { get; }
        public Expression DefaultValue { get; }

        public bool IsOptional => DefaultValue != null;

        public Parameter(string name, Expression defaultValue)
        {
            this.Name = name;
            this.DefaultValue = defaultValue;
        }
    }
}
