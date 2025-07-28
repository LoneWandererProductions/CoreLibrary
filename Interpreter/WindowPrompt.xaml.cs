/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        WindowPrompt.xaml.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/WindowPrompt.cs
 * PURPOSE:     Graphical Frontend for the Input
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable SwitchStatementMissingSomeCases

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ExtendedSystemObjects;

namespace Interpreter;

/// <inheritdoc cref="Window" />
/// <summary>
///     Start Console.xaml
/// </summary>
internal sealed partial class WindowPrompt
{
    /// <summary>
    ///     The code input (readonly). Value: new Dictionary&lt;int, string&gt;().
    /// </summary>
    private static readonly Dictionary<int, string> CodeInput = new();

    /// <summary>
    ///     The count up.
    /// </summary>
    private static int _countUp = -1;

    /// <summary>
    ///     The count down.
    /// </summary>
    private static int _countDown;

    /// <summary>
    ///     The interpret (readonly).
    /// </summary>
    private readonly IrtParserInput _interpret;

    /// <summary>
    ///     The in.
    /// </summary>
    private int _in = -1;

    /// <inheritdoc />
    /// <summary>
    ///     Fire it up
    /// </summary>
    internal WindowPrompt()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    /// <summary>
    ///     Fire it up
    /// </summary>
    /// <param name="interpret">Our Interpreter</param>
    internal WindowPrompt(IrtParserInput interpret)
    {
        _interpret = interpret;
        _interpret.SendInternalLog += SendLogs;
        InitializeComponent();
    }

    /// <summary>
    ///     Can display external Message
    /// </summary>
    /// <param name="messages">external Message</param>
    internal void FeedbackMessage(string messages)
    {
        TextDisplay.Text += messages;
    }

    /// <summary>
    ///     The text box inputs preview key down.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The key event arguments.</param>
    private void TextBoxInputs_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        //create character(test,1,test,test,4,5,6,7,8,9,10,True)
        switch (e.Key)
        {
            case Key.Enter:
                EnterKey();
                break;

            case Key.Up:
                UpKey();
                break;

            case Key.Down:
                DownKey();
                break;

            default:
                return;
        }
    }

    /// <summary>
    ///     Loads upper Element
    /// </summary>
    private void UpKey()
    {
        if (CodeInput.IsNullOrEmpty())
        {
            return;
        }

        _countUp++;

        if (!CodeInput.ContainsKey(_countDown))
        {
            _countDown = 0;
        }

        TextBoxInputs.Text = CodeInput[_countUp];
        TextBoxInputs.ScrollToEnd();
    }

    /// <summary>
    ///     Loads lower Element
    /// </summary>
    private void DownKey()
    {
        if (CodeInput.IsNullOrEmpty())
        {
            return;
        }

        _countDown--;

        if (!CodeInput.ContainsKey(_countDown))
        {
            _countDown = 0;
        }

        TextBoxInputs.Text = CodeInput[_countDown];
    }

    /// <summary>
    ///     Handle Enter Key
    /// </summary>
    private void EnterKey()
    {
        _countDown = CodeInput.Count;

        var input = TextBoxInputs.Text;
        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        //Handle Input
        //save as id
        _in++;
        TextDisplay.Text += input;
        TextDisplay.Text += Environment.NewLine;
        CodeInput.Add(_in, input);
        _interpret.HandleInput(input);
        TextBoxInputs.Clear();
    }

    /// <summary>
    ///     Display all Info on screen
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private void SendLogs(object sender, string e)
    {
        TextBoxInputs.Clear();
        TextDisplay.Text += e;
    }
}
