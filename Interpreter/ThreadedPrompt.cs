/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        ThreadedPrompt.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Interpreter;

//TODO implement for multi threading
public class ThreadedPrompt
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<string>> _pendingInputs;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadedPrompt"/> class.
    /// </summary>
    public ThreadedPrompt()
    {
        _pendingInputs = new ConcurrentDictionary<Guid, TaskCompletionSource<string>>();
    }

    // Method to initiate a request for input, returns a task that will complete when input is provided
    public Task<string> RequestInputAsync(Guid callerId)
    {
        var tcs = new TaskCompletionSource<string>();
        _pendingInputs[callerId] = tcs;
        return tcs.Task;
    }

    /// <summary>
    ///  Method to provide input, checks if the input matches the caller's request
    /// </summary>
    /// <param name="callerId">The caller identifier.</param>
    /// <param name="input">The input.</param>
    public void ConsoleInput(Guid callerId, string input)
    {
        if (_pendingInputs.TryRemove(callerId, out var tcs))
        {
            tcs.SetResult(input);
        }
        else
        {
            Console.WriteLine($"No pending input request for caller {callerId}");
        }
    }
}
