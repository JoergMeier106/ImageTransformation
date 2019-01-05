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
            DependencyProperty.Register(nameof(LayerOpacity), typeof(double), typeof(ImageModelControl), new PropertyMetadata(OpacityPropertyChanged));

        public static readonly DependencyProperty LayerSpaceProperty =
            DependencyProperty.Register(nameof(LayerSpace), typeof(double), typeof(ImageModelControl), new PropertyMetadata(SpacePropertyChanged));

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

        private static void OpacityPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is ImageModelControl control)
            {
                control.OpacityPropertyChanged();
            }
        }

        private static void SpacePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is ImageModelControl control)
            {
                control.SpacePropertyChanged();
            }
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
            Model3DGroup modelGroup = new Model3DGroup
            {
                Children = new Model3DCollection(Images.Count())
            };

            for (int i = 0; i < Images.Count(); i++)
            {
                WriteableBitmap originalImage = originalImages[i];
                //The 3D model for one layer is created
                GeometryModel3D layer = await CreateOneLayerAsync(originalImage, i, Images.Count());
                modelGroup.Children.Add(layer);
            }

            return modelGroup;
        }

        private async Task<GeometryModel3D> CreateOneLayerAsync(WriteableBitmap originalImage, int layerIndex, int layerCount)
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

                MeshGeometry3D layerMesh = CreateLayerMesh(bitmapSource.Height, bitmapSource.Width, layerCount + (-layerIndex * layerSpace));

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

        private ParallelOptions CreateParallelOptions()
        {
            return new ParallelOptions
            {
                CancellationToken = _cancellationTokenSource.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
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
                    Model3DGroup modelGroup = await CreateModelsAsync();
                    ModelVisual3D modelsVisual = new ModelVisual3D
                    {
                        Content = modelGroup
                    };

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

        /// <summary>
        /// Dependend on a threshold, an alpha value is applied to dark pixels.
        /// </summary>
        /// <param name="convertedImage">The image which will be changed. This image must have a pixel format which have an alpha channel.</param>
        /// <param name="threshold">Pixel below this threshold receive an alpha value</param>
        /// <param name="alphaValue">This alpha value will be applied to pixel below the threshold</param>
        /// <returns></returns>
        private BitmapSource MakeDarkPixelsTransparent(FormatConvertedBitmap convertedImage, int threshold, int alphaValue)
        {
            Bitmap imageWithDarkToAlpha = ConvertFormatConvertedBitmapToBitmap(convertedImage);

            System.Drawing.Imaging.BitmapData bData = imageWithDarkToAlpha.LockBits(new Rectangle(0, 0, imageWithDarkToAlpha.Width, imageWithDarkToAlpha.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format64bppPArgb);
            
            int bitsPerPixel = convertedImage.Format.BitsPerPixel;
            
            int size = bData.Stride * bData.Height;
            
            byte[] data = new byte[size];

            List<int> indices = CreateIndices(bitsPerPixel, size);

            // This overload copies data of /size/ into /data/ from location specified (/Scan0/)
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);

            Parallel.ForEach(indices, CreateParallelOptions(), (i) =>
            {
                double magnitude = 1 / 3d * (data[i] + data[i + 1] + data[i + 2]);

                //data[i] is the first of 3 bytes of color
                Color color = Color.FromArgb(data[i], data[i + 1], data[i + 2]);
                data[i + 6] = data[i];
                data[i + 7] = data[i + 1];
            });

            // This override copies the data back into the location specified
            System.Runtime.InteropServices.Marshal.Copy(data, 0, bData.Scan0, data.Length);

            imageWithDarkToAlpha.UnlockBits(bData);

            //To use the new image as the texture of a 3D model, a bitmapSource is needed.
            BitmapSource bitmapSource = ConvertBitmapToBitmapSource(imageWithDarkToAlpha);
            imageWithDarkToAlpha.Dispose();
            return bitmapSource;
        }

        private static List<int> CreateIndices(int bitsPerPixel, int size)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < size; i += bitsPerPixel / 8)
            {
                indices.Add(i);
            }

            return indices;
        }

        private void OpacityPropertyChanged()
        {
            if (Children.Count() >= 2 && Children[1] is ModelVisual3D model && model.Content is Model3DGroup group)
            {
                int depth = Images.Count();
                int currentLayer = 0;

                foreach (var image in Images)
                {
                    MeshGeometry3D layerMesh = CreateLayerMesh(image.Height, image.Width, depth + (-currentLayer * LayerSpace));
                    GeometryModel3D oldLayer = (GeometryModel3D)group.Children[currentLayer];
                    DiffuseMaterial material = (DiffuseMaterial)oldLayer.Material.CloneCurrentValue();

                    ImageBrush brush = (ImageBrush)material.Brush;
                    brush.Opacity = LayerOpacity;

                    group.Children.RemoveAt(currentLayer);
                    GeometryModel3D newLayer = new GeometryModel3D
                    {
                        Geometry = layerMesh,
                        Material = material,
                        BackMaterial = material
                    };
                    group.Children.Insert(currentLayer, newLayer);
                    currentLayer++;
                }
            }
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

        private void SpacePropertyChanged()
        {
            if (Children.Count() >= 2 && Children[1] is ModelVisual3D model && model.Content is Model3DGroup group)
            {
                int depth = Images.Count();
                int currentLayer = 0;

                foreach (var image in Images)
                {
                    MeshGeometry3D layerMesh = CreateLayerMesh(image.Height, image.Width, depth + (-currentLayer * LayerSpace));
                    GeometryModel3D oldLayer = (GeometryModel3D)group.Children[currentLayer];
                    group.Children.RemoveAt(currentLayer);
                    GeometryModel3D newLayer = new GeometryModel3D
                    {
                        Geometry = layerMesh,
                        Material = oldLayer.Material,
                        BackMaterial = oldLayer.BackMaterial
                    };
                    group.Children.Insert(currentLayer, newLayer);
                    currentLayer++;
                }
            }
        }
    }
}