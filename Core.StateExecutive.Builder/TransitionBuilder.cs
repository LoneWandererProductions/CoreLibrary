/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.StateExecutive.Builder
 * FILE:        TransitionBuilder.cs
 * PURPOSE:     Manages the construction of transitions in a fluent way for the State Executive Engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.StateExecutive.Interfaces;

namespace Core.StateExecutive.Builder
{
    /// <summary>
    /// Fluent builder for constructing transitions in the State Executive Engine.
    /// </summary>
    public sealed class TransitionBuilder
    {
        private readonly StateBuilder _parent;
        private readonly string _targetStateId;
        private Func<IStateContext, bool> _condition = _ => true;
        private readonly List<(string key, int amount)> _claims = new();
        private readonly List<Action<IStateContext>> _effects = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionBuilder"/> class.
        /// </summary>
        /// <param name="targetStateId">The target state identifier.</param>
        /// <param name="parent">The parent.</param>
        public TransitionBuilder(string targetStateId, StateBuilder parent)
        {
            _targetStateId = targetStateId;
            _parent = parent;
        }

        /// <summary>
        /// When the specified condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns>A Transition builder</returns>
        /// <exception cref="System.ArgumentNullException">condition</exception>
        public TransitionBuilder When(Func<IStateContext, bool> condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            return this;
        }

        /// <summary>
        /// Claims the specified resource key.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>A Transition builder</returns>
        public TransitionBuilder Claim(string resourceKey, int amount = 1)
        {
            _claims.Add((resourceKey, amount));
            return this;
        }

        /// <summary>
        /// Called when [transition].
        /// </summary>
        /// <param name="effect">The effect.</param>
        /// <returns>A Transition builder</returns>
        public TransitionBuilder OnTransition(Action<IStateContext> effect)
        {
            if (effect != null) _effects.Add(effect);
            return this;
        }

        /// <summary>
        /// Ends the transition.
        /// </summary>
        /// <returns>A State Builder</returns>
        public StateBuilder EndTransition()
        {
            var transition = new Transition(_targetStateId, _condition, _claims.ToList(), _effects.ToList());
            _parent.AddTransition(transition);
            return _parent;
        }
    }
}
