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
    public class Image2DViewModel : INotifyPropertyChanged
    {
        private readonly Image2DMatrixBuilder _image2DMatrixBuilder;
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

        public Image2DViewModel()
        {
            _image2DMatrixBuilder = new Image2DMatrixBuilder();

            QuadrilateralPoints = new ObservableCollection<Point>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Binding Properties

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
                return _image2DMatrixBuilder.Brightness;
            }
            set
            {
                _image2DMatrixBuilder.SetBrightness(value);
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
                    _image2DMatrixBuilder.MapBilinear(null);
                    QuadrilateralPoints = new ObservableCollection<Point>();
                    _image2DMatrixBuilder.Project(MarkerQuadrilateral);
                    await UpdateImage();
                });
            }
        }

        public int CurrentLayer
        {
            get
            {
                return _image2DMatrixBuilder.Layer;
            }
            set
            {
                _image2DMatrixBuilder.SetLayer(value);
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
                _image2DMatrixBuilder.SetTargetImageHeight(_imageHeight);
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
                _image2DMatrixBuilder.SetTargetImageWidth(_imageWidth);
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
                    _image2DMatrixBuilder.MapBilinear(_markerQuadrilateral);
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

                        _image2DMatrixBuilder.SetPath(openFileDialog.FileName);
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
                    _image2DMatrixBuilder.Project(_markerQuadrilateral);
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
                return _image2DMatrixBuilder.Alpha;
            }
            set
            {
                _image2DMatrixBuilder.Rotate(value);
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
                            encoder.Frames.Add(BitmapFrame.Create(Image));
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
                return _image2DMatrixBuilder.Sx;
            }
            set
            {
                _image2DMatrixBuilder.Scale(value, ScaleSy);
                RaisePropertyChanged(nameof(ScaleSx));
            }
        }

        public double ScaleSy
        {
            get
            {
                return _image2DMatrixBuilder.Sy;
            }
            set
            {
                _image2DMatrixBuilder.Scale(ScaleSx, value);
                RaisePropertyChanged(nameof(ScaleSy));
            }
        }

        public double ShearBx
        {
            get
            {
                return _image2DMatrixBuilder.Bx;
            }
            set
            {
                _image2DMatrixBuilder.Shear(value, ShearBy);
                RaisePropertyChanged(nameof(ShearBx));
            }
        }

        public double ShearBy
        {
            get
            {
                return _image2DMatrixBuilder.By;
            }
            set
            {
                _image2DMatrixBuilder.Shear(ShearBx, value);
                RaisePropertyChanged(nameof(ShearBy));
            }
        }

        public int ShiftDx
        {
            get
            {
                return _image2DMatrixBuilder.Dx;
            }
            set
            {
                _image2DMatrixBuilder.Shift(value, ShiftDy);
                RaisePropertyChanged(nameof(ShiftDx));
            }
        }

        public int ShiftDy
        {
            get
            {
                return _image2DMatrixBuilder.Dy;
            }
            set
            {
                _image2DMatrixBuilder.Shift(ShiftDx, value);
                RaisePropertyChanged(nameof(ShiftDy));
            }
        }

        public bool SourceToTargetEnabled
        {
            get
            {
                return _image2DMatrixBuilder.SourceToTargetEnabled;
            }
            set
            {
                _image2DMatrixBuilder.SetSourceToTargetEnabled(value);
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

        #endregion Binding Properties

        private void CancelOngoingOperations()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
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

            _image2DMatrixBuilder.MapBilinear(null)
                          .Project(null)
                          .SetSourceToTargetEnabled(true);
        }

        private void SetImagePropertiesInUIThread(WriteableBitmap image)
        {
            //Binding properties must be set in the UI thread.
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                FileFormat = Path.GetExtension(_image2DMatrixBuilder.Path);
                Image = image;
                ImageHeight = (int)Image.Height;
                ImageWidth = (int)Image.Width;
                LayerCount = _image2DMatrixBuilder.LayerCount - 1;
                LayerSliderEnabled = LayerCount > 1;
            });
        }

        private void ShowImage()
        {
            WriteableBitmap image = _image2DMatrixBuilder.Build();
            SetImagePropertiesInUIThread(image);
        }

        private async Task ShowImageAsync(CancellationToken cancellationToken)
        {
            WriteableBitmap image = await Task.Factory.StartNew(() => _image2DMatrixBuilder.Build());

            if (!cancellationToken.IsCancellationRequested)
            {
                SetImagePropertiesInUIThread(image);
            }
        }

        private async Task UpdateImage()
        {
            if (ImageIsOpen)
            {
                CancelOngoingOperations();
                if (AsyncEnabled)
                {
                    CancelOngoingOperations();
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