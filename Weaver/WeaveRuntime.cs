/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        WeaveRuntime.cs
 * PURPOSE:     Connects the variable registry to the script engine
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.ScriptEngine;

namespace Weaver
{
    /// <summary>
    /// The Weave runtime, holds the variable registry for scripts.
    /// </summary>
    public sealed class WeaveRuntime
    {
        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IVariableRegistry Variables { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeaveRuntime"/> class.
        /// </summary>
        public WeaveRuntime()
        {
            Variables = new VariableRegistry();
        }
    }
}
