/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        IRpnEngine.cs
 * PURPOSE:     Mathmatical RPN Engine interface
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Interfaces
{
    /// <summary>
    /// Contract for a Reverse Polish Notation (RPN) evaluation engine.
    /// </summary>
    internal interface IRpnEngine
    {
        /// <summary>
        /// Evaluates the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// Result of evaluation.
        /// </returns>
        double EvaluateRpn(List<string> tokens);
    }
}