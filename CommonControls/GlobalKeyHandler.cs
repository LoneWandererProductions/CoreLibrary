/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/GlobalKeyHandler.cs
 * PURPOSE:     Attached Control to handle global Keystrokes
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommonControls
{
    /// <summary>
    ///     A static class that provides global key handling functionality to attach key-command mappings to UI elements.
    /// </summary>
    public static class GlobalKeyHandler
    {
        /// <summary>
        ///     DependencyProperty to enable or disable global key handling on a specific UIElement.
        /// </summary>
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached(
                ComCtlResources.GlobalKeyAttach, typeof(bool), typeof(GlobalKeyHandler),
                new PropertyMetadata(false, OnAttachChanged));


        /// <summary>
        ///     DependencyProperty to store a dictionary of key-command pairs for a UIElement.
        /// </summary>
        public static readonly DependencyProperty CommandBindingsProperty =
            DependencyProperty.RegisterAttached(
                ComCtlResources.GlobalKeyCommandBindings, typeof(Dictionary<Key, ICommand>), typeof(GlobalKeyHandler),
                new PropertyMetadata(null));


        /// <summary>
        ///     Skip text controls property
        /// </summary>
        public static readonly DependencyProperty SkipTextControlsProperty =
            DependencyProperty.RegisterAttached(
                ComCtlResources.GlobalKeySkipTextControls, typeof(bool), typeof(GlobalKeyHandler),
                new PropertyMetadata(true));

        /// <summary>
        ///     Sets the skip text controls.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetSkipTextControls(UIElement element, bool value)
        {
            element.SetValue(SkipTextControlsProperty, value);
        }

        /// <summary>
        ///     Gets the skip text controls.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Returns if we skip control.</returns>
        public static bool GetSkipTextControls(UIElement element)
        {
            return (bool)element.GetValue(SkipTextControlsProperty);
        }

        /// <summary>
        ///     Sets the Attach property, enabling or disabling key handling on the specified UIElement.
        ///     When set to true, key events are registered; when false, they are unregistered.
        /// </summary>
        public static void SetAttach(UIElement element, bool value)
        {
            element.SetValue(AttachProperty, value);
        }

        /// <summary>
        ///     Gets the current value of the Attach property for a UIElement.
        /// </summary>
        public static bool GetAttach(UIElement element)
        {
            return (bool)element.GetValue(AttachProperty);
        }

        /// <summary>
        ///     Sets the dictionary of key-command bindings for the specified UIElement.
        ///     This dictionary maps specific keys to ICommand instances, allowing the element to handle
        ///     those keys by executing the associated commands.
        /// </summary>
        public static void SetCommandBindings(UIElement element, Dictionary<Key, ICommand> value)
        {
            element.SetValue(CommandBindingsProperty, value);
        }

        /// <summary>
        ///     Gets the dictionary of key-command bindings associated with the specified UIElement.
        /// </summary>
        public static Dictionary<Key, ICommand> GetCommandBindings(UIElement element)
        {
            return (Dictionary<Key, ICommand>)element.GetValue(CommandBindingsProperty);
        }

        /// <summary>
        ///     Called whenever the Attach property changes. Attaches or detaches key event handling based on the new value.
        /// </summary>
        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement element) return;

            if ((bool)e.NewValue) // If Attach is set to true
                element.PreviewKeyDown += OnPreviewKeyDown; // Attach the key-down handler
            else // If Attach is set to false
                element.PreviewKeyDown -= OnPreviewKeyDown; // Detach the key-down handler
        }

        /// <summary>
        ///     Handles the PreviewKeyDown event on the attached UIElement.
        ///     This method checks if there is a command bound to the pressed key and executes it if allowed.
        /// </summary>
        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return; // Do nothing if the event is already handled

            if (sender is not UIElement element) return;

            // Get the currently focused element using Keyboard.FocusedElement
            var focusedElement = Keyboard.FocusedElement;

            // Check if skipping text controls is enabled
            if (GetSkipTextControls(element) && focusedElement is TextBox or RichTextBox)
                return;

            // Retrieve the dictionary of key-command bindings for this element
            var bindings = GetCommandBindings(element);

            // Check if a command is bound to the pressed key and if it can execute
            if (bindings == null || !bindings.TryGetValue(e.Key, out var command) || !command.CanExecute(null)) return;

            // Execute the command if found and mark the event as handled
            command.Execute(null);
            e.Handled = true;
        }
    }
}