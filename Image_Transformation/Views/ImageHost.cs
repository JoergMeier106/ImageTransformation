using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Image_Transformation.Views
{
    public class ImageHost : FrameworkElement
    {
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(WriteableBitmap), typeof(ImageHost), new PropertyMetadata(ImagePropertyChanged));

        public static readonly DependencyProperty QuadrilateralProperty =
            DependencyProperty.Register(nameof(Quadrilateral), typeof(Quadrilateral), typeof(ImageHost), new PropertyMetadata(QuadrilateralPropertyChanged));

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(ObservableCollection<Point>), typeof(ImageHost), new PropertyMetadata(PointsPropertyChanged));

        private const int CROSS_SIZE = 40;

        private readonly VisualCollection _children;
        private int _adjustedWidth;
        private int _adjustedHeight;

        public ImageHost()
        {
            _children = new VisualCollection(this);
            Points = new ObservableCollection<Point>();
            MouseLeftButtonUp += OnMouseUp;
        }

        public WriteableBitmap Image
        {
            get { return (WriteableBitmap)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public Quadrilateral Quadrilateral
        {
            get { return (Quadrilateral)GetValue(QuadrilateralProperty); }
            set { SetValue(QuadrilateralProperty, value); }
        }

        public ObservableCollection<Point> Points
        {
            get { return (ObservableCollection<Point>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
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

        private void ImagePropertyChanged(WriteableBitmap image, double width, double height)
        {
            CreateImage(image, width, height);
        }

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

        private void CreateImage(WriteableBitmap image, double width, double height)
        {
            if (image != null)
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

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

                if (_children.Count > 0)
                {
                    _children.RemoveAt(0);
                }
                _children.Insert(0, drawingVisual);
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

            if (_children.Count >= 6)
            {
                _children.RemoveAt(1);
            }

            _children.Insert(1, drawingVisual);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            CreateImage(Image, availableSize.Width, availableSize.Height);
            return availableSize;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition((UIElement)sender);
            AddCross(point);
        }

        private void AddCross(Point point)
        {
            if (Points.Count >= 4)
            {
                Points.RemoveAt(0);
                _children.RemoveAt(2);
            }
            Points.Add(point);
            CreateCross(point.X, point.Y);

            if (Points.Count == 4)
            {
                ConnectCrosses();
                CreateQuadrilateral();
            }
        }

        private void CreateQuadrilateral()
        {
            List<Point> pointsOnImage = new List<Point>();
            foreach (Point point in Points)
            {
                pointsOnImage.Add(new Point((point.X / _adjustedWidth) * Image.Width, (point.Y / _adjustedHeight) * Image.Height));
            }
            Quadrilateral = new Quadrilateral(pointsOnImage);
        }
    }
}