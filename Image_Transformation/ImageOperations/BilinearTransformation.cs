﻿using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Image_Transformation
{
    /// <summary>
    /// Executes a bilinear transformation on an Image2DMatrix.
    /// </summary>
    public class BilinearTransformation : IImage2DOperation
    {
        private readonly IImage2DLoader _imageLoader;

        public BilinearTransformation(IImage2DLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public Quadrilateral Quadrilateral { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged => _imageLoader.MatrixChanged;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;

        public Image2DMatrix GetImageMatrix()
        {
            Image2DMatrix sourceMatrix = _imageLoader.GetImageMatrix();

            if (Quadrilateral != null)
            {
                //Bilinear transformations have 8 Parameter. These parameter can be obtained
                //by solving two linear equation systems. This will be done in the following.
                Quadrilateral targetQuadrilateral = GetTargetQuadrilateral();

                Matrix<double> m = GetmMatrix();
                Vector<double> a = GetaVector(targetQuadrilateral, m);
                Vector<double> b = GetbVector(targetQuadrilateral, m);

                double a0 = a[0];
                double a1 = a[1];
                double a2 = a[2];
                double a3 = a[3];

                double b0 = b[0];
                double b1 = b[1];
                double b2 = b[2];
                double b3 = b[3];

                return Image2DMatrix.Transform(sourceMatrix, (x, y) =>
                {
                    x = (int)(a0 * x + a1 * y + a2 * x * y + a3);
                    y = (int)(b0 * x + b1 * y + b2 * x * y + b3);
                    return (x, y);
                });
            }
            return sourceMatrix;
        }

        private Matrix<double> GetmMatrix()
        {
            double x0 = Quadrilateral.X0;
            double x1 = Quadrilateral.X1;
            double x2 = Quadrilateral.X2;
            double x3 = Quadrilateral.X3;

            double y0 = Quadrilateral.Y0;
            double y1 = Quadrilateral.Y1;
            double y2 = Quadrilateral.Y2;
            double y3 = Quadrilateral.Y3;

            var m = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                    { x0, y0, x0*y0, 1 },
                    { x1, y1, x1*y1, 1 },
                    { x2, y2, x2*y2, 1 },
                    { x3, y3, x3*y3, 1 },
            });
            return m;
        }

        private static Vector<double> GetbVector(Quadrilateral targetQuadrilateral, Matrix<double> m)
        {
            double y0_ = targetQuadrilateral.Y0;
            double y1_ = targetQuadrilateral.Y1;
            double y2_ = targetQuadrilateral.Y2;
            double y3_ = targetQuadrilateral.Y3;

            var targetYVector = Vector<double>.Build.Dense(new double[] { y0_, y1_, y2_, y3_ });
            return m.Solve(targetYVector);
        }

        private static Vector<double> GetaVector(Quadrilateral targetQuadrilateral, Matrix<double> m)
        {
            double x0_ = targetQuadrilateral.X0;
            double x1_ = targetQuadrilateral.X1;
            double x2_ = targetQuadrilateral.X2;
            double x3_ = targetQuadrilateral.X3;

            var targetXVector = Vector<double>.Build.Dense(new double[] { x0_, x1_, x2_, x3_ });
            return m.Solve(targetXVector);
        }

        private Quadrilateral GetTargetQuadrilateral()
        {
            double targetWidth = Math.Max(Quadrilateral.X1 - Quadrilateral.X0,
                                                          Quadrilateral.X2 - Quadrilateral.X3);
            double targetHeight = Math.Max(Quadrilateral.Y2 - Quadrilateral.Y1,
                                           Quadrilateral.Y3 - Quadrilateral.Y0);

            Quadrilateral targetQuadrilateral = new Quadrilateral
            (
                new List<Point>
                {
                    new Point(0, 0),
                    new Point(0, targetHeight),
                    new Point(targetWidth, 0),
                    new Point(targetWidth, targetHeight)
                }
            );
            return targetQuadrilateral;
        }
    }
}