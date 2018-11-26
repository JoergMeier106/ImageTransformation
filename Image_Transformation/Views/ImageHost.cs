using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Image_Transformation.Views
{
    /// <summary>
    /// Displays an image an adds the possibility to add marker.
    /// </summary>
    public class ImageHost : FrameworkElement
    {
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(WriteableBitmap), typeof(ImageHost), new PropertyMetadata(ImagePropertyChanged));

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(ObservableCollection<Point>), typeof(ImageHost), new PropertyMetadata(PointsPropertyChanged));

        public static readonly DependencyProperty QuadrilateralProperty =
            DependencyProperty.Register(nameof(Quadrilateral), typeof(Quadrilateral), typeof(ImageHost), new PropertyMetadata(QuadrilateralPropertyChanged));

        //The size of the marker cross.
        private const int CROSS_SIZE = 40;
        private const int MAX_DRAWS_COUNT = 6;
        private const int MAX_POINTS_COUNT = 4;
        private const int IMAGE_INDEX = 0;
        private readonly VisualCollection _children;
        private int _adjustedHeight;
        private int _adjustedWidth;

        public ImageHost()
        {
            _children = new VisualCollection(this);
            Points = new ObservableCollection<Point>();
            MouseLeftButtonUp += OnMouseUp;
        }

        /// <summary>
        /// The image which will be displayed.
        /// </summary>
        public WriteableBitmap Image
        {
            get { return (WriteableBitmap)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        /// <summary>
        /// These points hold the marker positions.
        /// </summary>
        public ObservableCollection<Point> Points
        {
            get { return (ObservableCollection<Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        /// <summary>
        /// This quadrilateral will be created when the user added 4 markers.
        /// </summary>
        public Quadrilateral Quadrilateral
        {
            get { return (Quadrilateral)GetValue(QuadrilateralProperty); }
            set { SetValue(QuadrilateralProperty, value); }
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            //Redraw the image if the windows size changed.
            CreateImage(Image, availableSize.Width, availableSize.Height);
            return availableSize;
        }

        private static void ImagePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is ImageHost host && e.NewValue is WriteableBitmap bitmap)
            {
                host.ImagePropertyChanged(bitmap, host.ActualWidth, host.ActualHeight);
            }
        }

        private static void PointsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is ImageHost host)
            {
                host.PointsPropertyChanged();
            }
        }

        private static void QuadrilateralPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is ImageHost host)
            {
                host.QuadrilateralPropertyChanged();
            }
        }

        private void AddCross(Point point)
        {
            if (Points.Count >= MAX_POINTS_COUNT)
            {
                //There are already 4 points, so this call adds a 5th one. This is why the first point will be removed.
                Points.RemoveAt(0);
                _children.RemoveAt(2);
            }
            Points.Add(point);
            CreateCross(point.X, point.Y);

            if (Points.Count == MAX_POINTS_COUNT)
            {
                //When we have 4 points, it is possible to draw and create the quadrilateral.
                ConnectCrosses();
                CreateQuadrilateral();
            }
        }

        private void ConnectCrosses()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            Quadrilateral drawingRectangel = new Quadrilateral(Points);

            Pen pen = new Pen
            {
                Brush = Brushes.Red,
                Thickness = 3
            };

            drawingContext.DrawLine(pen, drawingRectangel.Point0, drawingRectangel.Point1);
            drawingContext.DrawLine(pen, drawingRectangel.Point1, drawingRectangel.Point2);
            drawingContext.DrawLine(pen, drawingRectangel.Point2, drawingRectangel.Point3);
            drawingContext.DrawLine(pen, drawingRectangel.Point3, drawingRectangel.Point0);
            drawingContext.Close();

            if (_children.Count >= MAX_DRAWS_COUNT)
            {
                //The position of the drawn quadrilateral is 1. So if there is already a drawn quadrilateral, it gets removed.
                _children.RemoveAt(1);
            }

            _children.Insert(1, drawingVisual);
        }

        /// <summary>
        /// Adds a cross to given x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void CreateCross(double x, double y)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            Pen pen = new Pen
            {
                Brush = Brushes.Red,
                Thickness = 3
            };

            Point crossCenter = new Point(x, y);
            Point upperLeftPoint = new Point(x - CROSS_SIZE / 2, y - CROSS_SIZE / 2);
            Point lowerLeftPoint = new Point(x - CROSS_SIZE / 2, y + CROSS_SIZE / 2);
            Point lowerRightPoint = new Point(x + CROSS_SIZE / 2, y + CROSS_SIZE / 2);
            Point upperRightPoint = new Point(x + CROSS_SIZE / 2, y - CROSS_SIZE / 2);

            drawingContext.DrawLine(pen, crossCenter, lowerRightPoint);
            drawingContext.DrawLine(pen, crossCenter, upperRightPoint);
            drawingContext.DrawLine(pen, crossCenter, upperLeftPoint);
            drawingContext.DrawLine(pen, crossCenter, lowerLeftPoint);
            drawingContext.Close();

            _children.Add(drawingVisual);
        }

        private void CreateImage(WriteableBitmap image, double width, double height)
        {
            if (image != null)
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                //Dependend on the available height and width, the height and width of the image will be adjusted.
                //With the adjusted size the image ratio will be preserved.
                if (image.Height > image.Width)
                {
                    double ratio = image.Width / image.Height;
                    _adjustedWidth = (int)(height * ratio);
                    _adjustedHeight = (int)height;
                }
                else
                {
                    double ratio = image.Height / image.Width;
                    _adjustedWidth = (int)width;
                    _adjustedHeight = (int)(width * ratio);
                }

                drawingContext.DrawImage(image, new Rect(0, 0, _adjustedWidth, _adjustedHeight));
                drawingContext.Close();

                if (_children.Count > IMAGE_INDEX)
                {
                    _children.RemoveAt(IMAGE_INDEX);
                }
                _children.Insert(IMAGE_INDEX, drawingVisual);
            }
        }

        private void CreateQuadrilateral()
        {
            List<Point> pointsOnImage = new List<Point>();
            foreach (Point point in Points)
            {
                //The stored points store the marker position on the window. For the quadrilateral the position on the image is needed.
                //This is why the points must be divided with the drawn height and width of the image.
                pointsOnImage.Add(new Point((point.X / _adjustedWidth) * Image.Width, (point.Y / _adjustedHeight) * Image.Height));
            }
            Quadrilateral = new Quadrilateral(pointsOnImage);
        }

        private void ImagePropertyChanged(WriteableBitmap image, double width, double height)
        {
            CreateImage(image, width, height);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition((UIElement)sender);
            AddCross(point);
        }

        private void PointsPropertyChanged()
        {
            if (Points == null || !Points.Any())
            {
                if (_children.Count > 4)
                {
                    _children.RemoveRange(_children.Count - 4, 4);
                }
                else if (_children.Count > 1)
                {
                    _children.RemoveRange(1, _children.Count - 1);
                }
            }
        }

        /// <summary>
        /// Remove the drawn quadrilateral if the Quadrilateral is set to null.
        /// </summary>
        private void QuadrilateralPropertyChanged()
        {
            if (Quadrilateral == null)
            {
                if (_children.Count == 6)
                {
                    _children.RemoveAt(1);
                }
            }
        }
    }
}