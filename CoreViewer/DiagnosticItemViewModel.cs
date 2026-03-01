/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreViewer
 * FILE:        CoreViewer/DiagnosticItemViewModel.cs
 * PURPOSE:     Our ViewModel for a single diagnostic item in the list.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Windows.Input;
using CoreBuilder;
using ViewModel;

namespace CoreViewer
{
    /// <summary>
    /// All Info about a single diagnostic item in the list.
    /// </summary>
    public sealed class DiagnosticItemViewModel
    {
        /// <summary>
        /// Gets the diagnostic.
        /// </summary>
        /// <value>
        /// The diagnostic.
        /// </value>
        public Diagnostic Diagnostic { get; }

        /// <summary>
        /// Gets the open file command.
        /// </summary>
        /// <value>
        /// The open file command.
        /// </value>
        public ICommand OpenFileCommand { get; }

        /// <summary>
        /// Gets the ignore command.
        /// </summary>
        /// <value>
        /// The ignore command.
        /// </value>
        public ICommand IgnoreCommand { get; }

        /// <summary>
        /// Gets the fix command.
        /// </summary>
        /// <value>
        /// The fix command.
        /// </value>
        public ICommand? FixCommand { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can fix.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can fix; otherwise, <c>false</c>.
        /// </value>
        public bool CanFix => FixCommand != null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticItemViewModel"/> class.
        /// </summary>
        /// <param name="diagnostic">The diagnostic.</param>
        /// <param name="openFile">The open file.</param>
        /// <param name="ignore">The ignore.</param>
        /// <param name="fix">The fix.</param>
        public DiagnosticItemViewModel(Diagnostic diagnostic,
            Action<Diagnostic> openFile,
            Action<Diagnostic> ignore,
            Action<Diagnostic>? fix = null)
        {
            Diagnostic = diagnostic;
            OpenFileCommand = new RelayCommand(() => openFile(diagnostic));
            IgnoreCommand = new RelayCommand(() => ignore(diagnostic));
            if (fix != null)
                FixCommand = new RelayCommand(() => fix(diagnostic));
        }
    }
}
