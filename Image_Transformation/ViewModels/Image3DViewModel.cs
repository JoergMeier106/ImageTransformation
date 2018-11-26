using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Image_Transformation.ViewModels
{
    public class Image3DViewModel : INotifyPropertyChanged
    {
        private readonly Image3DBuilder _image3DMatrixBuilder;
        private bool _asyncEnabled;
        private CancellationTokenSource _cancellationTokenSource;
        private int _imageDepth;
        private int _imageHeight;
        private bool _imageIsOpen;
        private List<WriteableBitmap> _images;
        private int _imageWidth;
        private bool _isBusy;

        public Image3DViewModel()
        {
            _image3DMatrixBuilder = new Image3DBuilder();
            _cancellationTokenSource = new CancellationTokenSource();
            AsyncEnabled = true;
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

        public int ImageDepth
        {
            get
            {
                return _imageDepth;
            }
            set
            {
                _imageDepth = value;
                RaisePropertyChanged(nameof(ImageDepth));
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

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged(nameof(IsBusy));
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

        public double RotationXAlpha
        {
            get
            {
                return _image3DMatrixBuilder.AlphaX;
            }
            set
            {
                _image3DMatrixBuilder.RotateX(value);
                RaisePropertyChanged(nameof(RotationXAlpha));
            }
        }

        public double RotationYAlpha
        {
            get
            {
                return _image3DMatrixBuilder.AlphaY;
            }
            set
            {
                _image3DMatrixBuilder.RotateY(value);
                RaisePropertyChanged(nameof(RotationYAlpha));
            }
        }

        public double RotationZAlpha
        {
            get
            {
                return _image3DMatrixBuilder.AlphaZ;
            }
            set
            {
                _image3DMatrixBuilder.RotateZ(value);
                RaisePropertyChanged(nameof(RotationZAlpha));
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
                _image3DMatrixBuilder.Scale(value, ScaleSy, ScaleSz);
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
                _image3DMatrixBuilder.Scale(ScaleSx, value, ScaleSz);
                RaisePropertyChanged(nameof(ScaleSy));
            }
        }

        public double ScaleSz
        {
            get
            {
                return _image3DMatrixBuilder.Sz;
            }
            set
            {
                _image3DMatrixBuilder.Scale(ScaleSx, ScaleSy, value);
                RaisePropertyChanged(nameof(ScaleSz));
            }
        }

        public double ShearBxy
        {
            get
            {
                return _image3DMatrixBuilder.Bxy;
            }
            set
            {
                _image3DMatrixBuilder.Shear(value, ShearByx, ShearBxz, ShearBzx, ShearByz, ShearBzy);
                RaisePropertyChanged(nameof(ShearBxy));
            }
        }

        public double ShearBxz
        {
            get
            {
                return _image3DMatrixBuilder.Bxz;
            }
            set
            {
                _image3DMatrixBuilder.Shear(ShearBxy, ShearByx, value, ShearBzx, ShearByz, ShearBzy);
                RaisePropertyChanged(nameof(ShearBxz));
            }
        }

        public double ShearByx
        {
            get
            {
                return _image3DMatrixBuilder.Byx;
            }
            set
            {
                _image3DMatrixBuilder.Shear(ShearBxy, value, ShearBxz, ShearBzx, ShearByz, ShearBzy);
                RaisePropertyChanged(nameof(ShearByx));
            }
        }

        public double ShearByz
        {
            get
            {
                return _image3DMatrixBuilder.Byz;
            }
            set
            {
                _image3DMatrixBuilder.Shear(ShearBxy, ShearByx, ShearBxz, ShearBzx, value, ShearBzy);
                RaisePropertyChanged(nameof(ShearByz));
            }
        }

        public double ShearBzx
        {
            get
            {
                return _image3DMatrixBuilder.Bzx;
            }
            set
            {
                _image3DMatrixBuilder.Shear(ShearBxy, ShearByx, ShearBxz, value, ShearByz, ShearBzy);
                RaisePropertyChanged(nameof(ShearBzx));
            }
        }

        public double ShearBzy
        {
            get
            {
                return _image3DMatrixBuilder.Bzy;
            }
            set
            {
                _image3DMatrixBuilder.Shear(ShearBxy, ShearByx, ShearBxz, ShearBzx, ShearByz, value);
                RaisePropertyChanged(nameof(ShearBzy));
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
                _image3DMatrixBuilder.Shift(value, ShiftDy, ShiftDz);
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
                _image3DMatrixBuilder.Shift(ShiftDx, value, ShiftDz);
                RaisePropertyChanged(nameof(ShiftDy));
            }
        }

        public int ShiftDz
        {
            get
            {
                return _image3DMatrixBuilder.Dz;
            }
            set
            {
                _image3DMatrixBuilder.Shift(ShiftDx, ShiftDy, value);
                RaisePropertyChanged(nameof(ShiftDz));
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
            ShearBxy = 0;
            ShearByx = 0;
            ShearByz = 0;
            ShearBzy = 0;
            ShearBzx = 0;
            ShearBxz = 0;
            ShiftDx = 0;
            ShiftDy = 0;
            ShiftDz = 0;
            ScaleSx = 1;
            ScaleSy = 1;
            ScaleSz = 1;
            AsyncEnabled = true;
            RotationXAlpha = 0;
            RotationYAlpha = 0;
            RotationZAlpha = 0;
        }

        private void SetImagePropertiesInUIThread(List<WriteableBitmap> images)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Images = images;
                ImageHeight = (int)images[0].Height;
                ImageWidth = (int)images[0].Width;
                ImageDepth = images.Count;
            });
        }

        private void ShowImage()
        {
            List<WriteableBitmap> images = new List<WriteableBitmap>(_image3DMatrixBuilder.Build());
            SetImagePropertiesInUIThread(images);
        }

        private async Task ShowImageAsync(CancellationToken cancellationToken)
        {
            List<WriteableBitmap> images = new List<WriteableBitmap>(await Task.Factory.StartNew(() => _image3DMatrixBuilder.Build()));
            if (!cancellationToken.IsCancellationRequested)
            {
                SetImagePropertiesInUIThread(images);
            }
        }

        private async Task UpdateImage()
        {
            try
            {
                if (ImageIsOpen)
                {
                    IsBusy = true;
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
                    IsBusy = false;
                }
            }
            catch (System.Exception e)
            {
                string stacktrace = e.StackTrace;
                throw;
            }
        }
    }
}