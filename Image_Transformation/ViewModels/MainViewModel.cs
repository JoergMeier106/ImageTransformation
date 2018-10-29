using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Image_Transformation.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IBitmapBuilder _bitmapBuilder;
        private string _fileFormat;
        private WriteableBitmap _image;
        private int _imageHeight;
        private bool _imageIsOpen;
        private int _imageWidth;
        private int _layerCount;
        private bool _layerSliderEnabled;
        private Quadrilateral _markerQuadrilateral;
        private bool _projectEnabled;
        private double _scaleSx;
        private double _scaleSy;
        private int _shearBx;
        private int _shearBy;
        private int _shiftDx;
        private int _shiftDy;
        private ObservableCollection<Point> _quadrilateralPoints;

        public MainViewModel(IBitmapBuilder bitmapCreatorBuilder)
        {
            _bitmapBuilder = bitmapCreatorBuilder;
            QuadrilateralPoints = new ObservableCollection<Point>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double Brightness
        {
            get
            {
                return _bitmapBuilder.Brightness;
            }
            set
            {
                _bitmapBuilder.SetBrightness(value);
                UpdateImage();
                RaisePropertyChanged(nameof(Brightness));
            }
        }

        public int CurrentLayer
        {
            get
            {
                return _bitmapBuilder.Layer;
            }
            set
            {
                _bitmapBuilder.SetLayer(value);
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

        public WriteableBitmap Image
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

        public bool ImageIsOpen
        {
            get
            {
                return _imageIsOpen;
            }
            set
            {
                _imageIsOpen = value;
                RaisePropertyChanged(nameof(ImageIsOpen));
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

        public Quadrilateral MarkerQuadrilateral
        {
            get
            {
                return _markerQuadrilateral;
            }
            set
            {
                _markerQuadrilateral = value;
                ProjectEnabled = _markerQuadrilateral != null;
                RaisePropertyChanged(nameof(MarkerQuadrilateral));
            }
        }

        public ObservableCollection<Point> QuadrilateralPoints
        {
            get
            {
                return _quadrilateralPoints;
            }
            set
            {
                _quadrilateralPoints = value;
                RaisePropertyChanged(nameof(QuadrilateralPoints));
            }
        }

        public ICommand OpenImage
        {
            get
            {
                return new RelayCommand((args) =>
                {
                    ImageIsOpen = false;
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = " RAW Files (*.raw)|*.raw"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        Resetvalues();

                        _bitmapBuilder.SetPath(openFileDialog.FileName);
                        ShowImage();
                    }
                    ImageIsOpen = true;
                });
            }
        }

        public ICommand Project
        {
            get
            {
                return new RelayCommand((args) =>
                {
                    _bitmapBuilder.Project(_markerQuadrilateral);
                    MarkerQuadrilateral = null;
                    QuadrilateralPoints = new ObservableCollection<Point>();
                    UpdateImage();
                });
            }
        }

        public ICommand ClearMarker
        {
            get
            {
                return new RelayCommand((args) =>
                {
                    MarkerQuadrilateral = null;
                    QuadrilateralPoints = new ObservableCollection<Point>();
                    _bitmapBuilder.Project(MarkerQuadrilateral);
                    UpdateImage();
                });
            }
        }

        public bool ProjectEnabled
        {
            get
            {
                return _projectEnabled;
            }
            set
            {
                _projectEnabled = value;
                RaisePropertyChanged(nameof(ProjectEnabled));
            }
        }

        public double RotationAlpha
        {
            get
            {
                return _bitmapBuilder.Alpha;
            }
            set
            {
                _bitmapBuilder.Rotate(value);
                UpdateImage();
                RaisePropertyChanged(nameof(RotationAlpha));
            }
        }

        public ICommand SaveImage
        {
            get
            {
                return new RelayCommand((args) =>
                {
                    OpenFileDialog saveFileDialog = new OpenFileDialog
                    {
                        Filter = " PNG (*.png)|*.png",
                        CheckFileExists = false
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)Image));
                            encoder.Save(fileStream);
                        }
                    }
                });
            }
        }

        public double ScaleSx
        {
            get
            {
                return _scaleSx;
            }
            set
            {
                _scaleSx = value;
                _bitmapBuilder.Scale(_scaleSx, _scaleSy);
                UpdateImage();
                RaisePropertyChanged(nameof(ScaleSx));
            }
        }

        public double ScaleSy
        {
            get
            {
                return _scaleSy;
            }
            set
            {
                _scaleSy = value;
                _bitmapBuilder.Scale(_scaleSx, _scaleSy);
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
                _bitmapBuilder.Shear(_shearBx, _shearBy);
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
                _bitmapBuilder.Shear(_shearBx, _shearBy);
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
                _bitmapBuilder.Shift(_shiftDx, _shiftDy);
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
                _bitmapBuilder.Shift(_shiftDx, _shiftDy);
                UpdateImage();
                RaisePropertyChanged(nameof(ShiftDy));
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
            MarkerQuadrilateral = null;
            QuadrilateralPoints = new ObservableCollection<Point>();
        }

        private void ShowImage()
        {
            FileFormat = Path.GetExtension(_bitmapBuilder.Path);

            Image = _bitmapBuilder.Build();
            ImageHeight = (int)Image.Height;
            ImageWidth = (int)Image.Width;
            LayerCount = _bitmapBuilder.LayerCount - 1;
            LayerSliderEnabled = LayerCount > 1;
        }

        private void UpdateImage()
        {
            if (ImageIsOpen)
            {
                ShowImage();
            }
        }
    }
}