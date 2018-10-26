﻿using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Image_Transformation.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IBitmapBuilder _bitmapBuilder;
        private string _fileFormat;
        private ImageSource _image;
        private int _imageHeight;
        private int _imageWidth;
        private int _layerCount;
        private bool _layerSliderEnabled;
        private int _scaleSx;
        private int _scaleSy;
        private int _shearBx;
        private int _shearBy;
        private int _shiftDx;
        private int _shiftDy;
        private bool _imageIsOpen;

        public MainViewModel(IBitmapBuilder bitmapCreatorBuilder)
        {
            _bitmapBuilder = bitmapCreatorBuilder;
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

        public static System.Drawing.Bitmap BitmapSourceToBitmap2(BitmapSource srs)
        {
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(height * stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                using (var btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr))
                {
                    // Clone the bitmap so that we can dispose it and
                    // release the unmanaged memory at ptr
                    return new System.Drawing.Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
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

        public int ScaleSx
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

        public int ScaleSy
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