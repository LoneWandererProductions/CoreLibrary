using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Interpreter
{
    //TODO implement for multihreading
    public class ThreadedPrompt
    {
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<string>> _pendingInputs;

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

        // Method to provide input, checks if the input matches the caller's request
        public void ConsoleInput(Guid callerId, string input)
        {
            if (_pendingInputs.TryRemove(callerId, out var tcs))
                tcs.SetResult(input);
            else
                Console.WriteLine($"No pending input request for caller {callerId}");
        }
    }
}