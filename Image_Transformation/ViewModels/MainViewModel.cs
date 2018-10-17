using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;

namespace Image_Transformation.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IBitmapCreatorBuilder _bitmapCreatorBuilder;
        private string _currentFileName;
        private string _fileFormat;
        private ImageSource _image;
        private int _imageHeight;
        private int _imageWidth;
        private bool _isRawImage;
        private int _layerCount;
        private int _shiftDx;
        private int _shiftDy;

        public MainViewModel(IBitmapCreatorBuilder bitmapCreatorBuilder)
        {
            _bitmapCreatorBuilder = bitmapCreatorBuilder;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int CurrentLayer
        {
            get
            {
                return _bitmapCreatorBuilder.Layer;
            }
            set
            {
                _bitmapCreatorBuilder.SetLayer(value);
                ShowImage();
                RaisePropertyChanged(nameof(CurrentLayer));
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

        public ImageSource Image
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

        public bool LayerSliderEnabled
        {
            get
            {
                return _isRawImage;
            }
            set
            {
                _isRawImage = value;
                RaisePropertyChanged(nameof(LayerSliderEnabled));
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
                        Filter = " RAW Files (*.raw)|*.raw"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        _currentFileName = openFileDialog.FileName;
                        ShowImage();
                    }
                });
            }
        }

        public int ShiftDx
        {
            get
            {
                return _shiftDx;
            }
            set
            {
                _shiftDx = value;
                _bitmapCreatorBuilder.Shift(_shiftDx, _shiftDy);
                ShowImage();
                RaisePropertyChanged(nameof(ShiftDx));
            }
        }

        public int ShiftDy
        {
            get
            {
                return _shiftDy;
            }
            set
            {
                _shiftDy = value;
                _bitmapCreatorBuilder.Shift(_shiftDx, _shiftDy);
                ShowImage();
                RaisePropertyChanged(nameof(ShiftDy));
            }
        }

        private void RaisePropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        private void ShowImage()
        {
            FileFormat = Path.GetExtension(_currentFileName);

            var bitmapCreator = _bitmapCreatorBuilder.SetPath(_currentFileName)
                                              .Build();
            Image = bitmapCreator.GetImage();
            ImageHeight = (int)Image.Height;
            ImageWidth = (int)Image.Width;
            LayerCount = bitmapCreator.LayerCount;
            LayerSliderEnabled = FileFormat.ToLower() == ".raw" && LayerCount > 1;
        }
    }
}