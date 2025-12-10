/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Imaging
* FILE:        ImageGifThreaded.cs
* PURPOSE:     Extends Image Control to play GIFs asynchronously on its own thread
* PROGRAMMER:  Peter Geinitz (Wayfarer)
*/

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Imaging
{
    /// <summary>
    /// Image control capable of playing animated GIFs in a self-contained way.
    /// Switching GIF → non-GIF will always clear old frames and stop timers.
    /// </summary>
    public sealed class ImageGif : Image, IDisposable
    {
        // Dependency properties

        /// <summary>
        /// The GIF source property
        /// </summary>
        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register(
                nameof(GifSource),
                typeof(string),
                typeof(ImageGif),
                new PropertyMetadata(string.Empty, OnGifSourceChanged));

        /// <summary>
        /// The automatic start property
        /// </summary>
        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register(
                nameof(AutoStart),
                typeof(bool),
                typeof(ImageGif),
                new PropertyMetadata(false));

        /// <summary>
        /// The decoder
        /// </summary>
        private GifBitmapDecoder? _decoder;

        /// <summary>
        /// The timer
        /// </summary>
        private DispatcherTimer? _dispatcherTimer;

        /// <summary>
        /// The frame index
        /// </summary>
        private int _frameIndex;

        /// <summary>
        /// The is disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Gets or sets the GIF source.
        /// </summary>
        /// <value>
        /// The GIF source.
        /// </value>
        public string GifSource
        {
            get => (string)GetValue(GifSourceProperty);
            set => SetValue(GifSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [automatic start].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic start]; otherwise, <c>false</c>.
        /// </value>
        public bool AutoStart
        {
            get => (bool)GetValue(AutoStartProperty);
            set => SetValue(AutoStartProperty, value);
        }

        /// <summary>
        /// DP callback: a new GIF path was assigned.
        /// This method completely resets GIF state and loads the new file.
        /// </summary>
        private static void OnGifSourceChanged(
            DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (sender is ImageGif gif)
                gif.LoadGif(e.NewValue as string);
        }

        /// <summary>
        /// Full GIF load routine:
        /// - stops old animation
        /// - clears previous frames
        /// - loads new GIF decoder
        /// - optionally starts animation
        /// </summary>
        /// <param name="path">The path.</param>
        private void LoadGif(string? path)
        {
            Source = null;
            _decoder = null;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            try
            {
                var decoder = new GifBitmapDecoder(
                    new Uri(path, UriKind.Absolute),
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

                if (decoder.Frames.Count == 0)
                    return;

                // Freeze frames for safety & performance
                foreach (var f in decoder.Frames)
                {
                    if (f.CanFreeze) f.Freeze();
                }

                _decoder = decoder;
                _frameIndex = 0;

                Source = _decoder.Frames[0];

                if (AutoStart)
                    StartGif();
            }
            catch
            {
                Source = null;
                _decoder = null;
            }
        }

        /// <summary>
        /// Starts the animation timer.
        /// A simple timer is used to avoid async task races and frame lag.
        /// </summary>
        public void StartGif()
        {
            if (_decoder == null || _decoder.Frames.Count <= 1)
                return;

            const int delay = 80;

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(delay);
            _dispatcherTimer.Tick += (s, e) =>
            {
                if (_decoder == null)
                    return;

                _frameIndex = (_frameIndex + 1) % _decoder.Frames.Count;

                Source = _decoder.Frames[_frameIndex];
            };
            _dispatcherTimer.Start();
        }

        /// <summary>
        /// Stops animation and clears timers/decoder safely.
        /// </summary>
        public void StopGif()
        {
            StopGifInternal();
            // Important: clear the visual to guarantee no ghosting
            Source = null;
        }

        /// <summary>
        /// Stops the GIF internal.
        /// </summary>
        private void StopGifInternal()
        {
            _dispatcherTimer?.Stop();
            _dispatcherTimer = null;

            if (!string.IsNullOrEmpty(GifSource))
            {
                GifSource = string.Empty;
                Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            }

            _decoder = null;
            _frameIndex = 0;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> event, using the specified information as part of the eventual event data.
        /// </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            // this is intentionally left empty: no resize side-effects needed
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
                StopGif();

            _isDisposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ImageGif"/> class.
        /// </summary>
        ~ImageGif()
        {
            Dispose(false);
        }
    }
}