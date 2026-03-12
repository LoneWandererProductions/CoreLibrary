/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.MemoryLog
 * FILE:        NullScope.cs
 * PURPOSE:     Null Scope for IDisposable pattern
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Core.MemoryLog
{
    /// <summary>
    /// Null scope for IDisposable pattern.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public sealed class NullScope : IDisposable
    {
        /// <summary>
        /// The instance
        /// </summary>
        public static readonly NullScope Instance = new();

        /// <summary>
        /// Prevents a default instance of the <see cref="NullScope"/> class from being created.
        /// </summary>
        private NullScope()
        {
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
