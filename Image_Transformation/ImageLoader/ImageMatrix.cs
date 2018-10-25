﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Image_Transformation
{
    public class ImageMatrix
    {
        private const string ROTATING_KEY = "RotationOperation";
        private const string SCALING_KEY = "ScalingOperation";
        private const string SHEARING_KEY = "ShearingOperation";
        private readonly ushort[,] _matrix;
        private Dictionary<string, Func<int, int, (int x, int y)>> _imageTransformations;

        public ImageMatrix(int height, int width, byte[] bytes)
        {
            Height = height;
            Width = width;
            _matrix = new ushort[Height, Width];
            _imageTransformations = new Dictionary<string, Func<int, int, (int x, int y)>>();
            BytePerPixel = bytes.Length / (Height * Width);

            CreateMatrix(bytes);
        }

        public ImageMatrix(int height, int width, int bytePerPixel)
        {
            Height = height;
            Width = width;
            _matrix = new ushort[Height, Width];
            BytePerPixel = bytePerPixel;
            _imageTransformations = new Dictionary<string, Func<int, int, (int x, int y)>>();
        }

        public int BytePerPixel { get; private set; }
        public int Height { get; }

        public int Width { get; }

        public ushort this[int y, int x]
        {
            get { return _matrix[y, x]; }
            set { _matrix[y, x] = value; }
        }

        public static ImageMatrix Map(ImageMatrix sourceMatrix, ImageMatrix targetMatrix, Func<ushort, ushort> action)
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

        public static ImageMatrix Map(ImageMatrix sourceMatrix, ImageMatrix targetMatrix, Func<int, int, ushort> action)
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

        public static ImageMatrix operator *(ImageMatrix matrix, double value)
        {
            Map(matrix, matrix, (sourceValue) => (ushort)(Math.Min(sourceValue * value, ushort.MaxValue)));
            return matrix;
        }

        public static ImageMatrix operator /(ImageMatrix matrix, double value)
        {
            Map(matrix, matrix, ((sourceValue) => (ushort)(sourceValue / value)));
            return matrix;
        }

        public static ImageMatrix operator +(ImageMatrix matrix, double value)
        {
            Map(matrix, matrix, ((sourceValue) => (ushort)(sourceValue + value)));
            return matrix;
        }

        public static bool PointIsInBounds(int x, int y, int height, int width)
        {
            return y >= 0 && y < height && x >= 0 && x < width;
        }

        public static ImageMatrix Transform(ImageMatrix sourceMatrix, Func<int, int, (int x, int y)> transformFunction)
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

        public static ImageMatrix Transform(ImageMatrix sourceMatrix, TransformationMatrix transformationMatrix)
        {
            return Transform(sourceMatrix, (x, y) =>
            {
                TransformationMatrix homogeneousMatrix = ConvertToHomogeneousMatrix(x, y);
                TransformationMatrix transformedMatrix = transformationMatrix * homogeneousMatrix;
                return ((int)transformedMatrix[0, 0], (int)transformedMatrix[1, 0]);
            });
        }

        public static ImageMatrix Transform(ImageMatrix sourceMatrix, ImageMatrix imageMatrix, TransformationMatrix transformationMatrix)
        {
            return Transform(sourceMatrix, imageMatrix, (x, y) =>
            {
                TransformationMatrix homogeneousMatrix = ConvertToHomogeneousMatrix(x, y);
                TransformationMatrix transformedMatrix = transformationMatrix * homogeneousMatrix;
                return ((int)transformedMatrix[0, 0], (int)transformedMatrix[1, 0]);
            });
        }

        public static ImageMatrix Transform(ImageMatrix sourceMatrix, ImageMatrix targetMatrix, Func<int, int, (int x, int y)> transformFunction)
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

        public ImageMatrix ExecuteTransformations()
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
            byte[] bytes = new byte[Height * Width * BytePerPixel];
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    int targetIndex = ConvertXYToIndex(column * BytePerPixel, row, Width * BytePerPixel);
                    byte[] targetBytes = BitConverter.GetBytes(_matrix[row, column]);

                    if (BytePerPixel == 2)
                    {
                        bytes[targetIndex + 1] = targetBytes[1];
                    }
                    else if (targetBytes[1] > 0)
                    {
                        targetBytes[0] = byte.MaxValue;
                    }
                    bytes[targetIndex] = targetBytes[0];
                }
            }
            return bytes;
        }

        public ImageMatrix Rotate(double alpha)
        {
            ImageMatrix rotatedMatrix = Transform(this, new ImageMatrix(Height, Width, new byte[Height * Width * 2]), (x, y) =>
            {
                int xc = Width / 2;
                int yc = Height / 2;

                x = (int)(xc + (x - xc) * Math.Cos(alpha) - (y - yc) * Math.Sin(alpha));
                y = (int)(yc + (x - xc) * Math.Sin(alpha) + (y - yc) * Math.Cos(alpha));

                return (x, y);
            });
            rotatedMatrix._imageTransformations = _imageTransformations;
            return rotatedMatrix;
        }

        public ImageMatrix Scale(int sx, int sy)
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

        public ImageMatrix Shear(int bx, int by)
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

        public ImageMatrix Shift(int dx, int dy)
        {
            ImageMatrix shiftedMatrix = new ImageMatrix(Height, Width, new byte[Height * Width * 2]);
            shiftedMatrix._imageTransformations = _imageTransformations;

            return Transform(this, shiftedMatrix, (x, y) =>
            {
                x = x + dx;
                y = y + dy;
                return (x, y);
            });
        }

        private static TransformationMatrix ConvertToHomogeneousMatrix(int x, int y)
        {
            return new TransformationMatrix(new double[,]
            {
                    { x },
                    { y },
                    { 1 }
            });
        }

        private static ImageMatrix CreateNewSizedMatrix(Dictionary<(int x, int y), ushort> transformedPoints)
        {
            int smallestX = transformedPoints.Select(point => point.Key.x).Min();
            int smallestY = transformedPoints.Select(point => point.Key.y).Min();

            int biggestX = transformedPoints.Select(point => point.Key.x).Max();
            int biggestY = transformedPoints.Select(point => point.Key.y).Max();

            int newHeight = Math.Abs(biggestY) + Math.Abs(smallestY) + 1;
            int newWidth = Math.Abs(biggestX) + Math.Abs(smallestX) + 1;

            ImageMatrix transformedMatrix = new ImageMatrix(newHeight, newWidth, new byte[newHeight * newWidth * 2]);

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
            for (int i = 0; i < bytes.Length; i += BytePerPixel)
            {
                int x = ConvertIndexToX(i / BytePerPixel, Width);
                int y = ConvertIndexToY(i, Width * BytePerPixel);

                _matrix[y, x] = BitConverter.ToUInt16(bytes, i);
            }
        }
    }
}