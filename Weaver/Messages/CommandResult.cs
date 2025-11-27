/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        CommandResult.cs
 * PURPOSE:     Message Object, encapsulates the result of a command execution.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using System.Diagnostics;

namespace Weaver.Messages
{
    /// <summary>
    /// The Result of a command execution.
    /// </summary>
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    public sealed class CommandResult
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; init; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CommandResult"/> is success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success { get; init; }

        /// <summary>
        /// Gets a value indicating whether [requires confirmation].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires confirmation]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresConfirmation { get; init; }

        /// <summary>
        /// Gets the suggestions.
        /// </summary>
        /// <value>
        /// The suggestions.
        /// </value>
        public string[]? Suggestions { get; init; }

        /// <summary>
        /// Oks the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        public static CommandResult Ok(string msg) => new() { Success = true, Message = msg };

        /// <summary>
        /// Fails the specified MSG.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        public static CommandResult Fail(string msg) => new() { Success = false, Message = msg };

        /// <summary>
        /// Success variant.
        /// Oks the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns>Drop in ready Command.</returns>
        public static CommandResult Ok(string message, object? value = null, EnumTypes type = EnumTypes.Wstring)
            => new() { Success = true, Message = message, Value = value, Type = type };

        /// <summary>
        /// Failure variant.
        /// Fails the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns>Drop in ready Command.</returns>
        public static CommandResult Fail(string message, object? value = null, EnumTypes type = EnumTypes.Wstring)
            => new() { Success = false, Message = message, Value = value, Type = type };

        /// <summary>
        /// Gets the feedback.
        /// </summary>
        /// <value>
        /// The feedback.
        /// </value>
        public FeedbackRequest? Feedback { get; init; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public EnumTypes Type { get; init; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object? Value { get; init; }

        /// <summary>
        /// Returns a human-readable string representation of the command result.
        /// </summary>
        /// <summary>
        /// Returns a human-readable string representation of the command result.
        /// </summary>
        public override string ToString()
        {
            var suggestions = (Suggestions == null || Suggestions.Length == 0)
                ? "<none>"
                : string.Join(", ", Suggestions);

            var feedback = Feedback == null
                ? "<none>"
                : $"Id={Feedback.RequestId}";

            var valuePart = Value == null
                ? "<null>"
                : $"{Value} ({Value.GetType().Name})";

            return
                $"[{(Success ? "OK" : "FAIL")}] " +
                $"Msg=\"{Message}\" | Confirm={RequiresConfirmation} | " +
                $"Type={Type} | Value={valuePart} | " +
                $"Feedback={feedback} | Suggestions=[{suggestions}]";
        }

        /// <summary>
        /// Shortened display for debugger visualizers.
        /// </summary>
        private string GetDebuggerDisplay()
        {
            var status = Success ? "OK" : "FAIL";
            return $"{status}: {Message}";
        }
    }
}