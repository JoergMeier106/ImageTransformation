using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Image_Transformation
{
    public class BilinearOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;

        public BilinearOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public Quadrilateral Quadrilateral { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged => _imageLoader.MatrixChanged;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;

        public ImageMatrix GetImageMatrix()
        {
            ImageMatrix sourceMatrix = _imageLoader.GetImageMatrix();

            if (Quadrilateral != null)
            {
                double targetWidth = Math.Max(Quadrilateral.X1 - Quadrilateral.X0,
                                              Quadrilateral.X2 - Quadrilateral.X3);
                double targetHeight = Math.Max(Quadrilateral.Y2 - Quadrilateral.Y1,
                                               Quadrilateral.Y3 - Quadrilateral.Y0);

                Quadrilateral targetQuadrilateral = new Quadrilateral(new List<Point>
                {
                    new Point(0, 0),
                    new Point(0, targetHeight),
                    new Point(targetWidth, 0),
                    new Point(targetWidth, targetHeight)
                });

                double x0 = Quadrilateral.X0;
                double x1 = Quadrilateral.X1;
                double x2 = Quadrilateral.X2;
                double x3 = Quadrilateral.X3;

                double y0 = Quadrilateral.Y0;
                double y1 = Quadrilateral.Y1;
                double y2 = Quadrilateral.Y2;
                double y3 = Quadrilateral.Y3;

                double x0_ = targetQuadrilateral.X0;
                double x1_ = targetQuadrilateral.X1;
                double x2_ = targetQuadrilateral.X2;
                double x3_ = targetQuadrilateral.X3;

                double y0_ = targetQuadrilateral.Y0;
                double y1_ = targetQuadrilateral.Y1;
                double y2_ = targetQuadrilateral.Y2;
                double y3_ = targetQuadrilateral.Y3;

                var m = Matrix<double>.Build.DenseOfArray(new double[,]
                {
                    { x0, y0, x0*y0, 1 },
                    { x1, y1, x1*y1, 1 },
                    { x2, y2, x2*y2, 1 },
                    { x3, y3, x3*y3, 1 },
                });
                var targetXVector = Vector<Double>.Build.Dense(new double[] { x0_, x1_, x2_, x3_ });
                var targetYVector = Vector<Double>.Build.Dense(new double[] { y0_, y1_, y2_, y3_ });
                var a = m.Solve(targetXVector);
                var b = m.Solve(targetYVector);

                double a0 = a[0];
                double a1 = a[1];
                double a2 = a[2];
                double a3 = a[3];

                double b0 = b[0];
                double b1 = b[1];
                double b2 = b[2];
                double b3 = b[3];

                return ImageMatrix.Transform(sourceMatrix, (x, y) =>
                {
                    x = (int)(a0 * x + a1 * y + a2 * x * y + a3);
                    y = (int)(b0 * x + b1 * y + b2 * x * y + b3);
                    return (x, y);
                });
            }
            return sourceMatrix;
        }
    }
}