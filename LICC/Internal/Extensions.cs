using LICC.API;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IEnumerable<T> Yield<T>(this T obj)
        {
            yield return obj;
        }

        public static bool TrySingle<T>(this IEnumerable<T> ienum, Predicate<T> predicate, out T value)
        {
            value = ienum.SingleOrDefault(o => predicate(o));
            return value != default;
        }
    }
}
