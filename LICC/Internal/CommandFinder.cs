using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal
{
    internal interface ICommandFinder
    {
        (bool Found, Command Command, Command[] ClosestCommands) Find(string cmdName, int argsCount);
    }

    internal class CommandFinder : ICommandFinder
    {
        private readonly ICommandRegistryInternal CommandRegistry;
        private readonly ConsoleConfiguration Config;

        public CommandFinder(ICommandRegistryInternal commandRegistry, ConsoleConfiguration config)
        {
            this.CommandRegistry = commandRegistry;
            this.Config = config;
        }

        public (bool Found, Command Command, Command[] ClosestCommands) Find(string cmdName, int argsCount)
        {
            var allCmds = CommandRegistry.GetCommands();
            var cmdsWithMatchingName = allCmds
                .Where(o => o.Name.Equals(cmdName, Config.CaseSensitiveCommandNames ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                .OrderBy(o => o.OptionalParamCount) //Commands with the least optional params will be chosen first
                .ToArray();

            if (cmdsWithMatchingName.Length == 0)
                return (false, default, null);

            foreach (var cmd in cmdsWithMatchingName)
            {
                if (argsCount >= cmd.RequiredParamCount && argsCount <= cmd.Params.Length)
                    return (true, cmd, cmdsWithMatchingName);
            }

            return (false, default, cmdsWithMatchingName);
        }
    }
}
