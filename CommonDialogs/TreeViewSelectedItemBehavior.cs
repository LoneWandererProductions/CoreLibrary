/* 
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/TreeViewSelectedItemBehavior.cs
 * PURPOSE:     Provides an attached property to bind TreeView.SelectedItem to a ViewModel
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Windows;
using System.Windows.Controls;

namespace CommonDialogs
{
    /// <summary>
    /// Provides an attached property that enables binding the <see cref="TreeView.SelectedItem"/> to a property in a ViewModel.
    /// Standard TreeView does not allow direct binding of SelectedItem, so this behavior solves that limitation.
    /// </summary>
    public static class TreeViewSelectedItemBehavior
    {
        /// <summary>
        /// Identifies the <c>SelectedItem</c> attached dependency property.
        /// This property can be bound to a property in the ViewModel for two-way synchronization.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItem",
                typeof(object),
                typeof(TreeViewSelectedItemBehavior),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedItemChanged));

        /// <summary>
        /// Sets the value of the <c>SelectedItem</c> attached property on the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="element">The object on which to set the property (must be a TreeView).</param>
        /// <param name="value">The value to set as the selected item.</param>
        public static void SetSelectedItem(DependencyObject element, object value)
        {
            element.SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// Gets the value of the <c>SelectedItem</c> attached property from the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="element">The object from which to retrieve the property.</param>
        /// <returns>The currently selected item.</returns>
        public static object GetSelectedItem(DependencyObject element)
        {
            return element.GetValue(SelectedItemProperty);
        }

        /// <summary>
        /// Called when the <c>SelectedItem</c> attached property is changed.
        /// Subscribes to the TreeView.SelectedItemChanged event to propagate changes back to the attached property.
        /// </summary>
        /// <param name="d">The dependency object on which the property changed (expected to be a TreeView).</param>
        /// <param name="e">The event arguments containing old and new values.</param>
        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeView treeView)
            {
                // Ensure we do not double-subscribe
                treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
                treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
            }
        }

        /// <summary>
        /// Handles the TreeView.SelectedItemChanged event.
        /// Updates the attached property with the newly selected item.
        /// </summary>
        /// <param name="sender">The TreeView that raised the event.</param>
        /// <param name="e">Event arguments containing old and new selected items.</param>
        private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView treeView)
            {
                // Update the attached property so any bound ViewModel property reflects the change
                SetSelectedItem(treeView, e.NewValue);
            }
        }
    }
}
