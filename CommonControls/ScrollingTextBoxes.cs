/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/PathObject.cs
 * PURPOSE:     Extensions for TextBox and RichTextBox
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CommonControls
{
    /// <inheritdoc />
    /// <summary>
    ///     The Extension for the TextBox class.
    /// </summary>
    public sealed class ScrollingTextBoxes : TextBox
    {
        /// <summary>
        ///     DependencyProperty: Scrolling
        ///     The is auto scrolling (readonly). Value: DependencyProperty.Register IsAutoScrolling
        /// </summary>
        public static readonly DependencyProperty IsAutoScrolling = DependencyProperty.Register(nameof(IsAutoScrolling),
            typeof(bool),
            typeof(ScrollingTextBoxes), null);

        /// <summary>
        ///     Gets or sets a value indicating whether AutoScrolling is activated
        /// </summary>
        public bool AutoScrolling
        {
            get => (bool)GetValue(IsAutoScrolling);
            set => SetValue(IsAutoScrolling, value);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Raises the initialized event.
        ///     Set basic Attributes
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Raises the text changed event.
        ///     Change the standard behavior to scrolling down
        /// </summary>
        /// <param name="e">The text changed event arguments.</param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (!AutoScrolling)
            {
                return;
            }

            base.OnTextChanged(e);
            CaretIndex = Text.Length;
            ScrollToEnd();
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     The Extension for the TextBox class.
    /// </summary>
    public sealed class ScrollingRichTextBox : RichTextBox
    {
        /// <summary>
        ///     DependencyProperty: Scrolling
        ///     The is auto scrolling (readonly). Value: DependencyProperty.Register IsAutoScrolling
        /// </summary>
        public static readonly DependencyProperty IsAutoScrolling = DependencyProperty.Register(nameof(IsAutoScrolling),
            typeof(bool),
            typeof(ScrollingTextBoxes), null);

        /// <summary>
        ///     Gets or sets a value indicating whether AutoScrolling is activated
        /// </summary>
        public bool AutoScrolling
        {
            get => (bool)GetValue(IsAutoScrolling);
            set => SetValue(IsAutoScrolling, value);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Raises the initialized event.
        ///     Set basic Attributes
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Raises the text changed event.
        ///     Change the standard behavior to scrolling down
        /// </summary>
        /// <param name="e">The text changed event arguments.</param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (!AutoScrolling)
            {
                return;
            }

            base.OnTextChanged(e);
            ScrollToEnd();
        }
    }
}
