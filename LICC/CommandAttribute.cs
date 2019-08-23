using System;

namespace LICC
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }

        public string Description { get; set; }

        public CommandAttribute()
        {
        }

        public CommandAttribute(string name)
        {
            this.Name = name;
        }
    }
}
