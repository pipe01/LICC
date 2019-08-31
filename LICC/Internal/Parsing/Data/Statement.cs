using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICC.Internal.Parsing.Data
{
    internal interface IStatement
    {
    }

    internal class CommentStatement : IStatement
    {
    }

    internal class FunctionDeclarationStatement : IStatement
    {
        public string Name { get; }
        public IEnumerable<IStatement> Statements { get; }

        public FunctionDeclarationStatement(string name, IEnumerable<IStatement> statements)
        {
            this.Name = name;
            this.Statements = statements;
        }
    }
    
    internal class FunctionCallStatement : IStatement
    {
        public string Name { get; }
        public IEnumerable<string> Arguments { get; }

        public FunctionCallStatement(string name, IEnumerable<string> arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
        }
    }

    internal class CommandStatement : IStatement
    {
        public string CommandName { get; }
        public string[] Arguments { get; }

        public CommandStatement(string commandName, string[] arguments)
        {
            this.CommandName = commandName;
            this.Arguments = arguments;
        }
    }
}
