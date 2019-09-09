using LICC.API;
using LICC.Exceptions;
using LICC.Internal.LSF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal
{
    internal interface IShell
    {
        IEnvironment Environment { get; }
        Exception LastException { get; }

        void ExecuteLsf(string path);
        void ExecuteLine(string line, bool addToHistory = true);
    }

    internal class Shell : IShell
    {
        IEnvironment IShell.Environment => _Environment;

        private readonly IValueConverter ValueConverter;
        private readonly IWriteableHistory History;
        private readonly IFileSystem FileSystem;
        private readonly ICommandRegistryInternal CommandRegistry;
        private readonly IEnvironment _Environment;
        private readonly ILsfRunner LsfRunner;
        private readonly ConsoleConfiguration Config;

        private Exception _LastException;
        Exception IShell.LastException => _LastException;

        public Shell(IValueConverter valueConverter, IWriteableHistory history, IFileSystem fileSystem,
            ICommandRegistryInternal commandRegistry, IEnvironment environment, ILsfRunner lsfRunner = null,
            ConsoleConfiguration config = null)
        {
            this.ValueConverter = valueConverter;
            this.History = history;
            this.FileSystem = fileSystem;
            this.CommandRegistry = commandRegistry;
            this._Environment = environment;
            this.LsfRunner = lsfRunner ?? new LsfRunner(environment, commandRegistry, fileSystem);
            this.Config = config ?? new ConsoleConfiguration();
        }

        public void ExecuteLsf(string path)
        {
            if (FileSystem == null)
                throw new Exception("File system not defined");

            if (Path.GetExtension(path) == "")
                path += ".lsf";

            if (!FileSystem.FileExists(path))
                throw new FileNotFoundException(path);

            LsfRunner.Run(path);
        }

        ///<summary>Executes a single line</summary>
        /// <exception cref="CommandNotFoundException"></exception>
        /// <exception cref="ParameterMismatchException"></exception>
        /// <exception cref="ParameterConversionException"></exception>
        /// <exception cref="ParserException"></exception>
        public void ExecuteLine(string line, bool addToHistory = true)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            line = line.Trim();

            if (addToHistory)
                History.AddNewItem(line);

            int cmdNameSeparatorIndex = line.IndexOf(' ');
            string cmdName = cmdNameSeparatorIndex == -1 ? line.Substring(0) : line.Substring(0, cmdNameSeparatorIndex).Trim();
            string argsLine = cmdNameSeparatorIndex == -1 ? null : line.Substring(cmdNameSeparatorIndex + 1).Trim();

            argsLine = ReplaceVariables(argsLine);

            if (cmdName.StartsWith("$"))
            {
                HandleVariable(cmdName.Substring(1), argsLine);
                return;
            }

            var strArgs = GetArgs(argsLine).ToArray();

            if (!CommandRegistry.TryGetCommand(cmdName, strArgs.Length, out var cmd, !Config.CaseSensitiveCommandNames))
                throw new CommandNotFoundException(cmdName);

            int requiredParamCount = cmd.Params.Count(o => !o.Optional);

            object[] cmdArgs = Enumerable.Repeat(Type.Missing, cmd.Params.Length).ToArray();

            if (cmdNameSeparatorIndex != -1)
            {
                if (cmd.Params.Length == 1 && cmd.Params[0].Type == typeof(string))
                {
                    cmdArgs[0] = argsLine;
                }
                else
                {
                    if (strArgs.Length < requiredParamCount || strArgs.Length > cmd.Params.Length)
                        throw new ParameterMismatchException(requiredParamCount, cmd.Params.Length, strArgs.Length, cmd);

                    for (int i = 0; i < strArgs.Length; i++)
                    {
                        var (success, value) = ValueConverter.TryConvertValue(cmd.Params[i].Type, strArgs[i]);

                        if (!success)
                            throw new ParameterConversionException(cmd.Params[i].Name, cmd.Params[i].Type);
                        else
                            cmdArgs[i] = value;
                    }
                }
            }
            else if (requiredParamCount > 0)
            {
                throw new ParameterMismatchException(requiredParamCount, cmd.Params.Length, 0, cmd);
            }

            try
            {
                cmd.Method.Invoke(null, cmdArgs);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException.GetType().Name == "SuccessException") //For NUnit
                    throw ex.InnerException;

                HandleException(ex.InnerException);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            void HandleException(Exception ex)
            {
                _LastException = ex;

                LConsole.Frontend.PrintException(ex);
            }
        }

        private string ReplaceVariables(string str)
        {
            if (!Config.EnableVariables || string.IsNullOrEmpty(str))
                return str;

            var line = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\\' && i < str.Length - 1)
                {
                    var next = str[++i];
                    line.Append(next == '$' ? "$" : "\\" + next);
                }
                else if (c == '$')
                {
                    string var = new string(str.Skip(i + 1).TakeWhile(char.IsLetterOrDigit).ToArray());
                    var (exists, value) = _Environment.TryGet(var);

                    line.Append(exists ? value : "$" + var);

                    i += var.Length;
                }
                else
                {
                    line.Append(c);
                }
            }

            return line.ToString();
        }

        private void HandleVariable(string varName, string args)
        {
            if (!Config.EnableVariables)
            {
                LConsole.WriteLine("Variables are disabled", ConsoleColor.Red);
                return;
            }

            var (exists, value) = _Environment.TryGet(varName);

            if (string.IsNullOrWhiteSpace(args))
            {
                if (!exists)
                {
                    LConsole.WriteLine($"No variable found with name '{varName}'", ConsoleColor.Red);
                    return;
                }
            }
            else if (args.StartsWith(":="))
            {
                if (!HandleVariableOperation(varName, args.Substring(2).Trim()))
                    return;
            }
            else if (args.StartsWith("="))
            {
                value = args.Substring(1).Trim();

                if (string.IsNullOrWhiteSpace(value))
                {
                    _Environment.Remove(varName);
                    LConsole.WriteLine("Cleared variable");
                    return;
                }
                else
                {
                    _Environment.Set(varName, value);
                }
            }

            LConsole.BeginLine()
                .Write(varName, ConsoleColor.DarkYellow)
                .Write(" = ", ConsoleColor.DarkGray)
                .Write(value)
                .End();
        }

        private bool HandleVariableOperation(string assignTo, string argsStr)
        {
            return true;
            //var args = GetArgs(argsStr).ToArray();

            //if (args.Length != 3)
            //{
            //    LConsole.WriteLine("Invalid operation arguments", ConsoleColor.Red);
            //    return false;
            //}

            //string op = args[1];
        }

        private IEnumerable<string> GetArgs(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                yield break;

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
                else if (c == '#')
                {
                    break;
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
                    throw new ParserException("missing closing delimiter at end of line");

                return buffer;
            }
        }
    }
}
