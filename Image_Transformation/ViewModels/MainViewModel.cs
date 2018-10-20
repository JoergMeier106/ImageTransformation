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
        private string _fileFormat;
        private ImageSource _image;
        private int _imageHeight;
        private bool _imageOpen;
        private int _imageWidth;
        private int _layerCount;
        private bool _layerSliderEnabled;
        private int _scaleSx;
        private int _scaleSy;
        private int _shearBx;
        private int _shearBy;
        private int _shiftDx;
        private int _shiftDy;
        private bool _sliderEnabled;

        public MainViewModel(IBitmapCreatorBuilder bitmapCreatorBuilder)
        {
            _bitmapCreatorBuilder = bitmapCreatorBuilder;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double Brightness
        {
            get
            {
                return _bitmapCreatorBuilder.Brightness;
            }
            set
            {
                _bitmapCreatorBuilder.SetBrightness(value);
                UpdateImage();
                RaisePropertyChanged(nameof(Brightness));
            }
        }

        public int CurrentLayer
        {
            get
            {
                return _bitmapCreatorBuilder.Layer;
            }
            set
            {
                _bitmapCreatorBuilder.SetLayer(value);
                UpdateImage();
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
                return _layerSliderEnabled;
            }
            set
            {
                _layerSliderEnabled = value;
                RaisePropertyChanged(nameof(LayerSliderEnabled));
            }
        }

        public ICommand OpenImage
        {
            get
            {
                return new RelayCommand((args) =>
                {
                    _imageOpen = false;
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = " RAW Files (*.raw)|*.raw"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        Resetvalues();

                        _bitmapCreatorBuilder.SetPath(openFileDialog.FileName);
                        ShowImage();
                    }
                    _imageOpen = true;
                });
            }
        }

        public double RotationAlpha
        {
            get
            {
                return _bitmapCreatorBuilder.Alpha;
            }
            set
            {
                _bitmapCreatorBuilder.Rotate(value);
                UpdateImage();
                RaisePropertyChanged(nameof(RotationAlpha));
            }
        }

        public int ScaleSx
        {
            get
            {
                return _scaleSx;
            }
            set
            {
                _scaleSx = value;
                _bitmapCreatorBuilder.Scale(_scaleSx, _scaleSy);
                UpdateImage();
                RaisePropertyChanged(nameof(ScaleSx));
            }
        }

        public int ScaleSy
        {
            get
            {
                return _scaleSy;
            }
            set
            {
                _scaleSy = value;
                _bitmapCreatorBuilder.Scale(_scaleSx, _scaleSy);
                UpdateImage();
                RaisePropertyChanged(nameof(ScaleSy));
            }
        }

        public int ShearBx
        {
            get
            {
                return _shearBx;
            }
            set
            {
                _shearBx = value;
                _bitmapCreatorBuilder.Shear(_shearBx, _shearBy);
                UpdateImage();
                RaisePropertyChanged(nameof(ShearBx));
            }
        }

        public int ShearBy
        {
            get
            {
                return _shearBy;
            }
            set
            {
                _shearBy = value;
                _bitmapCreatorBuilder.Shear(_shearBx, _shearBy);
                UpdateImage();
                RaisePropertyChanged(nameof(ShearBy));
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
                UpdateImage();
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
                UpdateImage();
                RaisePropertyChanged(nameof(ShiftDy));
            }
        }

        public bool SliderEnabled
        {
            get
            {
                return _sliderEnabled;
            }
            set
            {
                _sliderEnabled = value;
                RaisePropertyChanged(nameof(SliderEnabled));
            }
        }

        private void RaisePropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        private void Resetvalues()
        {
            Brightness = 0;
            CurrentLayer = 0;
            ShiftDx = 0;
            ShiftDy = 0;
            ShearBx = 0;
            ShearBy = 0;
            RotationAlpha = 0;
            ScaleSx = 1;
            ScaleSy = 1;
        }

        private void ShowImage()
        {
            FileFormat = Path.GetExtension(_bitmapCreatorBuilder.Path);

            var bitmapCreator = _bitmapCreatorBuilder.Build();
            Image = bitmapCreator.GetImage();
            ImageHeight = (int)Image.Height;
            ImageWidth = (int)Image.Width;
            LayerCount = bitmapCreator.LayerCount;
            LayerSliderEnabled = LayerCount > 1;
            SliderEnabled = true;
        }

        private void UpdateImage()
        {
            if (_imageOpen)
            {
                ShowImage();
            }
        }
    }
}