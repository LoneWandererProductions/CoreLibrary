/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Imaging.Gifs
* FILE:        ImageGifThreaded.cs
* PURPOSE:     Extends Image Control to play GIFs asynchronously on its own thread
* PROGRAMMER:  Peter Geinitz (Wayfarer)
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Imaging.Gifs
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
        /// The frames
        /// </summary>
        private List<BitmapSource>? _frames;

        private ImageGifInfo? _metadata;

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
        /// The loader CTS
        /// </summary>
        private CancellationTokenSource _loaderCts;

        /// <summary>
        /// Occurs when [image loaded].
        /// </summary>
        public event EventHandler? ImageLoaded;

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
        private static void OnGifSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ImageGif gif)
            {
                gif.LoadGifAsync(e.NewValue as string);
            }
        }

        /// <summary>
        /// Full GIF load routine:
        /// - stops old animation
        /// - clears previous frames
        /// - loads new GIF decoder
        /// - optionally starts animation
        /// </summary>
        /// <param name="path">The path.</param>
        private async void LoadGifAsync(string? path)
        {
            _loaderCts?.Cancel();
            _loaderCts = new CancellationTokenSource();
            var token = _loaderCts.Token;

            ResetInternalState();

            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                // Fast path for non-GIFs: just load as BitmapImage and skip all the decoding/metadata overhead
                if (!path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    Source = ImageStreamMedia.GetBitmapImage(path);
                    ImageLoaded?.Invoke(this, EventArgs.Empty);
                    return;
                }

                // 1. Load metadata (so delays actually work)
                _metadata = ImageGifMetadataExtractor.ExtractGifMetadata(path);

                // 2. Decode frames (background thread)
                var rawFrames = await ImageGifHandler.SplitGifAsync(path);

                if (token.IsCancellationRequested)
                    return;

                // 3. Convert to BitmapSource (still off UI thread)
                _frames = rawFrames.Select(b =>
                {
                    var bi = b.BitmapToSource();

                    if (bi.CanFreeze)
                        bi.Freeze();

                    b.Dispose();
                    return bi;
                }).Cast<BitmapSource>().ToList();

                if (token.IsCancellationRequested || _frames.Count == 0)
                    return;

                // 4. UI update (ONLY this part touches WPF)
                await Dispatcher.InvokeAsync(() =>
                {
                    if (token.IsCancellationRequested) return;

                    if (_frames == null || _frames.Count == 0)
                        return;

                    _frameIndex = 0;
                    Source = _frames[_frameIndex];

                    // 🔥 CRITICAL: notify ImageZoom that size is now valid
                    ImageLoaded?.Invoke(this, EventArgs.Empty);

                    if (AutoStart)
                        StartGif();
                }, DispatcherPriority.Render);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"GIF Load Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts the animation timer.
        /// A simple timer is used to avoid async task races and frame lag.
        /// </summary>
        public void StartGif()
        {
            if (_frames == null || _frames.Count <= 1) return;

            if (_dispatcherTimer == null)
            {
                _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Render);
                _dispatcherTimer.Tick += Timer_Tick;
            }

            _dispatcherTimer.Stop();
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(GetDelayForFrame(0));
            _dispatcherTimer.Start();
        }

        /// <summary>
        /// Stops animation and clears timers/decoder safely.
        /// </summary>
        public void StopGif()
        {
            ResetInternalState();
            GifSource = string.Empty; // Now safe to clear
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
        /// Clears frames and timers without clearing the Dependency Property to avoid recursion.
        /// </summary>
        private void ResetInternalState()
        {
            _dispatcherTimer?.Stop();
            _dispatcherTimer = null;
            _frames = null;
            _metadata = null;
            _frameIndex = 0;
            Source = null;
        }

        /// <summary>
        /// Timers the tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_frames == null || _frames.Count == 0 || _dispatcherTimer == null) return;

            // Move to next frame
            _frameIndex = (_frameIndex + 1) % _frames.Count;
            Source = _frames[_frameIndex];

            // Update timer interval for the NEXT frame to support variable speed GIFs
            var nextDelay = GetDelayForFrame(_frameIndex);

            // Optimization: Only update interval if it actually changed
            if (_dispatcherTimer.Interval.TotalMilliseconds != nextDelay)
            {
                _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(nextDelay);
            }
        }

        /// <summary>
        /// Helper to get delay from metadata, converted from GIF units (1/100s) to Milliseconds.
        /// </summary>
        /// <param name="index">The frame index.</param>
        /// <returns>Delay in milliseconds.</returns>
        private double GetDelayForFrame(int index)
        {
            if (_metadata != null && _metadata.Frames.Count > index)
            {
                // GIF units are 1/100 of a second. Multiply by 10 to get milliseconds.
                var delay = _metadata.Frames[index].DelayTime * 10;

                // Industry Standard: Delays of 0ms or < 20ms are forced to 100ms
                // by most renderers to prevent CPU spikes and "way too fast" playback.
                return delay < 20 ? 100 : delay;
            }

            // Default fallback if metadata is missing
            return 80;
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
