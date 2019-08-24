using System;
using System.Reflection;

namespace LICC
{
    public static class CommandRegistryExtensions
    {
        /// <summary>
        /// Registers all command methods in all types contained in <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly containing types containing commands.</param>
        public static void RegisterCommandsIn(this ICommandRegistry registry, Assembly assembly)
        {
            foreach (var item in assembly.GetTypes())
            {
                registry.RegisterCommandsIn(item);
            }
        }

        /// <summary>
        /// Registers commands in all loaded assemblies.
        /// </summary>
        public static void RegisterCommandsInAllAssemblies(this ICommandRegistry registry)
        {
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                registry.RegisterCommandsIn(item);
            }
        }
    }
}
