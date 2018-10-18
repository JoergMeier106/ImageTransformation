using System;

namespace Image_Transformation
{
    public struct Matrix
    {
        private ushort[,] _matrix;

        public Matrix(int height, int width, byte[] bytes)
        {
            Height = height;
            Width = width;
            _matrix = new ushort[Height, Width];

            CreateMatrix(bytes);
        }

        public int Height { get; }
        public int Width { get; }

        public ushort this[int y, int x]
        {
            get { return _matrix[y, x]; }
            set { _matrix[y, x] = value; }
        }

        public static Matrix Map(Matrix sourceMatrix, Matrix targetMatrix, Func<ushort, ushort> action)
        {
            for (int y = 0; y < sourceMatrix.Height; y++)
            {
                for (int x = 0; x < sourceMatrix.Width; x++)
                {
                    targetMatrix[y, x] = action(sourceMatrix[y, x]);
                }
            }
            return targetMatrix;
        }

        public static Matrix Map(Matrix sourceMatrix, Matrix targetMatrix, Func<int, int, ushort> action)
        {
            for (int y = 0; y < sourceMatrix.Height; y++)
            {
                for (int x = 0; x < sourceMatrix.Width; x++)
                {
                    targetMatrix[y, x] = action(x, y);
                }
            }
            return targetMatrix;
        }

        public static Matrix operator *(Matrix matrix, double value)
        {
            Map(matrix, matrix, (sourceValue) => (ushort)(Math.Min(sourceValue * value, ushort.MaxValue)));
            return matrix;
        }

        public static Matrix operator /(Matrix matrix, double value)
        {
            Map(matrix, matrix, ((sourceValue) => (ushort)(sourceValue / value)));
            return matrix;
        }

        public static Matrix operator +(Matrix matrix, double value)
        {
            Map(matrix, matrix, ((sourceValue) => (ushort)(sourceValue + value)));
            return matrix;
        }

        public static bool PointIsInBounds(int x, int y, int height, int width)
        {
            return y >= 0 && y < height && x >= 0 && x < width;
        }

        public static Matrix Shear(Matrix sourceMatrix, Matrix targetMatrix, int bx, int by)
        {
            return Transform(sourceMatrix, targetMatrix, (x, y) =>
            {
                x = x + bx * y;
                y = y + by * x;

                if (bx < 0)
                {
                    x += sourceMatrix.Width * Math.Abs(bx);
                }

                if (by < 0)
                {
                    y += sourceMatrix.Height * Math.Abs(by);
                }

                return (Math.Abs(x), Math.Abs(y));
            });
        }

        public static Matrix Shift(Matrix sourceMatrix, Matrix targetMatrix, int dx, int dy)
        {
            return Transform(sourceMatrix, targetMatrix, (x, y) =>
            {
                x = x + dx;
                y = y + dy;
                return (x, y);
            });
        }

        public static Matrix Transform(Matrix sourceMatrix, Matrix targetMatrix, Func<int, int, (int x, int y)> transformFunction)
        {
            for (int y = 0; y < targetMatrix.Height; y++)
            {
                for (int x = 0; x < targetMatrix.Width; x++)
                {
                    if (PointIsInBounds(x, y, sourceMatrix.Height, sourceMatrix.Width))
                    {
                        ushort targetValue = sourceMatrix[y, x];
                        var targetPoint = transformFunction(x, y);

                        if (PointIsInBounds(targetPoint.x, targetPoint.y, targetMatrix.Height, targetMatrix.Width))
                        {
                            targetMatrix[targetPoint.y, targetPoint.x] = targetValue;
                        }
                    }
                }
            }
            return targetMatrix;
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[Height * Width * 2];
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int targetIndex = ConvertXYToIndex(column * 2, row, Width * 2);
                    byte[] targetBytes = BitConverter.GetBytes(_matrix[row, column]);
                    bytes[targetIndex] = targetBytes[0];
                    bytes[targetIndex + 1] = targetBytes[1];
                }
            }
            return bytes;
        }

        private int ConvertIndexToX(int index, int width)
        {
            return index % width;
        }

        private int ConvertIndexToY(int index, int width)
        {
            return (int)Math.Floor((double)index / width);
        }

        private int ConvertXYToIndex(int x, int y, int width)
        {
            return x + y * width;
        }

        private void CreateMatrix(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i += 2)
            {
                int x = ConvertIndexToX(i / 2, Width);
                int y = ConvertIndexToY(i, Width * 2);

                _matrix[y, x] = BitConverter.ToUInt16(bytes, i);
            }
        }
    }
}