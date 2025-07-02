using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter.Resources;

namespace Interpreter
{
    internal class FeedbackManager
    {
        private IrtFeedback _currentFeedback;

        public FeedbackState State { get; private set; } = FeedbackState.Idle;

        public event EventHandler<IrtFeedbackInputEventArgs> FeedbackResolved;

        public void Request(IrtFeedback feedback)
        {
            if (string.IsNullOrEmpty(feedback?.RequestId)) return;

            _currentFeedback = feedback;
            State = FeedbackState.Waiting;
        }

        public void Reset()
        {
            _currentFeedback = null;
            State = FeedbackState.Idle;
        }

        public bool IsWaiting => State == FeedbackState.Waiting;

        public void ProcessInput(string input, Action<string> logCallback)
        {
            if (_currentFeedback?.Feedback == null)
            {
                logCallback?.Invoke(IrtConst.ErrorFeedbackMissing);
                Reset();
                return;
            }

            var result = IrtHelper.CheckInput(input, _currentFeedback.Feedback.Options);
            logCallback?.Invoke($"{IrtConst.FeedbackMessage} {input}");

            switch (result)
            {
                case >= 0:
                    var response = _currentFeedback.GenerateFeedbackAnswer((AvailableFeedback)result);
                    FeedbackResolved?.Invoke(this, response);
                    Reset();
                    break;
                case IrtConst.Error:
                    logCallback?.Invoke(IrtConst.ErrorFeedbackOptions);
                    break;
                case IrtConst.ErrorOptionNotAvailable:
                    logCallback?.Invoke(IrtConst.ErrorFeedbackOptionNotAllowed);
                    break;
            }
        }
    }
}
