/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageGif.cs
 * PURPOSE:     Extends the Image Control and adds Gif Support via SourceGif Property
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Imaging
{
    /// <inheritdoc cref="Image" />
    /// <summary>
    ///     Extension for Image to play e.g. gif Images
    /// </summary>
    /// <seealso cref="Image" />
    public sealed class ImageGif : Image, IDisposable
    {
        /// <summary>
        ///     The frame index property
        /// </summary>
        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register(nameof(FrameIndex), typeof(int), typeof(ImageGif),
                new UIPropertyMetadata(0, ChangingFrameIndex));

        /// <summary>
        ///     The automatic start property
        /// </summary>
        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register(nameof(AutoStart), typeof(bool), typeof(ImageGif),
                new UIPropertyMetadata(false, AutoStartPropertyChanged));

        /// <summary>
        ///     The GIF source property
        /// </summary>
        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register(nameof(GifSource), typeof(string), typeof(ImageGif),
                new UIPropertyMetadata(string.Empty, GifSourcePropertyChanged));

        /// <summary>
        ///     The animation
        /// </summary>
        private Int32Animation _animation;

        /// <summary>
        ///     The image list
        /// </summary>
        private List<ImageSource> _imageList;

        /// <summary>
        ///     The is initialized
        /// </summary>
        private bool _isInitialized;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes the <see cref="ImageGif" /> class.
        /// </summary>
        static ImageGif()
        {
            VisibilityProperty.OverrideMetadata(typeof(ImageGif),
                new FrameworkPropertyMetadata(VisibilityPropertyChanged));
        }

        /// <summary>
        ///     Gets or sets the index of the frame.
        /// </summary>
        public int FrameIndex
        {
            get => (int)GetValue(FrameIndexProperty);
            set => SetValue(FrameIndexProperty, value);
        }

        /// <summary>
        ///     Defines whether the animation starts on its own.
        /// </summary>
        public bool AutoStart
        {
            get => (bool)GetValue(AutoStartProperty);
            set => SetValue(AutoStartProperty, value);
        }

        /// <summary>
        ///     Gets or sets the GIF source.
        /// </summary>
        public string GifSource
        {
            get => (string)GetValue(GifSourceProperty);
            set => SetValue(GifSourceProperty, value);
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            // Check if the image exists
            if (!File.Exists(GifSource))
            {
                // Log or show an error message
                return;
            }

            try
            {
                var info = ImageGifHandler.GetImageInfo(GifSource);

                // Handle possible error
                if (info == null || !info.IsAnimated) return;

                _imageList = ImageGifHandler.LoadGif(GifSource);
                Source = _imageList[0];

                var time = Math.Max(1, info.Frames / 10);
                _animation = new Int32Animation(0, info.Frames - 1,
                        new Duration(new TimeSpan(0, 0, 0, time)))
                { RepeatBehavior = RepeatBehavior.Forever };

                _isInitialized = true;

                if (AutoStart) StartAnimation();
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        /// <summary>
        ///     Visibilities the property changed.
        /// </summary>
        private static void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue == Visibility.Visible)
                ((ImageGif)sender).StartAnimation();
            else
                ((ImageGif)sender).StopAnimation();
        }

        /// <summary>
        ///     Changing the index of the frame.
        /// </summary>
        private static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            if (obj is ImageGif {AutoStart: true} gifImage)
            {
                var newIndex = (int)ev.NewValue;
                if (newIndex >= 0 && newIndex < gifImage._imageList.Count)
                {
                    gifImage.Source = gifImage._imageList[newIndex];
                }
            }
        }

        /// <summary>
        ///     Automatics the start property changed.
        /// </summary>
        private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ImageGif)?.StartAnimation();
        }

        /// <summary>
        ///     GIFs the source property changed.
        /// </summary>
        private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as ImageGif)?.Initialize();
        }

        /// <summary>
        ///     Starts the animation.
        /// </summary>
        private void StartAnimation()
        {
            if (!_isInitialized) Initialize();
            BeginAnimation(FrameIndexProperty, _animation);
        }

        /// <summary>
        ///     Stops the animation.
        /// </summary>
        public void StopAnimation()
        {
            BeginAnimation(FrameIndexProperty, null);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Dispose method for releasing resources.
        /// </summary>
        public void Dispose()
        {
            StopAnimation();
            // Optionally clear image resources
            if (_imageList != null)
            {
                foreach (var img in _imageList)
                {
                    if (img is IDisposable disposable)
                        disposable.Dispose();
                }
                _imageList.Clear();
            }

            _imageList = null;
            _animation = null;
        }
    }
}
