using LICC.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LICC
{
    public sealed class CommandConsole
    {
        public CommandRegistry Commands { get; } = new CommandRegistry();

        private readonly IValueConverter ValueConverter;

        public CommandConsole(Frontend frontend, IValueConverter valueConverter)
        {
            LConsole.Frontend = frontend;

            this.ValueConverter = valueConverter;
        }

        public CommandConsole(Frontend frontend) : this(frontend, new DefaultValueConverter())
        {
        }
    }
}
