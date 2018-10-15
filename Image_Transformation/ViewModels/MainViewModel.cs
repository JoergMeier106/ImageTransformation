using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Image_Transformation.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private int _currentLayer;
        private string _fileFormat;
        private BitmapImage _image;
        private bool _isRawImage;
        private int _layerCount;
        private readonly IImageLoaderBuilder _imageLoaderBuilder;

        public event PropertyChangedEventHandler PropertyChanged;

        public int CurrentLayer
        {
            get
            {
                return _currentLayer;
            }
            set
            {
                _currentLayer = value;
                RaisePropertyChanged(nameof(CurrentLayer));
            }
        }

        public int ImageHeight
        {
            get
            {
                return _imageHeight;
            }
            set
            {
                _imageHeight = value;
                RaisePropertyChanged(nameof(ImageHeight));
            }
        }

        public int ImageWidth
        {
            get
            {
                return _imageWidth;
            }
            set
            {
                _imageWidth = value;
                RaisePropertyChanged(nameof(ImageWidth));
            }
        }

        public string FileFormat
        {
            get
            {
                return _fileFormat;
            }
            set
            {
                _fileFormat = value;
                RaisePropertyChanged(nameof(FileFormat));
            }
        }

        public BitmapImage Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                RaisePropertyChanged(nameof(Image));
            }
        }

        public bool IsRawImage
        {
            get
            {
                return _isRawImage;
            }
            set
            {
                _isRawImage = value;
                RaisePropertyChanged(nameof(IsRawImage));
            }
        }

        private IImageLoader _imageLoader;
        private int _imageHeight;
        private int _imageWidth;

        public int LayerCount
        {
            get
            {
                return _layerCount;
            }
            set
            {
                _layerCount = value;
                RaisePropertyChanged(nameof(LayerCount));
            }
        }

        public ICommand OpenImage
        {
            get
            {
                return new RelayCommand((args) =>
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = " JPG Files (*.jpg)|*.jpg|*.jpeg| PNG Files (*.png)|*.png| JPEG Files (*.jpeg)| RAW Files (*.raw)|*.raw"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        string fileName = openFileDialog.FileName;
                        FileFormat = Path.GetExtension(fileName);

                        IsRawImage = FileFormat.ToLower() == "raw";

                        _imageLoader = _imageLoaderBuilder.SetPath(fileName)
                                                          .SetLayer(CurrentLayer)
                                                          .Build();
                        Image = _imageLoader.GetImage();
                        ImageHeight = (int)Image.Height;
                        ImageWidth = (int)Image.Width;
                    }
                });
            }
        }

        public MainViewModel(IImageLoaderBuilder imageLoaderBuilder)
        {
            _imageLoaderBuilder = imageLoaderBuilder;
        }

        private void RaisePropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}