/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Helper
 * FILE:        ConsoleEventOutput.cs
 * PURPOSE:     Sample console side channel for event output.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using CoreBuilder.Interface;

namespace CoreBuilder.Helper
{
    /// <inheritdoc />
    /// <summary>
    /// Console side channel for event output.
    /// </summary>
    /// <seealso cref="CoreBuilder.Interface.IEventOutput" />
    public sealed class ConsoleEventOutput : IEventOutput
    {
        /// <inheritdoc />
        public void Write(string message) => Console.WriteLine(message);
    }

}
