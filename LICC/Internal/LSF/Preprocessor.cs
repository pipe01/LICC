using LICC.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LICC.Internal.LSF
{
    internal interface IPreprocessor
    {
        string Process(string fileContents);
    }

    internal class Preprocessor : IPreprocessor
    {
        private readonly IFileSystem FileSystem;

        public Preprocessor(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem;
        }

        public string Process(string fileContents)
        {
            var lines = fileContents.Replace("\r\n", "\n").Split('\n').ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                var match = Regex.Match(lines[i], "(?<=^@include \").*?(?=\"$)");

                if (match.Success)
                {
                    if (!FileSystem.FileExists(match.Value))
                        throw new PreprocessorException($"cannot find file at '{match.Value}'");

                    string[] includedFileLines = FileSystem.ReadAllLines(match.Value);

                    lines.RemoveAt(i);
                    lines.InsertRange(i, includedFileLines);

                    i--;
                }
            }

            lines.RemoveAll(o => o.FirstOrDefault(i => i != ' ') == '#');

            return string.Join("\n", lines);
        }
    }

    internal class PreprocessorException : Exception
    {
        public PreprocessorException()
        {
        }

        public PreprocessorException(string message) : base(message)
        {
        }

        public PreprocessorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PreprocessorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
