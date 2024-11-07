using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CommonControls
{
    public static class GlobalKeyHandler
    {
        // DependencyProperty to enable or disable key handling
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached(
                "Attach", typeof(bool), typeof(GlobalKeyHandler),
                new PropertyMetadata(false, OnAttachChanged));

        // DependencyProperty to store custom key bindings
        public static readonly DependencyProperty KeyBindingsProperty =
            DependencyProperty.RegisterAttached(
                "KeyBindings", typeof(Dictionary<Key, ICommand>), typeof(GlobalKeyHandler),
                new PropertyMetadata(null));

        // Getter and Setter for Attach property
        public static void SetAttach(UIElement element, bool value)
        {
            element.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(UIElement element)
        {
            return (bool)element.GetValue(AttachProperty);
        }

        // Getter and Setter for KeyBindings property
        public static void SetKeyBindings(UIElement element, Dictionary<Key, ICommand> value)
        {
            element.SetValue(KeyBindingsProperty, value);
        }

        public static Dictionary<Key, ICommand> GetKeyBindings(UIElement element)
        {
            return (Dictionary<Key, ICommand>)element.GetValue(KeyBindingsProperty);
        }

        // When the Attach property changes, we subscribe or unsubscribe from the PreviewKeyDown event
        private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as UIElement;
            if (element == null) return;

            if ((bool)e.NewValue)
            {
                element.PreviewKeyDown += OnPreviewKeyDown;
            }
            else
            {
                element.PreviewKeyDown -= OnPreviewKeyDown;
            }
        }

        // This method processes the PreviewKeyDown event
        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Handled) return;

            var element = sender as UIElement;
            if (element == null) return;

            var keyBindings = GetKeyBindings(element);
            if (keyBindings == null) return;

            // Check if the pressed key has a command assigned to it
            if (keyBindings.ContainsKey(e.Key))
            {
                var command = keyBindings[e.Key];
                if (command.CanExecute(null))
                {
                    command.Execute(null);
                    e.Handled = true;  // Mark event as handled
                }
            }
        }
    }
}
