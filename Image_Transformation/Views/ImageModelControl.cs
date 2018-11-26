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
    /// <summary>
    /// Handles the loading of multiple images as 3D model.
    /// </summary>
    public class ImageModelControl : HelixViewport3D, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register(nameof(Images), typeof(IEnumerable<WriteableBitmap>), typeof(ImageModelControl), new PropertyMetadata(ImagesPropertyChanged));

        public static readonly DependencyProperty LayerOpacityProperty =
            DependencyProperty.Register(nameof(LayerOpacity), typeof(double), typeof(ImageModelControl), new PropertyMetadata(ImagesPropertyChanged));

        public static readonly DependencyProperty LayerSpaceProperty =
            DependencyProperty.Register(nameof(LayerSpace), typeof(double), typeof(ImageModelControl), new PropertyMetadata(ImagesPropertyChanged));

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isBusy;

        public ImageModelControl()
        {
            SetCameraStartingPosition();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The images from which the model is created.
        /// </summary>
        public IEnumerable<WriteableBitmap> Images
        {
            get { return (IEnumerable<WriteableBitmap>)GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }

        /// <summary>
        /// Enables the use of a BusyIndicator.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }

        /// <summary>
        /// Sets the image opacity of each layer.
        /// </summary>
        public double LayerOpacity
        {
            get { return (double)GetValue(LayerOpacityProperty); }
            set { SetValue(LayerOpacityProperty, value); }
        }

        /// <summary>
        /// Sets the space between the layer.
        /// </summary>
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

        /// <summary>
        /// Creates a MeshGeometry3D which is quadrilateral. For each layer such a mesh is created.
        /// </summary>
        /// <param name="height">The height of the quadrilateral</param>
        /// <param name="width">The width of the quadrilateral</param>
        /// <param name="zOff">The distance from the origin in z direction</param>
        /// <returns></returns>
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

            //The triangles are needed for applying textures.
            Int32Collection Triangles = new Int32Collection(new int[]
            {
                 2,1,0,
                 3,2,0
            });
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

        /// <summary>
        /// Dependend on a threshold, an alpha value is applied to dark pixels.
        /// </summary>
        /// <param name="convertedImage">The image which will be changed. This image must have a pixel format which have an alpha channel.</param>
        /// <param name="threshold">Pixel below this threshold receive an alpha value</param>
        /// <param name="alphaValue">This alpha value will be applied to pixel below the threshold</param>
        /// <returns></returns>
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
            //To use the new image as the texture of a 3D model, a bitmapSource is needed.
            BitmapSource bitmapSource = ConvertBitmapToBitmapSource(imageWithDarkToAlpha);
            imageWithDarkToAlpha.Dispose();
            return bitmapSource;
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async Task<Model3DGroup> CreateModelsAsync()
        {
            List<WriteableBitmap> originalImages = new List<WriteableBitmap>(Images);
            Model3DGroup modelGroup = new Model3DGroup();

            for (int i = 0; i < Images.Count(); i++)
            {
                WriteableBitmap originalImage = originalImages[i];
                //The 3D model for one layer is created
                GeometryModel3D layer = await CreateOneLayerAsync(originalImage, i);
                modelGroup.Children.Add(layer);
            }

            return modelGroup;
        }

        private async Task<GeometryModel3D> CreateOneLayerAsync(WriteableBitmap originalImage, int layerIndex)
        {
            GeometryModel3D layer = null;
            //The freeze is needed to share the object between threads.
            originalImage.Freeze();
            double layerSpace = LayerSpace;
            double layerOpacity = LayerOpacity;

            await Task.Run(() =>
            {
                //Add an alpha channel to the image.
                FormatConvertedBitmap convertedImage = ConvertPixelFormat(originalImage, PixelFormats.Prgba64);
                //Add alpha value to dark pixel.
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
                //Cancel any ongoing operation.
                Cancel();

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

        private void SetCameraStartingPosition()
        {
            Camera.Position = new Point3D(0, 5, -3500);
            Camera.LookDirection = new Vector3D(0, -5, 3500);
            (Camera as PerspectiveCamera).FieldOfView = 10;
        }

        private void ShowModels(ModelVisual3D modelsVisual)
        {
            Children.Clear();
            Children.Add(new DefaultLights());
            Children.Add(modelsVisual);
        }
    }
}