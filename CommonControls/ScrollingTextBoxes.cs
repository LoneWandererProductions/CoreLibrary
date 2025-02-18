/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ScrollingTextBoxes.cs
 * PURPOSE:     Extensions for TextBox and RichTextBox with AutoScrolling
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CommonControls
{
    /// <inheritdoc />
    /// <summary>
    ///     The Extension for the TextBox class with AutoScrolling support.
    /// </summary>
    public sealed class ScrollingTextBoxes : TextBox
    {
        /// <summary>
        ///     DependencyProperty: IsAutoScrolling
        ///     Determines if auto-scrolling is enabled.
        /// </summary>
        public static readonly DependencyProperty IsAutoScrollingProperty =
            DependencyProperty.Register(nameof(IsAutoScrolling), typeof(bool), typeof(ScrollingTextBoxes),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAutoScrollingChanged));

        /// <summary>
        ///     Gets or sets a value indicating whether auto-scrolling is activated.
        /// </summary>
        public bool IsAutoScrolling
        {
            get => (bool)GetValue(IsAutoScrollingProperty);
            set => SetValue(IsAutoScrollingProperty, value);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Raises the initialized event.
        ///     Sets basic attributes.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(OnScrollChanged));
        }

        /// <summary>
        /// Handles scroll changes and applies auto-scroll behavior.
        /// </summary>
        private void OnScrollChanged(object sender, RoutedEventArgs e)
        {
            if (IsAutoScrolling)
            {
                CaretIndex = Text.Length;
                ScrollToEnd();
            }
        }

        /// <summary>
        /// Handles changes to the IsAutoScrolling property.
        /// </summary>
        private static void OnAutoScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (ScrollingTextBoxes)d;
            if ((bool)e.NewValue)
            {
                textBox.CaretIndex = textBox.Text.Length;
                textBox.ScrollToEnd();
            }
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     The Extension for the RichTextBox class with AutoScrolling support.
    /// </summary>
    public sealed class ScrollingRichTextBox : RichTextBox
    {
        /// <summary>
        ///     DependencyProperty: IsAutoScrolling
        ///     Determines if auto-scrolling is enabled.
        /// </summary>
        public static readonly DependencyProperty IsAutoScrollingProperty =
            DependencyProperty.Register(nameof(IsAutoScrolling), typeof(bool), typeof(ScrollingRichTextBox),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAutoScrollingChanged));

        /// <summary>
        ///     Gets or sets a value indicating whether auto-scrolling is activated.
        /// </summary>
        public bool IsAutoScrolling
        {
            get => (bool)GetValue(IsAutoScrollingProperty);
            set => SetValue(IsAutoScrollingProperty, value);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Raises the initialized event.
        ///     Sets basic attributes.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(OnScrollChanged));
        }

        /// <summary>
        /// Handles scroll changes and applies auto-scroll behavior.
        /// </summary>
        private void OnScrollChanged(object sender, RoutedEventArgs e)
        {
            if (IsAutoScrolling)
            {
                _ = (Dispatcher?.BeginInvoke(DispatcherPriority.Background, new Action(ScrollToEnd)));
            }
        }

        /// <summary>
        /// Handles changes to the IsAutoScrolling property.
        /// </summary>
        private static void OnAutoScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var richTextBox = (ScrollingRichTextBox)d;
            if ((bool)e.NewValue)
            {
                richTextBox.ScrollToEnd();
            }
        }
    }
}
