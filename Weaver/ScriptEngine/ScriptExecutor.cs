/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ScriptExecutor.cs
 * PURPOSE:     Executes parsed script statements using Weave
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Executes a list of script statements sequentially using Weave.
    /// Supports goto labels, do{...}while(condition) loops, and if/else.
    /// Handles feedback by pausing execution until input is provided.
    /// </summary>
    public sealed class ScriptExecutor
    {
        private readonly Weave _weave;
        private readonly IEvaluator _evaluator;
        private readonly List<(string Category, string)> _statements;

        private readonly Dictionary<string, int> _labelPositions;
        private int _position;
        private FeedbackRequest? _pendingFeedback;

        // Only store the start index of do-while loops
        private readonly Stack<int> _doWhileStack = new();

        public ScriptExecutor(Weave weave, List<(string Category, string)> statements)
        {
            _weave = weave;
            var registry = new VariableRegistry();
            _evaluator = new ExpressionEvaluator(registry);

            // Register internal commands
            weave.Register(new SetValueCommand(registry));
            weave.Register(new GetValueCommand(registry));
            weave.Register(new DeleteValueCommand(registry));
            weave.Register(new MemoryCommand(registry));
            //the evaluate command needs the evaluator and registry
            weave.Register(new EvaluateCommand(_evaluator, registry));

            _statements = statements ?? new List<(string Category, string)>();
            _position = 0;

            _labelPositions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Pre-scan labels
            for (var i = 0; i < _statements.Count; i++)
            {
                var (category, stmt) = _statements[i];
                if (category == "Label" && !string.IsNullOrEmpty(stmt))
                    _labelPositions[stmt] = i;
            }
        }

        /// <summary>
        /// True if the script has executed all statements and no feedback is pending.
        /// </summary>
        public bool IsFinished => _position >= _statements.Count && _pendingFeedback == null;

        /// <summary>
        /// Executes the next statement in the script or resumes after feedback.
        /// </summary>
        /// <param name="feedbackInput">Optional input if resuming after feedback.</param>
        /// <param name="maxIterations">Optional safety limit to prevent infinite loops.</param>
        /// <returns>CommandResult from Weave.</returns>
        public CommandResult ExecuteNext(string? feedbackInput = null, int? maxIterations = null)
        {
            // Resume pending feedback
            if (_pendingFeedback != null)
            {
                if (feedbackInput == null)
                    throw new InvalidOperationException("Feedback input required to continue script.");

                var result = _weave.ProcessInput(feedbackInput);
                if (result.Feedback == null)
                    _pendingFeedback = null;

                return result;
            }

            var iterCount = 0;

            while (_position < _statements.Count)
            {
                if (maxIterations.HasValue && iterCount++ >= maxIterations.Value)
                    return CommandResult.Fail("Max iteration count reached.");

                var (category, stmt) = _statements[_position];

                if (string.IsNullOrWhiteSpace(stmt))
                {
                    _position++;
                    continue;
                }

                switch (category)
                {
                    case "Goto":
                        if (_labelPositions.TryGetValue(stmt!, out var pos))
                            _position = pos + 1;
                        else
                        {
                            _position++;
                            return CommandResult.Fail($"Label '{stmt}' not found.");
                        }

                        continue;

                    case "Label":
                        _position++; // labels are just markers
                        continue;

                    case "Do_Open":
                        _doWhileStack.Push(_position);
                        _position++;
                        continue;

                    case "While_Condition":
                        if (_doWhileStack.Count == 0)
                        {
                            _position++;
                            continue;
                        }

                        var loopStart = _doWhileStack.Pop();
                        var condResult = _evaluator.Evaluate(stmt!);

                        if (condResult)
                            _position = loopStart + 1; // loop again
                        else
                            _position++; // exit loop
                        continue;

                    case "If_Condition":
                        var cond = _evaluator.Evaluate(stmt!);
                        _position++; // move past the condition node

                        if (!cond)
                        {
                            // Skip the "true" branch to the "Else_Open" if present, otherwise skip to next after If block
                            var depth = 0;
                            while (_position < _statements.Count)
                            {
                                var (cat, _) = _statements[_position];
                                if (cat == "If_Condition" || cat == "Do_Condition")
                                    depth++;
                                else if (cat == "Else_Open" && depth == 0)
                                    break;
                                else if (cat == "CloseBrace" && depth > 0)
                                    depth--;

                                _position++;
                            }
                        }

                        continue;


                    default: // "Command", "Assignment", etc.
                        var result = _weave.ProcessInput(stmt!);
                        if (result.Feedback != null)
                        {
                            _pendingFeedback = result.Feedback;
                            return result;
                        }

                        _position++;
                        return result;
                }
            }

            return new CommandResult
            {
                Success = true,
                Message = "Script finished.",
                Feedback = null
            };
        }
    }
}