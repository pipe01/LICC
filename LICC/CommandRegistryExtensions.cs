using LICC.API;
using System;
using System.Collections.Generic;
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
            List<Exception> exceptions = null;

            foreach (var item in assembly.GetTypes())
            {
                try
                {
                    registry.RegisterCommandsIn(item);
                }
                catch (Exception ex)
                {
                    (exceptions ??= new List<Exception>()).Add(ex);
                }
            }

            if (exceptions != null)
                throw new AggregateException("Exceptions when registering commands in assembly", exceptions);
        }

        /// <summary>
        /// Registers commands in all loaded assemblies.
        /// </summary>
        public static void RegisterCommandsInAllAssemblies(this ICommandRegistry registry)
        {
            List<Exception> exceptions = null;

            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!item.FullName.StartsWith("Unity"))
                        registry.RegisterCommandsIn(item);
                }
                catch (Exception ex)
                {
                    (exceptions ??= new List<Exception>()).Add(ex);
                }
            }

            if (exceptions != null)
                throw new AggregateException("Exceptions when registering commands in assemblies", exceptions);
        }
    }
}
