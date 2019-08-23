using LICC.API;
using LICC.Exceptions;
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
        private readonly Shell Shell;

        public CommandConsole(Frontend frontend, IValueConverter valueConverter)
        {
            LConsole.Frontend = frontend;

            this.ValueConverter = valueConverter;
            this.Shell = new Shell(valueConverter, Commands);

            frontend.LineInput += Frontend_LineInput;
        }

        private void Frontend_LineInput(string line)
        {
            try
            {
                Shell.ExecuteLine(line);
            }
            catch (CommandNotFoundException ex)
            {
                LConsole.WriteLine(ex.Message, Color.Red);
            }
            catch (ParameterMismatchException ex)
            {
                LConsole.WriteLine(ex.Message, Color.Red);
            }
            catch (ParameterConversionException ex)
            {
                LConsole.WriteLine(ex.Message, Color.Red);
            }
        }

        public CommandConsole(Frontend frontend) : this(frontend, new DefaultValueConverter())
        {
        }
    }
}
