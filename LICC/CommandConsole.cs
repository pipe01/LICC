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

        private readonly Shell Shell;

        public CommandConsole(Frontend frontend, IValueConverter valueConverter)
        {
            LConsole.Frontend = frontend;

            var history = new History();
            frontend.History = history;

            this.Shell = new Shell(valueConverter, history, Commands);

            frontend.LineInput += Frontend_LineInput;
        }

        public CommandConsole(Frontend frontend) : this(frontend, new DefaultValueConverter())
        {
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
    }
}
