/*
* COPYRIGHT:   Your Company / Project Name
* PROJECT:     CommonControls
* FILE:        InspectorGrid.cs
* PURPOSE:     Reflection-based property editor control for WPF.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
*/

// ReSharper disable UnusedType.Global

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace CommonControls
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     A reusable reflection-based property grid control.
    ///     Automatically generates editors for properties of the bound <see cref="P:CommonControls.InspectorGrid.SelectedObject" />.
    /// </summary>
    public partial class InspectorGrid
    {
        /// <summary>
        /// Gets the content grid.
        /// Only needed for unit tests.
        /// </summary>
        /// <value>
        /// The content grid.
        /// </value>
        internal Grid ContentGrid => PartContent;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CommonControls.InspectorGrid" /> class.
        /// </summary>
        public InspectorGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Dependency property for <see cref="SelectedObject"/>.
        /// </summary>
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject),
                typeof(object),
                typeof(InspectorGrid),
                new PropertyMetadata(null, OnSelectedObjectChanged));

        /// <summary>
        ///     The object whose properties will be displayed and edited in the grid.
        /// </summary>
        public object? SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        /// <summary>
        ///     Called when the <see cref="SelectedObject"/> changes.
        ///     Rebuilds the property grid dynamically.
        /// </summary>
        private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InspectorGrid grid)
                grid.BuildGrid();
        }

        /// <summary>
        ///     Ensures that the grid has two columns:
        ///     one for labels, one for editors.
        /// </summary>
        private void EnsureColumns()
        {
            if (PartContent.ColumnDefinitions.Count < 2)
            {
                PartContent.ColumnDefinitions.Clear();
                PartContent.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                PartContent.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
        }

        /// <summary>
        ///     Builds the property grid dynamically based on the <see cref="SelectedObject"/>.
        /// </summary>
        /// <summary>
        ///     Builds the property grid dynamically based on the <see cref="SelectedObject"/>.
        /// </summary>
        private void BuildGrid()
        {
            PartContent.Children.Clear();
            PartContent.RowDefinitions.Clear();

            if (SelectedObject == null) return;

            EnsureColumns();

            var props = SelectedObject.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead) // include init-only and read-only
                .OrderBy(p => p.Name);

            var row = 0;
            foreach (var prop in props)
            {
                PartContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Label (with DisplayName/DataAnnotations support)
                var displayName = prop.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                                  ?? prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>()
                                      ?.Name
                                  ?? prop.Name;

                var label = new TextBlock
                {
                    Text = displayName,
                    Margin = new Thickness(4, 2, 4, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);
                PartContent.Children.Add(label);

                // Determine property characteristics
                var isInitOnly = !prop.CanWrite;
                var isReadOnlyAttr = prop.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly ?? false;
                var isCollection = typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType)
                                   && prop.PropertyType != typeof(string);

                FrameworkElement editor;

                if (isCollection)
                {
                    var value = prop.GetValue(SelectedObject) as System.Collections.IEnumerable;
                    var count = value?.Cast<object>().Count() ?? 0;
                    var typeName = prop.PropertyType.IsGenericType
                        ? prop.PropertyType.GetGenericArguments().First().Name
                        : prop.PropertyType.Name;

                    editor = new TextBlock
                    {
                        Text = $"Collection of {typeName}, Count = {count}",
                        Margin = new Thickness(2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }
                else if (isInitOnly || isReadOnlyAttr)
                {
                    // Display value as read-only text
                    editor = new TextBlock
                    {
                        Text = prop.GetValue(SelectedObject)?.ToString() ?? "(null)",
                        Margin = new Thickness(2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                }
                else
                {
                    // Editable
                    editor = CreateEditor(prop, SelectedObject);
                }

                Grid.SetRow(editor, row);
                Grid.SetColumn(editor, 1);
                PartContent.Children.Add(editor);

                row++;
            }
        }

        /// <summary>
        ///     Creates an editor UI element for a given property.
        ///     Supports bool, enums, strings, and numeric types by default.
        /// </summary>
        /// <param name="prop">The property to generate an editor for.</param>
        /// <param name="target">The target object containing the property.</param>
        /// <returns>A WPF control suitable for editing the property.</returns>
        private FrameworkElement CreateEditor(PropertyInfo prop, object target)
        {
            var type = prop.PropertyType;

            // Boolean => CheckBox
            if (type == typeof(bool))
            {
                var checkBox = new CheckBox { IsChecked = (bool?)prop.GetValue(target), Margin = new Thickness(2) };
                checkBox.Checked += (_, _) =>
                {
                    if (prop.CanWrite) prop.SetValue(target, true);
                };
                checkBox.Unchecked += (_, _) =>
                {
                    if (prop.CanWrite) prop.SetValue(target, false);
                };
                return checkBox;
            }

            // Enum => ComboBox
            if (type.IsEnum)
            {
                var comboBox = new ComboBox
                {
                    ItemsSource = Enum.GetValues(type),
                    SelectedItem = prop.GetValue(target),
                    Margin = new Thickness(2)
                };
                comboBox.SelectionChanged += (_, _) =>
                {
                    if (!prop.CanWrite)
                    {
                        return;
                    }

                    try
                    {
                        prop.SetValue(target, comboBox.SelectedItem);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to set enum property '{prop.Name}': {ex.Message}");
                    }
                };
                return comboBox;
            }

            // Default => TextBox (string or numeric types)
            var textBox = new TextBox
            {
                Text = prop.GetValue(target)?.ToString() ?? string.Empty, Margin = new Thickness(2)
            };

            textBox.LostFocus += (_, _) =>
            {
                if (!prop.CanWrite) return; // do not set init-only or read-only

                try
                {
                    var converted = Convert.ChangeType(textBox.Text, type);
                    prop.SetValue(target, converted);
                }
                catch
                {
                    // invalid input ignored
                }
            };

            return textBox;
        }
    }
}