/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        Weave.cs
 * PURPOSE:     Shared string Resource Class.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver
{
    /// <summary>
    /// Global reusable resources for Weaver.
    /// </summary>
    internal static class WeaverResources
    {
        /// <summary>
        /// The global namespace
        /// </summary>
        internal const string GlobalNamespace = "global";

        /// <summary>
        /// The global help command
        /// </summary>
        internal const string GlobalCommandHelp = "help";

        /// <summary>
        /// The global extension help
        /// </summary>
        internal static string GlobalExtensionHelp => GlobalCommandHelp;

        /// <summary>
        /// The global extension try run
        /// </summary>
        internal const string GlobalExtensionTryRun = "tryrun";
    }
}