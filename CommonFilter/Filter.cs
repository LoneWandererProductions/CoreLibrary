﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/Filter.cs
 * PURPOSE:     Implementation of IFilter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace CommonFilter
{
    /// <summary>
    ///     Filter Interface implementation
    /// </summary>
    /// <seealso cref="IFilter" />
    public sealed class Filter : IFilter
    {
        /// <summary>
        ///     The evaluate
        /// </summary>
        private ILogicEvaluations _evaluate;

        /// <summary>
        ///     The filter
        /// </summary>
        private FilterWindow _filter;

        /// <summary>
        ///     Gets or sets the options.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        internal List<FilterOption> Conditions { get; set; }

        /// <summary>
        ///     Occurs when [filter changed].
        /// </summary>
        public event EventHandler FilterChanged;

        /// <summary>
        ///     Starts this instance.
        /// </summary>
        public void Start()
        {
            _evaluate = new LogicEvaluations();
            _filter = new FilterWindow(this);
            _filter.ShowDialog();
        }

        /// <summary>
        ///     Starts the specified evaluate.
        ///     So we can use custom Evaluations
        /// </summary>
        /// <param name="evaluate">The evaluate.</param>
        public void Start(ILogicEvaluations evaluate)
        {
            _evaluate = evaluate;
            _filter = new FilterWindow(this);
            _filter.ShowDialog();
        }

        /// <summary>
        ///     Checks the filter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Condition fulfilled?</returns>
        public bool CheckFilter(string input)
        {
            if (Conditions.Count == 0 || string.IsNullOrEmpty(input))
            {
                return false;
            }

            return _evaluate.Evaluate(input, Conditions);
        }

        /// <summary>
        ///     Done.
        /// </summary>
        internal void Done()
        {
            FilterChanged(this, EventArgs.Empty);
            _filter.Close();
        }
    }
}
