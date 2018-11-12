using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Image_Transformation.ViewModels
{
    public class Image3DViewModel : INotifyPropertyChanged
    {
        private readonly Image2DMatrixBuilder _image2DMatrixBuilder;
        private readonly Image3DMatrixBuilder _image3DMatrixBuilder;
        private IImageMatrixBuilder _activeImageMatrixBuilder;
        private bool _asyncEnabled;
        private CancellationTokenSource _cancellationTokenSource;
        private string _fileFormat;
        private WriteableBitmap _image;
        private int _imageHeight;
        private bool _imageIsOpen;
        private int _imageWidth;
        private bool _is3DEnabled;
        private int _layerCount;
        private bool _layerSliderEnabled;
        private Quadrilateral _markerQuadrilateral;
        private bool _projectEnabled;
        private ObservableCollection<Point> _quadrilateralPoints;

        public Image3DViewModel()
        {
            _image2DMatrixBuilder = new Image2DMatrixBuilder();
            _image3DMatrixBuilder = new Image3DMatrixBuilder();
            _activeImageMatrixBuilder = _image2DMatrixBuilder;

            QuadrilateralPoints = new ObservableCollection<Point>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AsyncEnabled
        {
            get { return _asyncEnabled; }
            set
            {
                _asyncEnabled = value;
                RaisePropertyChanged(nameof(AsyncEnabled));
            }
        }

        public double Brightness
        {
            get
            {
                return _activeImageMatrixBuilder.Brightness;
            }
            set
            {
                _activeImageMatrixBuilder.SetBrightness(value);
                RaisePropertyChanged(nameof(Brightness));
            }
        }

        public ICommand ClearMarker
        {
            get
            {
                return new RelayCommand(async (args) =>
                {
                    MarkerQuadrilateral = null;
                    _activeImageMatrixBuilder.MapBilinear(null);
                    QuadrilateralPoints = new ObservableCollection<Point>();
                    _activeImageMatrixBuilder.Project(MarkerQuadrilateral);
                    await UpdateImage();
                });
            }
        }

        public int CurrentLayer
        {
            get
            {
                return _activeImageMatrixBuilder.Layer;
            }
            set
            {
                _activeImageMatrixBuilder.SetLayer(value);
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
                _activeImageMatrixBuilder.SetTargetImageHeight(_imageHeight);
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
                _activeImageMatrixBuilder.SetTargetImageWidth(_imageWidth);
                RaisePropertyChanged(nameof(ImageWidth));
            }
        }

        public bool Is3DEnabled
        {
            get { return _is3DEnabled; }
            set
            {
                _is3DEnabled = value;
                RaisePropertyChanged(nameof(Is3DEnabled));
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

        public ICommand MapBilinear
        {
            get
            {
                return new RelayCommand(async (args) =>
                {
                    _activeImageMatrixBuilder.MapBilinear(_markerQuadrilateral);
                    MarkerQuadrilateral = null;
                    QuadrilateralPoints = new ObservableCollection<Point>();
                    await UpdateImage();
                });
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

        public ICommand OpenImage
        {
            get
            {
                return new RelayCommand(async (args) =>
                {
                    ImageIsOpen = false;
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = " RAW Files (*.raw)|*.raw"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        ResetValues();

                        _activeImageMatrixBuilder.SetPath(openFileDialog.FileName);
                        ImageIsOpen = true;
                        await UpdateImage();
                    }
                });
            }
        }

        public ICommand Project
        {
            get
            {
                return new RelayCommand(async (args) =>
                {
                    _activeImageMatrixBuilder.Project(_markerQuadrilateral);
                    MarkerQuadrilateral = null;
                    QuadrilateralPoints = new ObservableCollection<Point>();
                    await UpdateImage();
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

        public ICommand ResetAll
        {
            get
            {
                return new RelayCommand((args) =>
                {
                    ResetValues();
                });
            }
        }

        public double RotationAlpha
        {
            get
            {
                return _activeImageMatrixBuilder.Alpha;
            }
            set
            {
                _activeImageMatrixBuilder.Rotate(value);
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
                return _activeImageMatrixBuilder.Sx;
            }
            set
            {
                _activeImageMatrixBuilder.Scale(value, ScaleSy);
                RaisePropertyChanged(nameof(ScaleSx));
            }
        }

        public double ScaleSy
        {
            get
            {
                return _activeImageMatrixBuilder.Sy;
            }
            set
            {
                _activeImageMatrixBuilder.Scale(ScaleSx, value);
                RaisePropertyChanged(nameof(ScaleSy));
            }
        }

        public double ShearBx
        {
            get
            {
                return _activeImageMatrixBuilder.Bx;
            }
            set
            {
                _activeImageMatrixBuilder.Shear(value, ShearBy);
                RaisePropertyChanged(nameof(ShearBx));
            }
        }

        public double ShearBy
        {
            get
            {
                return _activeImageMatrixBuilder.By;
            }
            set
            {
                _activeImageMatrixBuilder.Shear(ShearBx, value);
                RaisePropertyChanged(nameof(ShearBy));
            }
        }

        public int ShiftDx
        {
            get
            {
                return _activeImageMatrixBuilder.Dx;
            }
            set
            {
                _activeImageMatrixBuilder.Shift(value, ShiftDy);
                RaisePropertyChanged(nameof(ShiftDx));
            }
        }

        public int ShiftDy
        {
            get
            {
                return _activeImageMatrixBuilder.Dy;
            }
            set
            {
                _activeImageMatrixBuilder.Shift(ShiftDx, value);
                RaisePropertyChanged(nameof(ShiftDy));
            }
        }

        public bool SourceToTargetEnabled
        {
            get
            {
                return _activeImageMatrixBuilder.SourceToTargetEnabled;
            }
            set
            {
                _activeImageMatrixBuilder.SetSourceToTargetEnabled(value);
                RaisePropertyChanged(nameof(SourceToTargetEnabled));
            }
        }

        public ICommand Update
        {
            get
            {
                return new RelayCommand(async (args) =>
                {
                    await UpdateImage();
                });
            }
        }

        private void RaisePropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        private void ResetValues()
        {
            Brightness = 0;
            CurrentLayer = 0;
            ShearBx = 0;
            ShearBy = 0;
            ShiftDx = 0;
            ShiftDy = 0;
            ScaleSx = 1;
            ScaleSy = 1;
            SourceToTargetEnabled = true;
            AsyncEnabled = false;
            Is3DEnabled = false;
            RotationAlpha = 0;
            MarkerQuadrilateral = null;
            QuadrilateralPoints = new ObservableCollection<Point>();

            _activeImageMatrixBuilder.MapBilinear(null)
                          .Project(null)
                          .SetSourceToTargetEnabled(true);
        }

        private void SetActiveBitmapBuilder()
        {
            if (Is3DEnabled)
            {
                _activeImageMatrixBuilder = _image3DMatrixBuilder;
            }
            else
            {
                _activeImageMatrixBuilder = _image2DMatrixBuilder;
            }
        }

        private void SetImagePropertiesInUIThread(ImageMatrix imageMatrix)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                FileFormat = Path.GetExtension(_activeImageMatrixBuilder.Path);
                Image = MatrixToBitmapImageConverter.GetImage(imageMatrix);
                ImageHeight = (int)Image.Height;
                ImageWidth = (int)Image.Width;
                LayerCount = _activeImageMatrixBuilder.LayerCount - 1;
                LayerSliderEnabled = LayerCount > 1;
            });
        }

        private void ShowImage()
        {
            ImageMatrix imageMatrix = _activeImageMatrixBuilder.Build();
            SetImagePropertiesInUIThread(imageMatrix);
        }

        private async Task ShowImageAsync(CancellationToken cancellationToken)
        {
            ImageMatrix imageMatrix = await Task.Factory.StartNew(() => _activeImageMatrixBuilder.Build());

            if (!cancellationToken.IsCancellationRequested)
            {
                SetImagePropertiesInUIThread(imageMatrix);
            }
        }

        private async Task UpdateImage()
        {
            if (ImageIsOpen)
            {
                SetActiveBitmapBuilder();
                if (AsyncEnabled)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    await ShowImageAsync(_cancellationTokenSource.Token);
                }
                else
                {
                    ShowImage();
                }
            }
        }
    }
}