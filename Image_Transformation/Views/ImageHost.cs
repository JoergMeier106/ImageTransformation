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
        private readonly List<Point> _points;

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(WriteableBitmap), typeof(ImageHost), new PropertyMetadata(ImagePropertyChanged));

        public static readonly DependencyProperty RectangelProperty =
            DependencyProperty.Register(nameof(Rectangel), typeof(Rectangel), typeof(ImageHost), new PropertyMetadata(RectangelPropertyChanged));

        private const int CROSS_SIZE = 40;

        private readonly VisualCollection _children;
        private int _adjustedWidth;
        private int _adjustedHeight;

        public ImageHost()
        {
            _children = new VisualCollection(this);
            _points = new List<Point>();
            MouseUp += OnMouseUp;
        }

        public WriteableBitmap Image
        {
            get { return (WriteableBitmap)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public Rectangel Rectangel
        {
            get { return (Rectangel)GetValue(RectangelProperty); }
            set { SetValue(RectangelProperty, value); }
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

        private static void RectangelPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is ImageHost host)
            {
                host.RectangelPropertyChanged();
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

        private void RectangelPropertyChanged()
        {
            if (Rectangel == null)
            {
                _children.RemoveRange(_children.Count - 4, 4);

                if (_children.Count >= 2)
                {
                    _children.RemoveAt(1);
                }

                _points.Clear();
            }
        }

        private void CreateImage(WriteableBitmap image, double width, double height)
        {
            if (image != null)
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                double ratio = image.Height / image.Width;
                _adjustedWidth = (int)width;
                _adjustedHeight = (int)(width * ratio);

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

            Rectangel drawingRectangel = new Rectangel(_points);

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
            if (_points.Count >= 4)
            {
                _points.RemoveAt(0);
                _children.RemoveAt(2);
            }
            _points.Add(point);
            CreateCross(point.X, point.Y);

            if (_points.Count == 4)
            {
                ConnectCrosses();
                CreateRectangle();
            }
        }

        private void CreateRectangle()
        {
            List<Point> pointsOnImage = new List<Point>();
            _points.ForEach((point) =>
            {
                pointsOnImage.Add(new Point((point.X / _adjustedWidth) * Image.Width, (point.Y / _adjustedHeight) * Image.Height));
            });
            Rectangel = new Rectangel(pointsOnImage);
        }
    }
}