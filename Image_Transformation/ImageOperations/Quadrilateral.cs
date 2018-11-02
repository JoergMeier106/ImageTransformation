using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Image_Transformation
{
    public class Quadrilateral
    {
        public Quadrilateral(IEnumerable<Point> points)
        {
            if (points != null && points.Count() == 4)
            {
                IEnumerable<Point> yOrderedPoints = points.OrderBy((point) => point.Y);

                IEnumerable<Point> upperPoints = yOrderedPoints.Skip(2);
                IEnumerable<Point> lowerPoints = yOrderedPoints.Take(2);

                IEnumerable<Point> xOrderedUpperPoints = upperPoints.OrderBy((point) => point.X);
                IEnumerable<Point> xOrderedLowerPoints = lowerPoints.OrderBy((point) => point.X);

                Point0 = xOrderedLowerPoints.First();
                Point1 = xOrderedLowerPoints.Last();
                Point2 = xOrderedUpperPoints.Last();
                Point3 = xOrderedUpperPoints.First();

                X0 = Point0.X;
                Y0 = Point0.Y;
                X1 = Point1.X;
                Y1 = Point1.Y;
                X2 = Point2.X;
                Y2 = Point2.Y;
                X3 = Point3.X;
                Y3 = Point3.Y;
            }
        }

        public Point Point0 { get; }
        public Point Point1 { get; }
        public Point Point2 { get; }
        public Point Point3 { get; }
        public double X0 { get; }
        public double X1 { get; }
        public double X2 { get; }
        public double X3 { get; }

        public double Y0 { get; }
        public double Y1 { get; }
        public double Y2 { get; }
        public double Y3 { get; }
    }
}