using LICC.API;
using LICC.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC
{
    internal class Shell
    {
        private readonly IValueConverter ValueConverter;
        private readonly IWriteableHistory History;
        private readonly CommandRegistry CommandRegistry;
        private readonly ConsoleConfiguration Config;

        public Shell(IValueConverter valueConverter, IWriteableHistory history, CommandRegistry commandRegistry, ConsoleConfiguration config = null)
        {
            this.ValueConverter = valueConverter;
            this.History = history;
            this.CommandRegistry = commandRegistry;
            this.Config = config ?? new ConsoleConfiguration();
        }

        public void ExecuteLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            line = line.Trim();

            History.AddNewItem(line);

            int cmdNameSeparatorIndex = line.IndexOf(' ');
            string cmdName = cmdNameSeparatorIndex == -1 ? line.Substring(0) : line.Substring(0, cmdNameSeparatorIndex);

            if (!CommandRegistry.TryGetCommand(cmdName, out var cmd, !Config.CaseSensitiveCommandNames))
                throw new CommandNotFoundException(cmdName);

            int nonOptionalParamCount = cmd.Params.Count(o => !o.Optional);

            object[] cmdArgs = Enumerable.Repeat(Type.Missing, cmd.Params.Length).ToArray();

            if (cmdNameSeparatorIndex != -1)
            {
                string argsLine = line.Substring(cmdNameSeparatorIndex + 1);
                var strArgs = GetArgs(argsLine).ToArray();

                if (strArgs.Length < nonOptionalParamCount || strArgs.Length > cmd.Params.Length)
                    throw new ParameterMismatchException(nonOptionalParamCount, cmd.Params.Length, strArgs.Length, cmd);

                for (int i = 0; i < strArgs.Length; i++)
                {
                    var (success, value) = ValueConverter.TryConvertValue(cmd.Params[i].Type, strArgs[i]);

                    if (!success)
                        throw new ParameterConversionException(cmd.Params[i].Name, cmd.Params[i].Type);
                    else
                        cmdArgs[i] = value;
                }
            }
            else if (nonOptionalParamCount > 0)
            {
                throw new ParameterMismatchException(nonOptionalParamCount, cmd.Params.Length, 0, cmd);
            }

            cmd.Method.Invoke(null, cmdArgs);
        }

        private IEnumerable<string> GetArgs(string str)
        {
            int i = 0;

            while (i < str.Length)
            {
                char c = Take();

                if (c == '"')
                {
                    yield return TakeDelimitedString('"');
                }
                else if (c == '\'')
                {
                    yield return TakeDelimitedString('\'');
                }
                else if (c != ' ')
                {
                    i--;
                    yield return TakeDelimitedString(' ', true);
                }
            }

            char Take() => str[i++];

            string TakeDelimitedString(char delimiter, bool allowEndOfString = false)
            {
                string buffer = "";
                bool foundDelimiter = false;

                while (i < str.Length)
                {
                    char c = Take();

                    if (c == '\\')
                    {
                        char escaped = Take();
                        buffer += escaped;
                    }
                    else if (c == delimiter)
                    {
                        foundDelimiter = true;
                        break;
                    }
                    else
                    {
                        buffer += c;
                    }
                }

                if (!foundDelimiter && !allowEndOfString)
                    throw new ParserException("Missing closing delimiter at end of line");

                return buffer;
            }
        }
    }
}
