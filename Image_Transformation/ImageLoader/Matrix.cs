using System;
using System.Collections.Generic;
using System.Linq;

namespace Image_Transformation
{
    public class Matrix
    {
        private const string SHEARING_KEY = "ShearingOperation";
        private const string SCALING_KEY = "ScalingOperation";
        private const string ROTATING_KEY = "RotationOperation";

        private readonly ushort[,] _matrix;
        private Dictionary<string, Func<int, int, (int x, int y)>> _imageTransformations;

        public Matrix(int height, int width, byte[] bytes)
        {
            Height = height;
            Width = width;
            _matrix = new ushort[Height, Width];
            _imageTransformations = new Dictionary<string, Func<int, int, (int x, int y)>>();

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

        public static Matrix Transform(Matrix sourceMatrix, Func<int, int, (int x, int y)> transformFunction)
        {
            Dictionary<(int x, int y), ushort> transformedPoints = new Dictionary<(int x, int y), ushort>();

            for (int y = 0; y < sourceMatrix.Height; y++)
            {
                for (int x = 0; x < sourceMatrix.Width; x++)
                {
                    ushort targetValue = sourceMatrix[y, x];

                    var targetPoint = transformFunction(x, y);
                    transformedPoints[(targetPoint.x, targetPoint.y)] = targetValue;
                }
            }

            return CreateNewSizedMatrix(transformedPoints);
        }

        public static Matrix Transform(Matrix sourceMatrix, Matrix targetMatrix, Func<int, int, (int x, int y)> transformFunction)
        {
            for (int y = 0; y < sourceMatrix.Height; y++)
            {
                for (int x = 0; x < sourceMatrix.Width; x++)
                {
                    ushort targetValue = sourceMatrix[y, x];
                    var targetPoint = transformFunction(x, y);

                    if (PointIsInBounds(targetPoint.x, targetPoint.y, targetMatrix.Height, targetMatrix.Width))
                    {
                        targetMatrix[targetPoint.y, targetPoint.x] = targetValue;
                    }
                }
            }
            return targetMatrix;
        }

        public Matrix ExecuteTransformations()
        {
            if (_imageTransformations.Count > 0)
            {
                return Transform(this, (int x, int y) =>
                {
                    var point = (x, y);
                    foreach (var transformFunction in _imageTransformations.Values)
                    {
                        point = transformFunction(point.x, point.y);
                    }
                    return point;
                });
            }
            return this;
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

        public Matrix Rotate(double alpha)
        {
            return Transform(this, new Matrix(Height, Width, new byte[Height * Width * 2]), (x, y) =>
            {
                int xc = Width / 2;
                int yc = Height / 2;

                x = (int)(xc + (x - xc) * Math.Cos(alpha) - (y - yc) * Math.Sin(alpha));
                y = (int)(yc + (x - xc) * Math.Sin(alpha) + (y - yc) * Math.Cos(alpha));

                return (x, y);
            });

            if (alpha == 0)
            {
                _imageTransformations.Remove(ROTATING_KEY);
            }
            else
            {
                _imageTransformations[ROTATING_KEY] = (x, y) =>
                {
                    int xc = Width;
                    int yc = Height;

                    //x = x - xc;
                    //y = y - yc;

                    x = (int)(/*xc + (x - xc)*/x * Math.Cos(alpha) - /*(y - yc)*/y * Math.Sin(alpha));
                    y = (int)(/*yc + (x - xc)*/x * Math.Sin(alpha) + /*(y - yc)*/y * Math.Cos(alpha));

                    //x = x + xc;
                    //y = y + yc;

                    return (x, y);
                };
            }

            return this;
        }

        public Matrix Scale(int sx, int sy)
        {
            if (Math.Abs(sx) == 1 && Math.Abs(sy) == 1)
            {
                _imageTransformations.Remove(SCALING_KEY);
            }
            else
            {
                _imageTransformations[SCALING_KEY] = (x, y) =>
                {
                    x = x * sx;
                    y = y * sy;
                    return (x, y);
                };
            }

            return this;
        }

        public Matrix Shear(int bx, int by)
        {
            if (bx == 0 && by == 0)
            {
                _imageTransformations.Remove(SHEARING_KEY);
            }
            else
            {
                _imageTransformations[SHEARING_KEY] = (x, y) =>
                {
                    x = x + bx * y;
                    y = y + by * x;

                    return (x, y);
                };
            }

            return this;
        }

        public Matrix Shift(int dx, int dy)
        {
            Matrix shiftedMatrix = new Matrix(Height, Width, new byte[Height * Width * 2]);
            shiftedMatrix._imageTransformations = _imageTransformations;

            return Transform(this, shiftedMatrix, (x, y) =>
            {
                x = x + dx;
                y = y + dy;
                return (x, y);
            });
        }

        private static Matrix CreateNewSizedMatrix(Dictionary<(int x, int y), ushort> transformedPoints)
        {
            int smallestX = transformedPoints.Select(point => point.Key.x).Min();
            int smallestY = transformedPoints.Select(point => point.Key.y).Min();

            int biggestX = transformedPoints.Select(point => point.Key.x).Max();
            int biggestY = transformedPoints.Select(point => point.Key.y).Max();

            int newHeight = Math.Abs(biggestY) + Math.Abs(smallestY) + 1;
            int newWidth = Math.Abs(biggestX) + Math.Abs(smallestX) + 1;

            Matrix transformedMatrix = new Matrix(newHeight, newWidth, new byte[newHeight * newWidth * 2]);

            foreach (var (x, y) in transformedPoints.Keys)
            {
                transformedMatrix[y + Math.Abs(smallestY), x + Math.Abs(smallestX)] = transformedPoints[(x, y)];
            }

            return transformedMatrix;
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