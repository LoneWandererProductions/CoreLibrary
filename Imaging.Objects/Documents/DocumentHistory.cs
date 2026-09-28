/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        DocumentHistory.cs
 * PURPOSE:     Undo/redo stack of a document, with a "saved" marker for the dirty flag.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Interfaces;

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     Undo/redo for one <see cref="Document" />. Steps are commands, so memory grows with what was
    ///     edited, not with image size times number of steps.
    /// </summary>
    public sealed class DocumentHistory : IDisposable
    {
        private readonly Document _document;

        private readonly List<IDocumentCommand> _redo = new();

        private readonly List<IDocumentCommand> _undo = new();

        // Undo depth of the saved state; -1 when that state can no longer be reached.
        private int _cleanDepth;

        /// <summary>Initializes a new instance of the <see cref="DocumentHistory" /> class.</summary>
        /// <param name="document">The document.</param>
        /// <param name="limit">Maximum number of undo steps kept.</param>
        public DocumentHistory(Document document, int limit = 50)
        {
            ArgumentNullException.ThrowIfNull(document);

            _document = document;
            Limit = Math.Max(1, limit);
        }

        /// <summary>Gets the maximum number of undo steps.</summary>
        public int Limit { get; }

        /// <summary>Gets a value indicating whether there is something to undo.</summary>
        public bool CanUndo => _undo.Count > 0;

        /// <summary>Gets a value indicating whether there is something to redo.</summary>
        public bool CanRedo => _redo.Count > 0;

        /// <summary>Gets the description of the step <see cref="Undo" /> would revert.</summary>
        public string? UndoDescription => CanUndo ? _undo[^1].Description : null;

        /// <summary>Gets the description of the step <see cref="Redo" /> would re-apply.</summary>
        public string? RedoDescription => CanRedo ? _redo[^1].Description : null;

        /// <summary>Gets a value indicating whether the document differs from the state marked by <see cref="MarkClean" />.</summary>
        public bool IsDirty => _cleanDepth != _undo.Count;

        /// <summary>Raised whenever CanUndo, CanRedo, the descriptions or IsDirty may have changed.</summary>
        public event EventHandler? Changed;

        /// <summary>Applies a command and makes it undoable. If it throws, nothing is recorded.</summary>
        public void Execute(IDocumentCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.Apply(_document);
            Push(command);
        }

        /// <summary>
        ///     Records a command whose effect is already in the document (a stroke that was drawn live and
        ///     packaged by <see cref="RasterEditRecorder" />).
        /// </summary>
        public void Record(IDocumentCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);
            Push(command);
        }

        /// <summary>Reverts the last step. If reverting throws, the step stays on the stack.</summary>
        public void Undo()
        {
            if (!CanUndo) return;

            var command = _undo[^1];
            command.Revert(_document);

            _undo.RemoveAt(_undo.Count - 1);
            _redo.Add(command);
            RaiseChanged();
        }

        /// <summary>Re-applies the last undone step. If applying throws, the step stays on the redo stack.</summary>
        public void Redo()
        {
            if (!CanRedo) return;

            var command = _redo[^1];
            command.Apply(_document);

            _redo.RemoveAt(_redo.Count - 1);
            _undo.Add(command);
            RaiseChanged();
        }

        /// <summary>Marks the current state as saved.</summary>
        public void MarkClean()
        {
            _cleanDepth = _undo.Count;
            RaiseChanged();
        }

        /// <summary>Forgets all steps. The dirty flag keeps its current value.</summary>
        public void Clear()
        {
            var wasDirty = IsDirty;

            DisposeAll(_undo);
            DisposeAll(_redo);

            _cleanDepth = wasDirty ? -1 : 0;
            RaiseChanged();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DisposeAll(_undo);
            DisposeAll(_redo);
        }

        private void Push(IDocumentCommand command)
        {
            if (_redo.Count > 0)
            {
                // The saved state lived on the branch we are about to throw away.
                if (_cleanDepth > _undo.Count) _cleanDepth = -1;

                DisposeAll(_redo);
            }

            _undo.Add(command);

            if (_undo.Count > Limit)
            {
                var oldest = _undo[0];
                _undo.RemoveAt(0);

                if (_cleanDepth >= 0) _cleanDepth--;

                (oldest as IDisposable)?.Dispose();
            }

            RaiseChanged();
        }

        private static void DisposeAll(List<IDocumentCommand> commands)
        {
            foreach (var command in commands)
            {
                (command as IDisposable)?.Dispose();
            }

            commands.Clear();
        }

        private void RaiseChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}