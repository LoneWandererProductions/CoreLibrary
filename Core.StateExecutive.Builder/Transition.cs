/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.StateExecutive.Builder
 * FILE:        Transition.cs
 * PURPOSE:     The Transition class implements the ITransition interface, representing a pathway from one state to another in the State Executive Engine. 
 *              It evaluates guards and resource claims before allowing a transition, and executes effects atomically when transitioning.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.StateExecutive.Interfaces;
using ExtendedSystemObjects;

namespace Core.StateExecutive.Builder
{
    /// <summary>
    /// ITransition implementation for the State Executive Engine. Represents a pathway from one state to another, with guards and effects.
    /// </summary>
    /// <seealso cref="Core.StateExecutive.Interfaces.ITransition" />
    internal sealed class Transition : ITransition
    {
        /// <inheritdoc />
        public string TargetStateId { get; }

        /// <summary>
        /// The condition
        /// </summary>
        private readonly Func<IStateContext, bool> _condition;

        /// <summary>
        /// The claims
        /// </summary>
        private readonly List<(string key, int amount)> _claims;

        /// <summary>
        /// The effects
        /// </summary>
        private readonly List<Action<IStateContext>> _effects;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transition"/> class.
        /// </summary>
        /// <param name="targetStateId">The target state identifier.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="claims">The claims.</param>
        /// <param name="effects">The effects.</param>
        public Transition(
            string targetStateId,
            Func<IStateContext, bool> condition,
            List<(string, int)> claims,
            List<Action<IStateContext>> effects)
        {
            TargetStateId = targetStateId;
            _condition = condition;
            _claims = claims;
            _effects = effects;
        }

        /// <inheritdoc />
        public bool CanTransition(IStateContext context)
        {
            // 1. Check logical guard
            if (!_condition(context)) return false;

            // 2. Check resource availability without consuming
            return _claims.AllFast(claim => context.HasResource(claim.key, claim.amount));
        }

        /// <inheritdoc />
        public void Execute(IStateContext context)
        {
            // Atomic transaction: Consume resources then run effects
            foreach (var claim in _claims)
            {
                if (!context.TryClaimResource(claim.key, claim.amount))
                {
                    context.Log($"CRITICAL: Thread collision or resource lost for {claim.key} during transition.");
                    return; // Abort
                }
            }

            foreach (var effect in _effects)
            {
                effect(context);
            }
        }
    }
}
