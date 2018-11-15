using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;
using Point3D = System.Windows.Media.Media3D.Point3D;

namespace Image_Transformation.Views
{
    public class ImageModelControl : HelixViewport3D, INotifyPropertyChanged
    {
        private CancellationTokenSource _cancellationTokenSource;

        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register(nameof(Images), typeof(IEnumerable<WriteableBitmap>), typeof(ImageModelControl), new PropertyMetadata(ImagesPropertyChanged));

        public static readonly DependencyProperty LayerOpacityProperty =
            DependencyProperty.Register(nameof(LayerOpacity), typeof(double), typeof(ImageModelControl), new PropertyMetadata(ImagesPropertyChanged));

        public static readonly DependencyProperty LayerSpaceProperty =
            DependencyProperty.Register(nameof(LayerSpace), typeof(double), typeof(ImageModelControl), new PropertyMetadata(ImagesPropertyChanged));

        private bool _isBusy;

        public ImageModelControl()
        {
            Camera.Position = new Point3D(0, 5, -3500);
            Camera.LookDirection = new Vector3D(0, -5, 3500);
            (Camera as PerspectiveCamera).FieldOfView = 10;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<WriteableBitmap> Images
        {
            get { return (IEnumerable<WriteableBitmap>)GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }

        public double LayerOpacity
        {
            get { return (double)GetValue(LayerOpacityProperty); }
            set { SetValue(LayerOpacityProperty, value); }
        }

        public double LayerSpace
        {
            get { return (double)GetValue(LayerSpaceProperty); }
            set { SetValue(LayerSpaceProperty, value); }
        }

        private static BitmapSource ConvertBitmapToBitmapSource(Bitmap imageWithDarkToAlpha)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(imageWithDarkToAlpha.GetHbitmap(),
                                                         IntPtr.Zero,
                                                         Int32Rect.Empty,
                                                         BitmapSizeOptions.FromEmptyOptions());
        }

        private static Bitmap ConvertFormatConvertedBitmapToBitmap(FormatConvertedBitmap formatConvertedBitmap)
        {
            Bitmap bitmap;
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(formatConvertedBitmap));
                encoder.Save(stream);
                bitmap = new Bitmap(stream);
            }

            return bitmap;
        }

        private static FormatConvertedBitmap ConvertPixelFormat(WriteableBitmap originalImage, PixelFormat format)
        {
            FormatConvertedBitmap convertedImage = new FormatConvertedBitmap();
            BitmapImage imageToConvert = ConvertWriteableBitmapToBitmapImage(originalImage);

            convertedImage.BeginInit();
            convertedImage.Source = imageToConvert;
            convertedImage.DestinationFormat = format;
            convertedImage.EndInit();

            return convertedImage;
        }

        private static BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap writeableBitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                encoder.Save(stream);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        private static MeshGeometry3D CreateLayerMesh(double height, double width, double zOff)
        {
            MeshGeometry3D layerMesh = new MeshGeometry3D();
            Point3DCollection corners = new Point3DCollection
            {
                new Point3D(width / 2, height / 2, 1 + zOff),
                new Point3D(-width / 2, height / 2, 1 + zOff),
                new Point3D(-width / 2, -height / 2, 1 + zOff),
                new Point3D(width / 2, -height / 2, 1 + zOff)
            };
            layerMesh.Positions = corners;

            Int32[] indices =
            {
                 2,1,0,
                 3,2,0
            };

            Int32Collection Triangles = new Int32Collection(indices);
            layerMesh.TriangleIndices = Triangles;
            layerMesh.TextureCoordinates = new PointCollection(new List<Point>
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(1, 1),
                new Point(0, 1)
            });
            return layerMesh;
        }

        private static void ImagesPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is ImageModelControl control)
            {
                control.ImagesPropertyChanged();
            }
        }

        private static BitmapSource MakeDarkPixelsTransparent(FormatConvertedBitmap convertedImage, int threshold, int alphaValue)
        {
            Bitmap imageWithDarkToAlpha = ConvertFormatConvertedBitmapToBitmap(convertedImage);

            for (int y = 0; y < imageWithDarkToAlpha.Height; y++)
            {
                for (int x = 0; x < imageWithDarkToAlpha.Width; x++)
                {
                    Color color = imageWithDarkToAlpha.GetPixel(x, y);
                    if (color.R < threshold && color.G < threshold && color.B < threshold)
                    {
                        imageWithDarkToAlpha.SetPixel(x, y, Color.FromArgb(alphaValue, color.R, color.G, color.B));
                    }
                }
            }
            BitmapSource bitmapSource = ConvertBitmapToBitmapSource(imageWithDarkToAlpha);
            return bitmapSource;
        }

        private async Task<Model3DGroup> CreateModelsAsync()
        {
            List<WriteableBitmap> originalImages = new List<WriteableBitmap>(Images);
            Model3DGroup modelGroup = new Model3DGroup();

            for (int i = 0; i < Images.Count(); i++)
            {
                WriteableBitmap originalImage = originalImages[i];
                GeometryModel3D layer = await CreateOneLayerAsync(originalImage, i);
                modelGroup.Children.Add(layer);
            }

            return modelGroup;
        }

        private async Task<GeometryModel3D> CreateOneLayerAsync(WriteableBitmap originalImage, int layerIndex)
        {
            GeometryModel3D layer = null;
            originalImage.Freeze();
            double layerSpace = LayerSpace;
            double layerOpacity = LayerOpacity;

            await Task.Run(() =>
            {
                FormatConvertedBitmap convertedImage = ConvertPixelFormat(originalImage, PixelFormats.Prgba64);
                BitmapSource bitmapSource = MakeDarkPixelsTransparent(convertedImage, threshold: 100, alphaValue: 0);

                MeshGeometry3D layerMesh = CreateLayerMesh(bitmapSource.Height, bitmapSource.Width, -layerIndex * layerSpace);
                Material material = new DiffuseMaterial(new ImageBrush(bitmapSource) { Opacity = layerOpacity });
                layer = new GeometryModel3D
                {
                    Geometry = layerMesh,
                    Material = material,
                    BackMaterial = material
                };
                layer.Freeze();
            }, _cancellationTokenSource.Token);
            return layer;
        }

        private async void ImagesPropertyChanged()
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                IsBusy = true;
            });

            if (Images != null && Images.Any())
            {
                ResetCancellationTokenSource();

                try
                {
                    ModelVisual3D modelsVisual = new ModelVisual3D();
                    Model3DGroup modelGroup = await CreateModelsAsync();
                    modelsVisual.Content = modelGroup;

                    ShowModels(modelsVisual);
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Operation canceled");
                }
            }
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                IsBusy = false;
            });
        }

        private void ResetCancellationTokenSource()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void ShowModels(ModelVisual3D modelsVisual)
        {
            Children.Clear();
            Children.Add(new DefaultLights());
            Children.Add(modelsVisual);
        }
    }
}