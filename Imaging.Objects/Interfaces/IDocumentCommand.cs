/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Interfaces
 * FILE:        IDocumentCommand.cs
 * PURPOSE:     Undoable edits of a document. A command stores just enough to apply and revert itself:
 *              ids and small records for structure/shape edits, tile patches for pixel edits.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;

namespace Imaging.Objects.Interfaces
{
    /// <summary>
    ///     One undoable step. <see cref="Apply" /> is used for the first execution and for redo.
    ///     Commands that own layers implement <see cref="IDisposable" />; <see cref="DocumentHistory" /> disposes
    ///     them when they can no longer be reached (trimmed, redo branch dropped, history disposed).
    /// </summary>
    public interface IDocumentCommand
    {
        /// <summary>Gets a short human-readable name for menus ("Undo Add layer").</summary>
        string Description { get; }

        /// <summary>Executes (or re-executes) the change.</summary>
        void Apply(Document document);

        /// <summary>Undoes the change.</summary>
        void Revert(Document document);
    }
}