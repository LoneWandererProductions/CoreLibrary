/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        ScriptExecutor.cs
 * PURPOSE:     Executes parsed script statements using Weave
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

//TODO the node exit for DoWhile is currently not implemented in the ScriptExecutor

using System.Diagnostics;
using Weaver.Evaluate;
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
        /// <summary>
        /// The weave
        /// </summary>
        private readonly Weave _weave;

        /// <summary>
        /// The evaluator
        /// </summary>
        private readonly IEvaluator _evaluator;

        /// <summary>
        /// The statements
        /// </summary>
        private readonly List<(string Category, string)> _statements;

        /// <summary>
        /// The label positions
        /// </summary>
        private readonly Dictionary<string, int> _labelPositions;

        /// <summary>
        /// The position
        /// </summary>
        private int _position;

        /// <summary>
        /// The debug
        /// </summary>
        private readonly bool _debug;

        /// <summary>
        /// The pending feedback
        /// </summary>
        private FeedbackRequest? _pendingFeedback;

        /// <summary>
        /// Only store the start index of do-while loops
        /// </summary>
        private readonly Stack<int> _doWhileStack = new();

        //private readonly Stack<(int BodyStart, int WhileIndex)> _doWhileStack = new();


        /// <summary>
        /// Current instruction index.
        /// </summary>
        /// <value>
        /// The instruction pointer.
        /// </value>
        public int InstructionPointer => _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptExecutor" /> class.
        /// </summary>
        /// <param name="weave">The weave.</param>
        /// <param name="statements">The statements.</param>
        /// <param name="debug">if set to <c>true</c> [debug].</param>
        public ScriptExecutor(Weave weave, List<(string Category, string)> statements, bool debug = false)
        {
            //now weave also holds all the variables and evaluator commands we need.
            _weave = weave;
            _evaluator = new ExpressionEvaluator(_weave.Runtime.Variables);
            _statements = statements ?? new List<(string Category, string)>();
            _position = 0;
            _debug = debug;

            _labelPositions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < _statements.Count; i++)
            {
                if (_statements[i].Category == "Label")
                    _labelPositions[_statements[i].Item2] = i;
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
            if (_pendingFeedback != null)
            {
                if (feedbackInput == null)
                    throw new InvalidOperationException("Feedback input required to continue script.");

                var result = _weave.ProcessInput(feedbackInput);
                if (result.Feedback == null)
                    _pendingFeedback = null;

                if (_debug) DebugInternal(result);

                return result;
            }

            int iterCount = 0;

            while (_position < _statements.Count)
            {
                if (maxIterations.HasValue && iterCount++ >= maxIterations.Value)
                {
                    if (_debug)
                        Trace.WriteLine($"[Internal] Max iteration count ({maxIterations}) reached at Pos={_position}. Aborting.");
                    return CommandResult.Fail("Max iteration count reached.");
                }

                var (category, stmt) = _statements[_position];

                // Skip empty commands only
                if (category == ScriptConstants.CommandToken && string.IsNullOrWhiteSpace(stmt))
                {
                    if (_debug)
                        Trace.WriteLine($"[Internal] Skipping empty command at Pos={_position}.");
                    _position++;
                    continue;
                }

                if (_debug)
                    DebugLine(category, stmt, StepType.Input, "Stack",  string.Join(",", _doWhileStack));

                switch (category.Trim())
                {
                    case ScriptConstants.GotoToken:
                        if (_debug)
                            DebugLine(category, stmt, StepType.Execute, "Postion", string.Join(",", _position));

                        if (_labelPositions.TryGetValue(stmt!, out var pos))
                        {
                            if (_debug)
                                Trace.WriteLine($"[Debug] Goto '{stmt}' found at {pos}. Jumping from {_position} to {pos + 1}.");
                            _position = pos + 1;
                        }
                        else
                        {
                            if (_debug)
                                Trace.WriteLine($"[Debug] Label '{stmt}' not found at Pos={_position}.");
                            _position++;
                            return CommandResult.Fail($"Label '{stmt}' not found.");
                        }
                        continue;

                    case ScriptConstants.LabelToken:
                        if (_debug)
                            DebugLine(category, stmt, StepType.Execute, "Postion", string.Join(",", _position));
                        _position++;
                        continue;

                    case ScriptConstants.DoOpenToken:
                        if (_debug)
                            DebugLine(category, stmt, StepType.Execute, "Postion", string.Join(",", _position));

                        _doWhileStack.Push(_position + 1);
                        if (_debug)
                            Trace.WriteLine($"[Debug] Do_Open at {_position}. Pushed body start {_position + 1}. Stack: [{string.Join(",", _doWhileStack)}]");
                        _position++;
                        continue;

                    case ScriptConstants.WhileConditionToken:

                        if (_debug)
                            DebugLine(category, stmt, StepType.Execute, "Postion", string.Join(",", _position));

                        if (_doWhileStack.Count == 0)
                        {
                            if (_debug)
                                Trace.WriteLine($"[Debug] Warning: While_Condition without Do_Open at Pos={_position}.");
                            _position++;
                            continue;
                        }

                        int bodyStart = _doWhileStack.Peek();

                        bool cond = _evaluator.Evaluate(stmt!);
                        //Todo Error is here
                        //Trace.WriteLine($"[Debug] EVAL: '{stmt}' → {cond}. counter={_weave.Runtime.Variables.Get("counter")}");


                        if (_debug)
                            Trace.WriteLine($"[Debug] While_Condition at {_position}, bodyStart={bodyStart}, cond={cond}, Stack=[{string.Join(",", _doWhileStack)}]");

                        if (cond)
                        {
                            _position = bodyStart; // repeat loop
                            if (_debug)
                                Trace.WriteLine($"[Debug] Condition true → jumping back to {bodyStart}.");
                        }
                        else
                        {
                            _doWhileStack.Pop();
                            if (_debug)
                                Trace.WriteLine($"[Debug] Condition false → exiting loop. Stack after pop: [{string.Join(",", _doWhileStack)}]");
                            _position++; // move past While_Condition
                        }
                        continue;

                    case ScriptConstants.DoEndToken:
                        if (_debug)
                            DebugLine(category, stmt, StepType.Execute, "Postion", string.Join(",", _position));

                        _position++;
                        continue;

                    case ScriptConstants.IfConditionToken:
                        if (_debug)
                            DebugLine(category, stmt, StepType.Execute, "Postion", string.Join(",", _position));

                        bool ifcond = _evaluator.Evaluate(stmt!);
                        if (_debug)
                            Trace.WriteLine($"[Debug] If_Condition at {_position}, cond={ifcond}");

                        _position++;
                        if (!ifcond)
                        {
                            int depth = 0;
                            while (_position < _statements.Count)
                            {
                                var (cat, _) = _statements[_position];
                                if (cat == ScriptConstants.IfConditionToken) { depth++; _position++; }
                                else if (cat == ScriptConstants.IfEndToken && depth == 0) { _position++; break; }
                                else if (cat == ScriptConstants.ElseOpenToken && depth == 0) { _position++; break; }
                                else if ((cat == ScriptConstants.IfEndToken || cat == ScriptConstants.ElseEndToken) && depth > 0) { depth--; _position++; }
                                else { _position++; }
                            }
                            if (_debug)
                                Trace.WriteLine($"[Debug] Skipped false If_Condition block. New Pos={_position}");
                        }
                        continue;

                    case ScriptConstants.ElseOpenToken:
                    case ScriptConstants.IfEndToken:
                    case ScriptConstants.ElseEndToken:
                        if (_debug)
                            DebugLine(category, stmt, StepType.Execute, "Postion", string.Join(",", _position));

                        _position++;
                        continue;

                    default:
                        if (_debug)
                            DebugLine(category, stmt, StepType.Execute, "Postion", string.Join(",", _position));

                        var result = _weave.ProcessInput(stmt!);
                        if (_debug)
                            DebugLine(category, stmt, StepType.Output, "Result", $"Success ={ result.Success}, Feedback ={ (result.Feedback != null ? "<pending>" : "null")}, Pos ={ _position}");



                        if (result.Feedback != null)
                        {
                            _pendingFeedback = result.Feedback;
                            return result;
                        }

                        _position++;
                        return result;
                }
            }

            if (_debug)
                Trace.WriteLine("[Internal] Script finished successfully.");

            return new CommandResult
            {
                Success = true,
                Message = "Script finished.",
                Feedback = null
            };
        }

        /// <summary>
        /// Debugs the internal.
        /// </summary>
        /// <param name="result">The result.</param>
        private void DebugInternal(CommandResult result)
        {
            Trace.WriteLine($"[Internal] Resumed after feedback. Success={result.Success}, Feedback={(result.Feedback != null ? "<pending>" : "null")}, Message={result.Message}");
        }

        /// <summary>
        /// Debugs the line.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="stmt">The statement.</param>
        private void DebugLine(string category, string stmt, StepType step, string extra, string info)
        {
            switch (step)
            {
                case StepType.Input:
                    Trace.WriteLine($"{"[Input] " + category} : {stmt ?? "<null>"} | Pos={_position} | {extra}=[{info}]");
                    break;
                case StepType.Execute:
                    Trace.WriteLine($"{"[Execute] " + category} : {stmt ?? "<null>"} | Pos={_position} | {extra}=[{info}]");
                    break;
                case StepType.Output:
                    break;
                case StepType.Internal:
                    break;
            }
        }
    }
}