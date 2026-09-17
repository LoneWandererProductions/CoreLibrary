/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Dialogs
 * FILE:        FolderControl.cs
 * PURPOSE:     FolderView Control, drop-in replacement for FolderBrowser, improved and thread-safe
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Common.Dialogs
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// Interaction logic for FolderControl.xaml
    /// </summary>
    public sealed partial class FolderControl
    {
        /// <summary>
        /// The path as it was before the user started editing it, so Escape can cleanly restore it
        /// instead of just hiding the box and leaving whatever half-typed text behind in LookUp.
        /// </summary>
        private string? _pathBeforeEdit;

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public FolderViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderControl"/> class.
        /// </summary>
        public FolderControl()
        {
            InitializeComponent();

            ViewModel = new FolderViewModel();
            DataContext = ViewModel;

            // So keyboard-only users (Tab into the dialog, or it just opened) can start navigating
            // immediately without having to click the tree first.
            Loaded += (_, _) => FolderTree.Focus();

            PreviewKeyDown += FolderControl_PreviewKeyDown;
        }

        /// <summary>
        /// Expose a convenient method to initialize the control with a starting folder.
        /// </summary>
        /// <param name="startFolder">The start folder.</param>
        internal void Initiate(string startFolder)
        {
            ViewModel.Initiate(startFolder);
        }

        /// <summary>
        /// Handles the MouseDown event of the PathDisplay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void PathDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _pathBeforeEdit = ViewModel.Paths;
            ViewModel.LookUp = ViewModel.Paths;

            // Hide the entire border, not just the text block inside it
            PathDisplayBorder.Visibility = Visibility.Collapsed;
            PathEntry.Visibility = Visibility.Visible;

            e.Handled = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                PathEntry.Focus();
                Keyboard.Focus(PathEntry);
                PathEntry.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Handles the LostFocus event of the PathEntry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PathEntry_LostFocus(object sender, RoutedEventArgs e)
        {
            PathEntry.Visibility = Visibility.Collapsed;
            // Show the border again
            PathDisplayBorder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the KeyDown event of the PathEntry control to trigger navigation on Enter key press,
        /// or revert the edit on Escape.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void PathEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ViewModel.GoCommand.CanExecute(null))
                {
                    ViewModel.GoCommand.Execute(null);
                }

                PathEntry.Visibility = Visibility.Collapsed;
                // Show the border again
                PathDisplayBorder.Visibility = Visibility.Visible;

                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                ViewModel.LookUp = _pathBeforeEdit ?? ViewModel.Paths;

                PathEntry.Visibility = Visibility.Collapsed;
                // Show the border again
                PathDisplayBorder.Visibility = Visibility.Visible;

                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles folder-navigation shortcuts: Backspace to go up a level, Alt+Left/Alt+Right for
        /// Back/Forward history - the same conventions Explorer uses. Skipped entirely while the
        /// path textbox is visible/focused so it doesn't hijack normal text editing (deleting a
        /// character with Backspace, etc.).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void FolderControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (PathEntry.Visibility == Visibility.Visible) return;

            switch (e.Key)
            {
                case Key.Back when ViewModel.UpCommand.CanExecute(null):
                    ViewModel.UpCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Left when Keyboard.Modifiers == ModifierKeys.Alt &&
                                    ViewModel.BackCommand.CanExecute(null):
                    ViewModel.BackCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Right when Keyboard.Modifiers == ModifierKeys.Alt &&
                                     ViewModel.ForwardCommand.CanExecute(null):
                    ViewModel.ForwardCommand.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Up when Keyboard.Modifiers == ModifierKeys.Alt &&
                                  ViewModel.UpCommand.CanExecute(null):
                    // Alt+Up is the other very common "go up" convention (used alongside Backspace).
                    ViewModel.UpCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
