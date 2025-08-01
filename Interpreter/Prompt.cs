﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/Prompt.cs
 * PURPOSE:     Handles user input and serves as the entry point for all commands.
 *              Currently designed as a single-instance class.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ExtendedSystemObjects;
using Interpreter.Interface;
using Interpreter.Resources;

namespace Interpreter;

/// <summary>
///     Bucket List:
///     - nested Commands
/// </summary>
/// <seealso cref="IPrompt" />
/// <seealso cref="IDisposable" />
/// <inheritdoc cref="IDisposable" />
/// <inheritdoc cref="IPrompt" />
public sealed class Prompt : IPrompt, IDisposable
{
    /// <summary>
    ///     Used to interpret commands.
    ///     Shared across all instances as static for single-instance design.
    /// </summary>
    private static IrtParserInput _interpret;

    /// <summary>
    ///     The count
    /// </summary>
    private static int _count = -1;

    /// <summary>
    ///     The feedback manager
    /// </summary>
    private readonly FeedbackManager _feedbackManager = new();

    /// <summary>
    ///     User Input Windows
    /// </summary>
    private WindowPrompt _prompt;

    /// <summary>
    ///     Gets or sets the state.
    /// </summary>
    /// <value>
    ///     The state.
    /// </value>
    private PromptState _state;

    /// <summary>
    ///     The collected Namespaces
    /// </summary>
    public Dictionary<string, UserSpace> CollectedSpaces { get; private set; }

    /// <summary>
    ///     Send selected Command to the Subscriber
    /// </summary>
    public EventHandler<OutCommand> SendCommands { get; set; }

    /// <summary>
    ///     Send Message to the Subscriber
    /// </summary>
    public EventHandler<string> SendLogs { get; set; }

    /// <summary>
    ///     Gets the log.
    /// </summary>
    /// <value>
    ///     The log.
    /// </value>
    public Dictionary<int, string> Log { get; private set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="Prompt" /> is disposed.
    /// </summary>
    /// <value>
    ///     <c>true</c> if disposed; otherwise, <c>false</c>.
    /// </value>
    public bool Disposed { get; private set; }

    /// <summary>
    ///     Logs all activities
    /// </summary>
    private static int MaxLines => 1000;

    /// <inheritdoc />
    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Starts the sender and interpreter with the given command set and optional extensions.
    ///     UserSpace commands do not overwrite system commands.
    ///     Parentheses can be either '(' or '[', as defined by the user.
    /// </summary>
    /// <param name="com">The dictionary of commands to register.</param>
    /// <param name="userSpace">The user space namespace for the commands.</param>
    /// <param name="extension">Optional dictionary of extension commands.</param>
    /// <param name="userFeedback">Optional dictionary of user feedback handlers.</param>
    public void Initiate(Dictionary<int, InCommand> com = null, string userSpace = "",
        Dictionary<int, InCommand> extension = null, Dictionary<int, UserFeedback> userFeedback = null)
    {
        _state = PromptState.Normal;
        ResetState();
        //Userspace handler
        var use = new UserSpace { UserSpaceName = userSpace, Commands = com, ExtensionCommands = extension };

        //Upper is needed because of the way we compare commands in the Interpreter
        CollectedSpaces.AddDistinct(userSpace.ToUpper(), use);

        //feedback stuff
        _feedbackManager.FeedbackResolved += (s, e) => OnHandleFeedback(e);
        _interpret = new IrtParserInput(this, userFeedback);
        _interpret.Initiate(use);
        _interpret.SendInternalLog += SendLog;
    }

    /// <inheritdoc />
    /// <summary>Start the Sender and Interpreter</summary>
    /// <param name="com">Command Register</param>
    /// <param name="userSpace">UserSpace of the register</param>
    /// <param name="extension">Optional Extension Methods</param>
    public void AddCommands(Dictionary<int, InCommand> com, string userSpace,
        Dictionary<int, InCommand> extension = null)
    {
        if (CollectedSpaces.IsNullOrEmpty())
        {
            SendLogs?.Invoke(this, IrtConst.ErrorNotInitialized);
            return;
        }

        var use = CreateUserSpace(userSpace, com);
        CollectedSpaces.AddDistinct(userSpace.ToUpper(), use);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Start a Window for the Input.
    ///     Included and optional
    /// </summary>
    public void StartWindow()
    {
        _prompt = new WindowPrompt(_interpret) { ShowInTaskbar = true };
        _prompt.Show();
    }

    /// <inheritdoc />
    /// <summary>
    ///     Handle it within the code, no User Input
    /// </summary>
    /// <param name="input">Input string</param>
    public void ConsoleInput(string input)
    {
        switch (_state)
        {
            case PromptState.Normal:
                _interpret?.HandleInput(input);
                break;
            case PromptState.WaitingForFeedback:
                CheckFeedback(input);
                break;
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Callback from the Outside, here for window
    /// </summary>
    /// <param name="message">Feedback Message for Display</param>
    public void CallbacksWindow(string message)
    {
        _prompt?.FeedbackMessage(message);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Generic Callback from Outside, will appear in the window as SendLog Event
    /// </summary>
    /// <param name="message">Feedback Message for Display</param>
    public void Callback(string message)
    {
        SendLogs?.Invoke(nameof(Callback), message);
    }

    /// <summary>
    ///     Occurs when feedback input has been processed and needs handling.
    ///     Subscribers should process the <see cref="IrtFeedbackInputEventArgs" /> accordingly.
    /// </summary>
    public event EventHandler<IrtFeedbackInputEventArgs> HandleFeedback;

    /// <summary>Switches the name spaces.</summary>
    /// <param name="space">The Namespace we would like to use.</param>
    internal void SwitchNameSpaces(string space)
    {
        space = space.ToUpper(CultureInfo.InvariantCulture);
        var use = CollectedSpaces[space];
        IrtParserInput.SwitchUserSpace(use);
    }

    /// <summary>
    ///     Return the selected Command
    /// </summary>
    /// <param name="sender">Object</param>
    /// <param name="e">Type</param>
    internal void SendCommand(object sender, OutCommand e)
    {
        AddToLog(e.Command == IrtConst.Error ? e.ErrorMessage : e.Command.ToString());

        SendCommands?.Invoke(nameof(Prompt), e);
    }

    /// <summary>
    ///     Send the News Out
    /// </summary>
    /// <param name="sender">Object</param>
    /// <param name="e">Type</param>
    internal void SendLog(object sender, string e)
    {
        AddToLog(e);
        SendLogs?.Invoke(nameof(Prompt), e);
    }

    /// <summary>
    ///     Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///     unmanaged resources.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        if (disposing)
        {
            _interpret = null;
            CollectedSpaces = null;
            Log = null;
            _prompt?.Close();
        }

        Disposed = true;
    }

    /// <summary>
    ///     Adds to log.
    /// </summary>
    /// <param name="message">The message.</param>
    internal void AddToLog(string message)
    {
        if (_count == MaxLines)
        {
            Log.Remove(Log.Keys.First());
        }

        _count++;
        Log?.Add(_count, message);
    }

    /// <summary>
    ///     Resets the state.
    /// </summary>
    private void ResetState()
    {
        CollectedSpaces = new Dictionary<string, UserSpace>();
        Log = new Dictionary<int, string>();
        _count = -1;
    }

    /// <summary>
    ///     Creates the user space.
    /// </summary>
    /// <param name="userSpace">The user space.</param>
    /// <param name="com">The COM.</param>
    /// <returns>New UserSpace</returns>
    private UserSpace CreateUserSpace(string userSpace, Dictionary<int, InCommand> com)
    {
        var use = new UserSpace { UserSpaceName = userSpace, Commands = com };
        CollectedSpaces.AddDistinct(userSpace.ToUpper(), use);
        return use;
    }

    /// <summary>
    ///     NOTE: Leave out the finalizer altogether if this class doesn't
    ///     own unmanaged resources, but leave the other methods
    ///     exactly as they are.
    ///     Finalizes an instance of the <see cref="Prompt" /> class.
    /// </summary>
    ~Prompt()
    {
        // Finalizer calls Dispose(false)
        Dispose(false);
    }


    /// <summary>
    ///     Requests the feedback.
    /// </summary>
    /// <param name="feedbackRequest">The feedback request.</param>
    internal void RequestFeedback(IrtFeedback feedbackRequest)
    {
        _feedbackManager.Request(feedbackRequest);
        _state = PromptState.WaitingForFeedback;

        SendLogs?.Invoke(this, feedbackRequest.Feedback?.ToString());
    }

    /// <summary>
    ///     Checks the feedback.
    /// </summary>
    /// <param name="input">The input.</param>
    internal void CheckFeedback(string input)
    {
        _feedbackManager.ProcessInput(input, msg => SendLogs(this, msg));
        if (!_feedbackManager.IsWaiting)
        {
            _state = PromptState.Normal;
        }
    }

    /// <summary>
    ///     Raises the <see cref="E:HandleFeedback" /> event.
    /// </summary>
    /// <param name="e">The <see cref="IrtFeedbackInputEventArgs" /> instance containing the event data.</param>
    internal void OnHandleFeedback(IrtFeedbackInputEventArgs e)
    {
        HandleFeedback?.Invoke(this, e); // Null-conditional operator to safely invoke event
    }
}
