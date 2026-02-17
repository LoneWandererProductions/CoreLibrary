using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace CommonControls
{
    public class ThumbnailItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private BitmapSource _imageSource;

        public int Id { get; set; }
        public string FilePath { get; set; }

        // The actual image to display
        public BitmapSource ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(); }
        }

        // Two-way binding for the CheckBox
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
