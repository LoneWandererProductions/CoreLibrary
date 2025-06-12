/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreInject
 * FILE:        CoreInjectResource.cs
 * PURPOSE:     CoreInject Resource File
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreInject
{
    /// <summary>
    /// All string Resources
    /// </summary>
    internal static class CoreInjectResource
    {
        /// <summary>
        /// The error no active scope
        /// </summary>
        internal const string ErrorNoActiveScope = "No active scope. Call BeginScope() first.";

        /// <summary>
        /// The error service registered single ton
        /// </summary>
        internal const string ErrorServiceRegisteredSingleTon = "Service {0} is already registered as singleton.";

        /// <summary>
        /// The error service registered
        /// </summary>
        internal const string ErrorServiceRegistered = "Service {0} is already registered.";

        /// <summary>
        /// The error service not registered
        /// </summary>
        internal const string ErrorServiceNotRegistered = "Service {0} is not registered.";

        /// <summary>
        /// The method register transient
        /// </summary>
        internal const string MethodRegisterTransient = "RegisterTransient";

        /// <summary>
        /// The variable register
        /// </summary>
        internal const string VariableRegister = "_registrations";

        /// <summary>
        /// The error registering service
        /// </summary>
        internal const string ErrorRegisteringService = "Error registering {0}: {1}";

        /// <summary>
        /// The error factory null
        /// </summary>
        internal const string ErrorFactoryNull = "Factory function cannot be null.";
    }
}
