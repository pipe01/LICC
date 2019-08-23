using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LICC
{
    public interface ICommandConsole
    {
        CommandRegistry Commands { get; }
    }

    public class CommandConsole : ICommandConsole
    {
        public CommandRegistry Commands { get; } = new CommandRegistry();
    }
}
