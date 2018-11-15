using Microsoft.Win32;
using System.Collections.Generic;
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
        private readonly Image3DMatrixBuilder _image3DMatrixBuilder;
        private bool _asyncEnabled;
        private CancellationTokenSource _cancellationTokenSource;
        private string _fileFormat;
        private List<WriteableBitmap> _images;
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
            _image3DMatrixBuilder = new Image3DMatrixBuilder();

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
                return _image3DMatrixBuilder.Brightness;
            }
            set
            {
                _image3DMatrixBuilder.SetBrightness(value);
                RaisePropertyChanged(nameof(Brightness));
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

        public List<WriteableBitmap> Images
        {
            get
            {
                return _images;
            }
            set
            {
                _images = value;
                RaisePropertyChanged(nameof(Images));
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

                        _image3DMatrixBuilder.SetPath(openFileDialog.FileName);
                        ImageIsOpen = true;
                        await UpdateImage();
                    }
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
                return _image3DMatrixBuilder.Alpha;
            }
            set
            {
                _image3DMatrixBuilder.Rotate(value);
                RaisePropertyChanged(nameof(RotationAlpha));
            }
        }

        public double ScaleSx
        {
            get
            {
                return _image3DMatrixBuilder.Sx;
            }
            set
            {
                _image3DMatrixBuilder.Scale(value, ScaleSy);
                RaisePropertyChanged(nameof(ScaleSx));
            }
        }

        public double ScaleSy
        {
            get
            {
                return _image3DMatrixBuilder.Sy;
            }
            set
            {
                _image3DMatrixBuilder.Scale(ScaleSx, value);
                RaisePropertyChanged(nameof(ScaleSy));
            }
        }

        public double ShearBx
        {
            get
            {
                return _image3DMatrixBuilder.Bx;
            }
            set
            {
                _image3DMatrixBuilder.Shear(value, ShearBy);
                RaisePropertyChanged(nameof(ShearBx));
            }
        }

        public double ShearBy
        {
            get
            {
                return _image3DMatrixBuilder.By;
            }
            set
            {
                _image3DMatrixBuilder.Shear(ShearBx, value);
                RaisePropertyChanged(nameof(ShearBy));
            }
        }

        public int ShiftDx
        {
            get
            {
                return _image3DMatrixBuilder.Dx;
            }
            set
            {
                _image3DMatrixBuilder.Shift(value, ShiftDy);
                RaisePropertyChanged(nameof(ShiftDx));
            }
        }

        public int ShiftDy
        {
            get
            {
                return _image3DMatrixBuilder.Dy;
            }
            set
            {
                _image3DMatrixBuilder.Shift(ShiftDx, value);
                RaisePropertyChanged(nameof(ShiftDy));
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
            ShearBx = 0;
            ShearBy = 0;
            ShiftDx = 0;
            ShiftDy = 0;
            ScaleSx = 1;
            ScaleSy = 1;
            AsyncEnabled = false;
            Is3DEnabled = false;
            RotationAlpha = 0;
            MarkerQuadrilateral = null;
            QuadrilateralPoints = new ObservableCollection<Point>();
        }

        private void SetImagePropertiesInUIThread(List<WriteableBitmap> images)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                FileFormat = Path.GetExtension(_image3DMatrixBuilder.Path);
                Images = images;
                ImageHeight = (int)images[0].Height;
                ImageWidth = (int)images[0].Width;
                LayerCount = _image3DMatrixBuilder.LayerCount - 1;
                LayerSliderEnabled = LayerCount > 1;
            });
        }

        private void ShowImage()
        {
            Image2DMatrix matrix = _image3DMatrixBuilder.SetLayer(0).Build();
            WriteableBitmap bitmap = MatrixToBitmapImageConverter.GetImage(matrix);
            List<WriteableBitmap> images = new List<WriteableBitmap>();
            images.Add(bitmap);

            for (int i = 1; i < _image3DMatrixBuilder.LayerCount; i++)
            {
                matrix = _image3DMatrixBuilder.SetLayer(i).Build();
                bitmap = MatrixToBitmapImageConverter.GetImage(matrix);
                images.Add(bitmap);
            }
            SetImagePropertiesInUIThread(images);
        }

        private async Task ShowImageAsync(CancellationToken cancellationToken)
        {
            Image2DMatrix matrix = await Task.Factory.StartNew(() => _image3DMatrixBuilder.SetLayer(0).Build());
            WriteableBitmap bitmap = MatrixToBitmapImageConverter.GetImage(matrix);
            List<WriteableBitmap> images = new List<WriteableBitmap>();
            images.Add(bitmap);

            for (int i = 1; i < _image3DMatrixBuilder.LayerCount; i++)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    matrix = await Task.Factory.StartNew(() => _image3DMatrixBuilder.SetLayer(i).Build());
                    bitmap = MatrixToBitmapImageConverter.GetImage(matrix);
                    images.Add(bitmap);
                }
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                SetImagePropertiesInUIThread(images);
            }
        }

        private async Task UpdateImage()
        {
            if (ImageIsOpen)
            {
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