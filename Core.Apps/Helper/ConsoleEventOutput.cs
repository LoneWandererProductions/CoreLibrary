/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Helper
 * FILE:        ConsoleEventOutput.cs
 * PURPOSE:     Sample console side channel for event output.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using Core.Apps.Interface;

namespace Core.Apps.Helper
{
    /// <inheritdoc />
    /// <summary>
    /// Console side channel for event output.
    /// </summary>
    /// <seealso cref="IEventOutput" />
    public sealed class ConsoleEventOutput : IEventOutput
    {
        /// <inheritdoc />
        public void Write(string message) => Console.WriteLine(message);
    }
}
