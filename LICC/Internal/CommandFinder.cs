using System.Linq;

namespace LICC.Internal
{
    internal interface ICommandFinder
    {
        (bool Found, Command Command, Command[] CommandsWithSameName) Find(string cmdName, int argsCount);
    }

    internal class CommandFinder : ICommandFinder
    {
        private readonly ICommandRegistryInternal CommandRegistry;

        public CommandFinder(ICommandRegistryInternal commandRegistry)
        {
            this.CommandRegistry = commandRegistry;
        }

        public (bool Found, Command Command, Command[] CommandsWithSameName) Find(string cmdName, int argsCount)
        {
            if (!CommandRegistry.AllRegisteredCommands.ContainsCommand(cmdName))
                return (false, default, null);

            var cmdsWithMatchingName = CommandRegistry.AllRegisteredCommands.GetEntry(cmdName).Commands
                .OrderBy(o => o.OptionalParamCount) //Commands with the fewest optional params will be chosen first
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
