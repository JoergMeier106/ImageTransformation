using System;

namespace Image_Transformation
{
    public sealed class Matrix
    {
        private ushort[,] _matrix;

        public Matrix(int height, int width, byte[] bytes)
        {
            Height = height;
            Width = width;

            CreateMatrix(bytes);
        }

        public int Height { get; }
        public int Width { get; }

        public ushort this[int i, int j]
        {
            get { return _matrix[i, j]; }
            set { _matrix[i, j] = value; }
        }

        public static Matrix operator *(Matrix matrix, double value)
        {
            matrix.Map((sourceValue) => (ushort)(Math.Min(sourceValue * value, ushort.MaxValue)));
            return matrix;
        }

        public static Matrix operator /(Matrix matrix, double value)
        {
            matrix.Map((sourceValue) => (ushort)(sourceValue / value));
            return matrix;
        }

        public static Matrix operator +(Matrix matrix, double value)
        {
            matrix.Map((sourceValue) => (ushort)(sourceValue + value));
            return matrix;
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

        public void Map(Func<ushort, ushort> action)
        {
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    _matrix[row, column] = action(_matrix[row, column]);
                }
            }
        }

        public Matrix Shift(int dx, int dy)
        {
            Matrix shiftedMatrix = new Matrix(Height, Width, GetBytes());

            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    ushort targetValue = this[row, column];

                    int targetRow = row + dx;
                    int targetColumn = row + dy;

                    if (targetRow >= 0 && targetRow < Height &&
                        targetColumn >= 0 && targetColumn < Width)
                    {
                        shiftedMatrix[targetRow, targetColumn] = targetValue;
                    }
                }
            }
            return shiftedMatrix;
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
            _matrix = new ushort[Height, Width];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                int x = ConvertIndexToX(i / 2, Width);
                int y = ConvertIndexToY(i, Width * 2);

                _matrix[y, x] = BitConverter.ToUInt16(bytes, i);
            }

            //for (int row = 0; row < Height; row++)
            //{
            //    for (int column = 0; column < Width; column++)
            //    {
            //        int targetIndex = ConvertXYToIndex(column, row, Width * 2);
            //        _matrix[row, column] = BitConverter.ToUInt16(bytes, targetIndex);
            //    }
            //}
        }
    }
}