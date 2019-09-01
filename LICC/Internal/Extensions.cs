using LICC.API;
using System.Collections.Generic;

namespace LICC.Internal
{
    internal static class Extensions
    {
        public static string ReadAllText(this IFileSystem fileSystem, string filePath)
        {
            using (var reader = fileSystem.OpenRead(filePath))
                return reader.ReadToEnd();
        }

        public static string[] ReadAllLines(this IFileSystem fileSystem, string filePath)
        {
            using (var reader = fileSystem.OpenRead(filePath))
            {
                var lines = new List<string>();

                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }

                return lines.ToArray();
            }
        }
    }
}
