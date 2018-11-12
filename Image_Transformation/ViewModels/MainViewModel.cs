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
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Bitmap2DBuilder _bitmap2DBuilder;
        private readonly Bitmap3DBuilder _bitmap3DBuilder;
        private IBitmapBuilder _activeBitmapBuilder;
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

        public MainViewModel()
        {
            _bitmap2DBuilder = new Bitmap2DBuilder();
            _bitmap3DBuilder = new Bitmap3DBuilder();
            _activeBitmapBuilder = _bitmap2DBuilder;

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
                return _activeBitmapBuilder.Brightness;
            }
            set
            {
                _activeBitmapBuilder.SetBrightness(value);
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
                    _activeBitmapBuilder.MapBilinear(null);
                    QuadrilateralPoints = new ObservableCollection<Point>();
                    _activeBitmapBuilder.Project(MarkerQuadrilateral);
                    await UpdateImage();
                });
            }
        }

        public int CurrentLayer
        {
            get
            {
                return _activeBitmapBuilder.Layer;
            }
            set
            {
                _activeBitmapBuilder.SetLayer(value);
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
                _activeBitmapBuilder.SetTargetImageHeight(_imageHeight);
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
                _activeBitmapBuilder.SetTargetImageWidth(_imageWidth);
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
                    _activeBitmapBuilder.MapBilinear(_markerQuadrilateral);
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

                        _activeBitmapBuilder.SetPath(openFileDialog.FileName);
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
                    _activeBitmapBuilder.Project(_markerQuadrilateral);
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
                return _activeBitmapBuilder.Alpha;
            }
            set
            {
                _activeBitmapBuilder.Rotate(value);
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
                return _activeBitmapBuilder.Sx;
            }
            set
            {
                _activeBitmapBuilder.Scale(value, ScaleSy);
                RaisePropertyChanged(nameof(ScaleSx));
            }
        }

        public double ScaleSy
        {
            get
            {
                return _activeBitmapBuilder.Sy;
            }
            set
            {
                _activeBitmapBuilder.Scale(ScaleSx, value);
                RaisePropertyChanged(nameof(ScaleSy));
            }
        }

        public double ShearBx
        {
            get
            {
                return _activeBitmapBuilder.Bx;
            }
            set
            {
                _activeBitmapBuilder.Shear(value, ShearBy);
                RaisePropertyChanged(nameof(ShearBx));
            }
        }

        public double ShearBy
        {
            get
            {
                return _activeBitmapBuilder.By;
            }
            set
            {
                _activeBitmapBuilder.Shear(ShearBx, value);
                RaisePropertyChanged(nameof(ShearBy));
            }
        }

        public int ShiftDx
        {
            get
            {
                return _activeBitmapBuilder.Dx;
            }
            set
            {
                _activeBitmapBuilder.Shift(value, ShiftDy);
                RaisePropertyChanged(nameof(ShiftDx));
            }
        }

        public int ShiftDy
        {
            get
            {
                return _activeBitmapBuilder.Dy;
            }
            set
            {
                _activeBitmapBuilder.Shift(ShiftDx, value);
                RaisePropertyChanged(nameof(ShiftDy));
            }
        }

        public bool SourceToTargetEnabled
        {
            get
            {
                return _activeBitmapBuilder.SourceToTargetEnabled;
            }
            set
            {
                _activeBitmapBuilder.SetSourceToTargetEnabled(value);
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
            RotationAlpha = 0;
            MarkerQuadrilateral = null;
            QuadrilateralPoints = new ObservableCollection<Point>();

            _activeBitmapBuilder.MapBilinear(null)
                          .Project(null)
                          .SetSourceToTargetEnabled(true);
        }

        private void SetActiveBitmapBuilder()
        {
            if (Is3DEnabled)
            {
                _activeBitmapBuilder = _bitmap3DBuilder;
            }
            else
            {
                _activeBitmapBuilder = _bitmap2DBuilder;
            }
        }

        private void SetImagePropertiesInUIThread(ImageMatrix imageMatrix)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                FileFormat = Path.GetExtension(_activeBitmapBuilder.Path);
                Image = MatrixToBitmapImageConverter.GetImage(imageMatrix);
                ImageHeight = (int)Image.Height;
                ImageWidth = (int)Image.Width;
                LayerCount = _activeBitmapBuilder.LayerCount - 1;
                LayerSliderEnabled = LayerCount > 1;
            });
        }

        private void ShowImage()
        {
            ImageMatrix imageMatrix = _activeBitmapBuilder.Build();
            SetImagePropertiesInUIThread(imageMatrix);
        }

        private async Task ShowImageAsync(CancellationToken cancellationToken)
        {
            ImageMatrix imageMatrix = await Task.Factory.StartNew(() => _activeBitmapBuilder.Build());

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