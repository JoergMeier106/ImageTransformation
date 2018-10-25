using System;
using System.Collections.Generic;

namespace Image_Transformation
{
    public sealed class TransformationMatrix
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
            _matrix = new double[Height, Width];
        }

        public int Height { get; }

        public int Width { get; }

        public double this[int y, int x]
        {
            get { return _matrix[y, x]; }
            private set { _matrix[y, x] = value; }
        }

        public static bool operator !=(TransformationMatrix matrix1, TransformationMatrix matrix2)
        {
            return !(matrix1 == matrix2);
        }

        public static TransformationMatrix GetRotationMatrix(double alpha, int xc, int yc)
        {
            TransformationMatrix shiftToOriginMatrix = new TransformationMatrix(new double[,]
            {
                { 1, 0,  xc  },
                { 0, 1,  yc },
                { 0, 0,  1          },
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
                { 0, 0,  1             },
            });
            return shiftToOriginMatrix * rotationMatrix * shiftBackMatrix;
        }

        public static TransformationMatrix GetScalingMatrix(int sx, int sy)
        {
            return new TransformationMatrix(new double[,]
            {
                { sx, 0,  0 },
                { 0,  sy, 0 },
                { 0,  0,  1 }
            });
        }

        public static TransformationMatrix GetShearingMatrix(int bx, int by)
        {
            return new TransformationMatrix(new double[,]
            {
                { 1,  bx, 0 },
                { by, 1,  0 },
                { 0,  0,  1 }
            });
        }

        public static TransformationMatrix GetShiftingMatrix(int dx, int dy)
        {
            return new TransformationMatrix(new double[,]
            {
                { 1, 0, dx },
                { 0, 1, dy },
                { 0, 0, 1  },
            });
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
            var matrix = obj as TransformationMatrix;
            return matrix != null &&
                   MatrixContentIsEqual(matrix);
        }

        public override int GetHashCode()
        {
            var hashCode = -1054311227;
            hashCode = hashCode * -1521134295 + EqualityComparer<double[,]>.Default.GetHashCode(_matrix);
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            return hashCode;
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