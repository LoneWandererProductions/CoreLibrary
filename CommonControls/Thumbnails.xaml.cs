using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CommonControls
{
    public partial class Thumbnails : UserControl
    {
        // ObservableCollection automatically notifies the UI when items are added/removed
        public ObservableCollection<ThumbnailItem> ThumbCollection { get; set; } = new();

        public static readonly DependencyProperty ThumbCellSizeProperty =
            DependencyProperty.Register(nameof(ThumbCellSize), typeof(double), typeof(Thumbnails), new PropertyMetadata(100.0));

        public double ThumbCellSize
        {
            get => (double)GetValue(ThumbCellSizeProperty);
            set => SetValue(ThumbCellSizeProperty, value);
        }

        public Thumbnails()
        {
            InitializeComponent();
            // Set the DataContext to this control so XAML can bind to ThumbCollection
            DataContext = this;
        }

        public async Task LoadImages(Dictionary<int, string> sourceList)
        {
            ThumbCollection.Clear();

            // Using your optimized loading logic, but slightly cleaner
            using (var semaphore = new SemaphoreSlim(4))
            {
                var tasks = sourceList.Select(async kvp =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var img = await LoadImageAsync(kvp.Value);
                        if (img != null)
                        {
                            // Create the Data Item
                            var item = new ThumbnailItem
                            {
                                Id = kvp.Key,
                                FilePath = kvp.Value,
                                ImageSource = img
                            };

                            // Thread-safe add to collection
                            Application.Current.Dispatcher.Invoke(() => ThumbCollection.Add(item));
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
            }
        }

        private async Task<BitmapSource> LoadImageAsync(string path)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(path)) return null;
                    var bi = new BitmapImage();
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        bi.BeginInit();
                        bi.StreamSource = fs;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.DecodePixelWidth = (int)ThumbCellSize; // Decode small to save RAM
                        bi.EndInit();
                        bi.Freeze(); // Vital for cross-thread
                    }
                    return bi;
                }
                catch { return null; }
            });
        }
    }
}
