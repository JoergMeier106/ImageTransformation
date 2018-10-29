using System;
using System.Collections.Generic;
using System.Windows;

namespace Image_Transformation
{
    public struct TransformationMatrix
    {
        public static readonly TransformationMatrix UnitMatrix = new TransformationMatrix(new double[,]
        {
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 },
        });

        private readonly double[,] _matrix;

        public TransformationMatrix(double[,] matrix)
        {
            Height = matrix.GetLength(0);
            Width = matrix.GetLength(1);
            _matrix = matrix;
        }

        public TransformationMatrix(int height, int width)
        {
            Height = height;
            Width = width;
            _matrix = new double[height, width];
        }

        public int Height { get; }
        public int Width { get; }

        public double this[int y, int x]
        {
            get { return _matrix[y, x]; }
            private set { _matrix[y, x] = value; }
        }

        public static TransformationMatrix GetTransformationFromUnitSquare(Quadrilateral quadrilateral)
        {
            double a00 = GetA00(quadrilateral);
            double a01 = GetA01(quadrilateral);
            double a02 = GetA02(quadrilateral);
            double a10 = GetA10(quadrilateral);
            double a11 = GetA11(quadrilateral);
            double a12 = GetA12(quadrilateral);
            double a20 = GetA20(quadrilateral);
            double a21 = GetA21(quadrilateral);

            return new TransformationMatrix(new double[,]
            {
                { a00, a01, a02 },
                { a10, a11, a12 },
                { a20, a21, 1   }
            });
        }

        public static bool operator !=(TransformationMatrix matrix1, TransformationMatrix matrix2)
        {
            return !(matrix1 == matrix2);
        }

        public static TransformationMatrix operator *(TransformationMatrix leftMatrix, TransformationMatrix rightMatrix)
        {
            if (leftMatrix.Width != rightMatrix.Height)
            {
                throw new InvalidOperationException("The row count of the left matrix and the column count of the right matrix must be equal!");
            }

            TransformationMatrix matrix = new TransformationMatrix(leftMatrix.Height, rightMatrix.Width);

            for (int i = 0; i < leftMatrix.Height; i++)
            {
                for (int j = 0; j < rightMatrix.Width; j++)
                {
                    double value = 0;
                    for (int k = 0; k < leftMatrix.Width; k++)
                    {
                        value += leftMatrix[i, k] * rightMatrix[k, j];
                    }
                    matrix[i, j] = value;
                }
            }
            return matrix;
        }

        public static bool operator ==(TransformationMatrix matrix1, TransformationMatrix matrix2)
        {
            return EqualityComparer<TransformationMatrix>.Default.Equals(matrix1, matrix2);
        }

        public override bool Equals(object obj)
        {
            if (obj is TransformationMatrix matrix)
            {
                return MatrixContentIsEqual(matrix);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = -1054311227;
            hashCode = hashCode * -1521134295 + EqualityComparer<double[,]>.Default.GetHashCode(_matrix);
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            return hashCode;
        }

        public static TransformationMatrix Map(TransformationMatrix sourceMatrix, Func<double, double> action)
        {
            TransformationMatrix targetMatrix = new TransformationMatrix(sourceMatrix.Height, sourceMatrix.Width);
            for (int y = 0; y < sourceMatrix.Height; y++)
            {
                for (int x = 0; x < sourceMatrix.Width; x++)
                {
                    targetMatrix[y, x] = action(sourceMatrix[y, x]);
                }
            }
            return targetMatrix;
        }

        public static TransformationMatrix operator *(TransformationMatrix matrix, double value)
        {
            return Map(matrix, (sourceValue) => sourceValue * value);
        }

        public TransformationMatrix Invert()
        {
            double a00 = this[0, 0];
            double a01 = this[0, 1];
            double a02 = this[0, 2];
            double a10 = this[1, 0];
            double a11 = this[1, 1];
            double a12 = this[1, 2];
            double a20 = this[2, 0];
            double a21 = this[2, 1];
            double a22 = this[2, 2];

            double determinant = GetDeterminant();

            TransformationMatrix adjugateMatrix = new TransformationMatrix(new double[,]
            {
                { a11*a22-a12*a21, a02*a21-a01*a22, a01*a12-a02*a11 },
                { a12*a20-a10*a22, a00*a22-a02*a20, a02*a10-a00*a12 },
                { a10*a21-a11*a20, a01*a20-a00*a21, a00*a11-a01*a10 },
            });

            TransformationMatrix inversedMatrix = adjugateMatrix * (1 / determinant);

            return inversedMatrix;
        }

        private double GetDeterminant()
        {
            double a00 = this[0, 0];
            double a01 = this[0, 1];
            double a02 = this[0, 2];
            double a10 = this[1, 0];
            double a11 = this[1, 1];
            double a12 = this[1, 2];
            double a20 = this[2, 0];
            double a21 = this[2, 1];
            double a22 = this[2, 2];

            return a00 * a11 * a22 + a01 * a12 * a20 + a02 * a10 * a21 - a00 * a12 * a21 - a01 * a10 * a22 - a02 * a11 * a20;
        }

        public TransformationMatrix Rotate(double alpha, int xc, int yc)
        {
            TransformationMatrix shiftToOriginMatrix = new TransformationMatrix(new double[,]
            {
                { 1, 0,  xc },
                { 0, 1,  yc },
                { 0, 0,  1  },
            });
            TransformationMatrix rotationMatrix = new TransformationMatrix(new double[,]
            {
                { Math.Cos(alpha), -Math.Sin(alpha),  0 },
                { Math.Sin(alpha),  Math.Cos(alpha),  0 },
                { 0,                0,                1 }
            });
            TransformationMatrix shiftBackMatrix = new TransformationMatrix(new double[,]
            {
                { 1, 0,  -xc },
                { 0, 1,  -yc },
                { 0, 0,  1   }
            });
            return this * shiftToOriginMatrix * rotationMatrix * shiftBackMatrix;
        }

        public TransformationMatrix Scale(double sx, double sy)
        {
            TransformationMatrix scalingMatrix = new TransformationMatrix(new double[,]
            {
                { sx, 0,  0 },
                { 0,  sy, 0 },
                { 0,  0,  1 }
            });
            return this * scalingMatrix;
        }

        public TransformationMatrix Shear(int bx, int by)
        {
            TransformationMatrix shearingMatrix = new TransformationMatrix(new double[,]
            {
                { 1,  bx, 0 },
                { by, 1,  0 },
                { 0,  0,  1 }
            });
            return this * shearingMatrix;
        }

        public TransformationMatrix Shift(int dx, int dy)
        {
            TransformationMatrix shiftingMatrix = new TransformationMatrix(new double[,]
            {
                { 1, 0, dx },
                { 0, 1, dy },
                { 0, 0, 1  },
            });
            return this * shiftingMatrix;
        }

        private static double GetA00(Quadrilateral quadrilateral)
        {
            double x0 = quadrilateral.X0;
            double x1 = quadrilateral.X1;

            return x1 - x0 + GetA20(quadrilateral) * x1;
        }

        private static double GetA01(Quadrilateral quadrilateral)
        {
            double x0 = quadrilateral.X0;
            double x3 = quadrilateral.X3;

            return x3 - x0 + GetA21(quadrilateral) * x3;
        }

        private static double GetA02(Quadrilateral quadrilateral)
        {
            return quadrilateral.X0;
        }

        private static double GetA10(Quadrilateral quadrilateral)
        {
            double y0 = quadrilateral.Y0;
            double y1 = quadrilateral.Y1;

            return y1 - y0 + GetA20(quadrilateral) * y1;
        }

        private static double GetA11(Quadrilateral quadrilateral)
        {
            double y0 = quadrilateral.Y0;
            double y3 = quadrilateral.Y3;

            return y3 - y0 + GetA21(quadrilateral) * y3;
        }

        private static double GetA12(Quadrilateral quadrilateral)
        {
            return quadrilateral.Y0;
        }

        private static double GetA20(Quadrilateral quadrilateral)
        {
            double x0 = quadrilateral.X0;
            double x1 = quadrilateral.X1;
            double x2 = quadrilateral.X2;
            double x3 = quadrilateral.X3;

            double y0 = quadrilateral.Y0;
            double y1 = quadrilateral.Y1;
            double y2 = quadrilateral.Y2;
            double y3 = quadrilateral.Y3;

            return ((x0 - x1 + x2 - x3) * (y3 - y2) - (y0 - y1 + y2 - y3) * (x3 - x2)) /
                           ((x1 - x2) * (y3 - y2) - (x3 - x2) * (y1 - y2));
        }

        private static double GetA21(Quadrilateral quadrilateral)
        {
            double x0 = quadrilateral.X0;
            double x1 = quadrilateral.X1;
            double x2 = quadrilateral.X2;
            double x3 = quadrilateral.X3;

            double y0 = quadrilateral.Y0;
            double y1 = quadrilateral.Y1;
            double y2 = quadrilateral.Y2;
            double y3 = quadrilateral.Y3;

            return ((y0 - y1 + y2 - y3) * (x1 - x2) - (x0 - x1 + x2 - x3) * (y1 - y2)) /
                           ((x1 - x2) * (y3 - y2) - (x3 - x2) * (y1 - y2));
        }

        public TransformationMatrix Project(Quadrilateral sourceQuadrilateral)
        {
            if (sourceQuadrilateral != null)
            {
                double targetWidth = Math.Max(sourceQuadrilateral.X1 - sourceQuadrilateral.X0,
                                              sourceQuadrilateral.X2 - sourceQuadrilateral.X3);
                double targetHeight = Math.Max(sourceQuadrilateral.Y2 - sourceQuadrilateral.Y1,
                                              sourceQuadrilateral.Y3 - sourceQuadrilateral.Y0);

                Quadrilateral targetQuadrilateral = new Quadrilateral(new List<Point>
                {
                    new Point(0, 0),
                    new Point(0, targetHeight),
                    new Point(targetWidth, 0),
                    new Point(targetWidth, targetHeight)
                });

                TransformationMatrix unitSquareToSourceTransformation = GetTransformationFromUnitSquare(sourceQuadrilateral);
                TransformationMatrix sourceToUnitSquareTransformation = unitSquareToSourceTransformation.Invert();

                TransformationMatrix unitSquareToTargetTransformation = GetTransformationFromUnitSquare(targetQuadrilateral);

                TransformationMatrix projectiveMapping = unitSquareToTargetTransformation * sourceToUnitSquareTransformation;
                return this * projectiveMapping;
            }
            return this;
        }

        private bool MatrixContentIsEqual(TransformationMatrix matrix)
        {
            if (Height != matrix.Height || Width != matrix.Width)
            {
                return false;
            }

            bool contentIsEqual = true;
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    if (this[row, column] != matrix[row, column])
                    {
                        contentIsEqual = false;
                    }
                }
            }
            return contentIsEqual;
        }
    }
}