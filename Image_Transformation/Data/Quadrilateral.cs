using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Image_Transformation
{
    /// <summary>
    /// Holds vertices of a quadrilateral. Can be used for projective or bilinear transformations.
    /// </summary>
    public class Quadrilateral
    {
        public Quadrilateral(IEnumerable<Point> points)
        {
            SetProperties(points);
        }

        public Point Point0 { get; private set; }

        public Point Point1 { get; private set; }

        public Point Point2 { get; private set; }

        public Point Point3 { get; private set; }

        public double X0 { get; private set; }

        public double X1 { get; private set; }

        public double X2 { get; private set; }

        public double X3 { get; private set; }

        public double Y0 { get; private set; }

        public double Y1 { get; private set; }

        public double Y2 { get; private set; }

        public double Y3 { get; private set; }

        private void SetProperties(IEnumerable<Point> points)
        {
            if (points != null && points.Count() == 4)
            {
                //Here the points are ordered so that p0, p1, p2, p3 are arranged as follows.
                //p2---p1
                //|    |
                //|    |
                //p3---p0

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
    }
}