/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.Feedback
 * FILE:        FeedbackManager.cs
 * PURPOSE:     Manages user feedback interaction and input validation within the command interpreter.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using Interpreter.Resources;

namespace Interpreter.Feedback;

/// <summary>
///     Responsible for managing the state and processing of user feedback requests.
///     Handles validation of input against expected feedback options,
///     raises events when feedback is resolved, and manages internal state.
/// </summary>
internal class FeedbackManager
{
    /// <summary>
    ///     Stores the current feedback request that is waiting for user input.
    /// </summary>
    private IrtFeedback _currentFeedback;

    /// <summary>
    ///     Represents the current state of the feedback manager,
    ///     e.g. whether it is idle or waiting for input.
    /// </summary>
    public FeedbackState State { get; private set; } = FeedbackState.Idle;

    /// <summary>
    ///     Indicates whether the manager is currently waiting for user input.
    /// </summary>
    public bool IsWaiting => State == FeedbackState.Waiting;

    /// <summary>
    ///     Event that is triggered when user feedback has been successfully processed and resolved.
    ///     Subscribers receive the full feedback response event args.
    /// </summary>
    public event EventHandler<IrtFeedbackInputEventArgs> FeedbackResolved;

    /// <summary>
    ///     Starts a new feedback request by storing the request and
    ///     setting the state to Waiting, if the request ID is valid.
    /// </summary>
    /// <param name="feedback">The feedback request object containing details and expected options.</param>
    public void Request(IrtFeedback feedback)
    {
        if (string.IsNullOrEmpty(feedback?.RequestId))
        {
            return; // Ignore invalid requests without a RequestId
        }

        _currentFeedback = feedback;
        State = FeedbackState.Waiting;
    }

    /// <summary>
    ///     Resets the manager state and clears the current feedback request,
    ///     returning to the idle state.
    /// </summary>
    public void Reset()
    {
        _currentFeedback = null;
        State = FeedbackState.Idle;
    }

    /// <summary>
    ///     Processes user input in response to the current feedback request.
    ///     Validates the input against allowed options, invokes logging callbacks,
    ///     raises the FeedbackResolved event if input is valid, or logs errors otherwise.
    /// </summary>
    /// <param name="input">The user's input string.</param>
    /// <param name="logCallback">An action to receive logging messages.</param>
    public void ProcessInput(string input, Action<string> logCallback)
    {
        // Validate current feedback presence and options
        if (_currentFeedback?.Feedback == null)
        {
            logCallback?.Invoke(IrtConst.ErrorFeedbackMissing);
            Reset();
            return;
        }

        // Check if the input is among valid feedback options
        var result = IrtHelper.CheckInput(input, _currentFeedback.Feedback.Options);

        // Log the received feedback input
        logCallback?.Invoke($"{IrtConst.FeedbackMessage} {input}");

        switch (result)
        {
            // Valid input: generate full feedback answer and notify subscribers
            case >= 0:
                var response = _currentFeedback.GenerateFeedbackAnswer((AvailableFeedback)result);
                FeedbackResolved?.Invoke(this, response);
                Reset();
                break;

            // Input error: notify user about invalid feedback options
            case IrtConst.Error:
                logCallback?.Invoke(IrtConst.ErrorFeedbackOptions);
                break;

            // Input is not in the allowed options: notify user
            case IrtConst.ErrorOptionNotAvailable:
                logCallback?.Invoke(IrtConst.ErrorFeedbackOptionNotAllowed);
                break;
        }
    }
}
