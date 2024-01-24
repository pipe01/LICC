using System;

namespace LICC.API
{
    /// <summary>
    /// Public interface for a command registry.
    /// </summary>
    public interface ICommandRegistry
    {
        /// <summary>
        /// Registers all command methods in <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type containing methods marked with the <see cref="CommandAttribute"/> attribute.</param>
        void RegisterCommandsIn(Type type);
    }
}
