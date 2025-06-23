/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/UserFeedback.cs
 * PURPOSE:     Dialog Options for the user
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Text;
using Interpreter.Resources;

namespace Interpreter
{
    /// <summary>
    ///     Object that will hold the Information for the user about user confirmations
    /// </summary>
    public sealed class UserFeedback
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="UserFeedback" /> is before.
        /// </summary>
        /// <value>
        ///     <c>true</c> if before; otherwise, <c>false</c>.
        /// </value>
        public bool Before { get; init; }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string Message { get; init; }

        /// <summary>
        ///     Gets or sets the options. The Hardcoded options like yes, no, cancel, only one is allowed
        ///     Value is the Message fitting to the Key.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        public Dictionary<AvailableFeedback, string> Options { get; init; }

        /// <summary>
        ///     Converts to string.
        ///     Builds the output Message
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance, creates the User message.
        /// </returns>
        public override string ToString()
        {
            var message = new StringBuilder();
            message.Append(Message); // Start with the main message

            if (!(Options?.Count > 0))
            {
                return message.ToString();
            }

            // Iterate through each key-value pair in Options
            foreach (var (key, value) in Options)
            {
                message.Append($" {key} {value},"); // Format key-value pairs
            }

            message.Length -= 2; // Remove the last ", " to avoid extra trailing comma

            return message.ToString();
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Before, Message, Options);
        }
    }
}
