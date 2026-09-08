/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        BusyIndicator.xaml.cs
 * PURPOSE:     Small "ready / working" status dot, shared by SlimViewer and the Compare window
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Common.Images
{
    /// <inheritdoc />
    /// <summary>
    ///     A minimal activity indicator: a steady green dot while idle, a pulsing amber dot while
    ///     work is going on in the background. Replaces the previous static red/green icon-swap,
    ///     which was easy to miss at a glance - the blink is what actually draws the eye.
    /// </summary>
    /// <seealso cref="UserControl" />
    public sealed partial class BusyIndicator : UserControl
    {
        /// <summary>
        ///     Identifies the <see cref="IsBusy" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy), typeof(bool), typeof(BusyIndicator),
            new PropertyMetadata(false, OnIsBusyChanged));

        /// <summary>
        ///     The brush used while idle/ready. Frozen since it's shared across every instance.
        /// </summary>
        private static readonly SolidColorBrush ReadyBrush = CreateFrozenBrush(0x2E, 0xCC, 0x71);

        /// <summary>
        ///     The brush used while busy. Frozen since it's shared across every instance.
        /// </summary>
        private static readonly SolidColorBrush BusyBrush = CreateFrozenBrush(0xE6, 0x7E, 0x22);

        /// <summary>
        ///     The blink animation, built once per instance and simply started/stopped as needed.
        /// </summary>
        private readonly Storyboard _blink;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BusyIndicator" /> class.
        /// </summary>
        public BusyIndicator()
        {
            InitializeComponent();

            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.25,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(animation, Dot);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));

            _blink = new Storyboard();
            _blink.Children.Add(animation);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether background work is currently in progress.
        ///     <c>true</c> shows a pulsing amber dot; <c>false</c> shows a steady green dot.
        /// </summary>
        /// <value>
        ///   <c>true</c> if busy; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        /// <summary>
        ///     Creates a frozen (thread-safe, shareable) solid color brush from RGB bytes.
        /// </summary>
        private static SolidColorBrush CreateFrozenBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        /// <summary>
        ///     Called when the <see cref="IsBusy" /> property changes; starts or stops the blink.
        /// </summary>
        private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not BusyIndicator control)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                control.Dot.Fill = BusyBrush;
                control._blink.Begin(control.Dot, true);
            }
            else
            {
                control._blink.Stop(control.Dot);
                control.Dot.Opacity = 1.0;
                control.Dot.Fill = ReadyBrush;
            }
        }
    }
}
