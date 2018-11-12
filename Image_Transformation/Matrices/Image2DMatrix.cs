using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Image_Transformation
{
    public class Image2DMatrix
    {
        private const int MAX_HEIGHT = 8192;
        private const int MAX_WIDTH = 8192;
        private readonly ushort[,] _matrix;

        private CancellationTokenSource _tokenSource;

        public Image2DMatrix(int height, int width, byte[] bytes)
        {
            Height = height;
            Width = width;
            _matrix = new ushort[Height, Width];
            BytePerPixel = bytes.Length / (Height * Width);
            _tokenSource = new CancellationTokenSource();

            CreateMatrix(bytes);
        }

        public Image2DMatrix(int height, int width, int bytePerPixel)
        {
            Height = height;
            Width = width;
            _matrix = new ushort[Height, Width];
            BytePerPixel = bytePerPixel;
            _tokenSource = new CancellationTokenSource();
        }

        public int BytePerPixel { get; private set; }
        public int Height { get; }

        public int Width { get; }

        public ushort this[int y, int x]
        {
            get { return _matrix[y, x]; }
            set { _matrix[y, x] = value; }
        }

        public static Image2DMatrix Map(Image2DMatrix sourceMatrix, Image2DMatrix targetMatrix, Func<ushort, ushort> action)
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

        public static Image2DMatrix Map(Image2DMatrix sourceMatrix, Image2DMatrix targetMatrix, Func<int, int, ushort> action)
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

        public static Image2DMatrix operator *(Image2DMatrix matrix, double value)
        {
            return Map(matrix, matrix, (sourceValue) => (ushort)(Math.Min(sourceValue * value, ushort.MaxValue)));
        }

        public static Image2DMatrix operator /(Image2DMatrix matrix, double value)
        {
            return Map(matrix, matrix, ((sourceValue) => (ushort)(sourceValue / value)));
        }

        public static Image2DMatrix operator +(Image2DMatrix matrix, double value)
        {
            return Map(matrix, matrix, ((sourceValue) => (ushort)(sourceValue + value)));
        }

        public static bool PointIsInBounds(int x, int y, int height, int width)
        {
            return y >= 0 && y < height && x >= 0 && x < width;
        }

        public static Image2DMatrix Transform(Image2DMatrix sourceMatrix, Func<int, int, (int x, int y)> transformFunction)
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

            return CreateNewSizedMatrix(transformedPoints, sourceMatrix.BytePerPixel);
        }

        public static Image2DMatrix Transform(Image2DMatrix sourceMatrix, Transformation2DMatrix transformationMatrix)
        {
            return Transform(sourceMatrix, (x, y) =>
            {
                Transformation2DMatrix homogeneousMatrix = ConvertToHomogeneousMatrix(x, y);
                Transformation2DMatrix transformedMatrix = transformationMatrix * homogeneousMatrix;

                double z = transformedMatrix[2, 0];
                double x_ = transformedMatrix[0, 0] / z;
                double y_ = transformedMatrix[1, 0] / z;
                return ((int)x_, (int)y_);
            });
        }

        public static Image2DMatrix Transform(Image2DMatrix sourceMatrix, Image2DMatrix imageMatrix, Transformation2DMatrix transformationMatrix)
        {
            return Transform(sourceMatrix, imageMatrix, (x, y) =>
            {
                Transformation2DMatrix homogeneousMatrix = ConvertToHomogeneousMatrix(x, y);
                Transformation2DMatrix transformedMatrix = transformationMatrix * homogeneousMatrix;

                double z = transformedMatrix[2, 0];
                double x_ = transformedMatrix[0, 0] / z;
                double y_ = transformedMatrix[1, 0] / z;

                return ((int)x_, (int)y_);
            });
        }

        public static Image2DMatrix Transform(Image2DMatrix sourceMatrix, Image2DMatrix targetMatrix, Func<int, int, (int x, int y)> transformFunction)
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

        public static Image2DMatrix TransformTargetToSource(Image2DMatrix sourceMatrix, Image2DMatrix imageMatrix,
                            Transformation2DMatrix transformationMatrix)
        {
            return TransformTargetToSource(sourceMatrix, imageMatrix, (x, y) =>
            {
                Transformation2DMatrix homogeneousMatrix = ConvertToHomogeneousMatrix(x, y);
                Transformation2DMatrix transformedMatrix = transformationMatrix * homogeneousMatrix;

                double z = transformedMatrix[2, 0];
                double x_ = transformedMatrix[0, 0] / z;
                double y_ = transformedMatrix[1, 0] / z;

                return ((int)x_, (int)y_);
            });
        }

        public static Image2DMatrix TransformTargetToSource(Image2DMatrix sourceMatrix, Image2DMatrix targetMatrix,
            Func<int, int, (int x, int y)> transformFunction)
        {
            sourceMatrix.CancelParallelTransformation();
            ParallelOptions options = CreateParallelOptions(sourceMatrix);

            try
            {
                Parallel.For(0, targetMatrix.Height, options, (y) =>
                {
                    Parallel.For(0, targetMatrix.Width, options, (x) =>
                    {
                        var sourcePoint = transformFunction(x, y);

                        if (PointIsInBounds(sourcePoint.x, sourcePoint.y, sourceMatrix.Height, sourceMatrix.Width))
                        {
                            targetMatrix[y, x] = sourceMatrix[sourcePoint.y, sourcePoint.x];
                        }
                    });
                });
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation Cancelled");
            }
            return targetMatrix;
        }

        public void CancelParallelTransformation()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
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

        public Image2DMatrix Rotate(double alpha)
        {
            Image2DMatrix rotatedMatrix = Transform(this, new Image2DMatrix(Height, Width, BytePerPixel), (x, y) =>
            {
                int xc = Width / 2;
                int yc = Height / 2;

                x = (int)(xc + ((x - xc) * Math.Cos(alpha)) - ((y - yc) * Math.Sin(alpha)));
                y = (int)(yc + ((x - xc) * Math.Sin(alpha)) + ((y - yc) * Math.Cos(alpha)));

                return (x, y);
            });
            return rotatedMatrix;
        }

        public Image2DMatrix Scale(int sx, int sy)
        {
            Image2DMatrix scaledMatrix = Transform(this, new Image2DMatrix(Height, Width, new byte[Height * Width * 2]), (x, y) =>
            {
                x = x * sx;
                y = y * sy;
                return (x, y);
            });
            return scaledMatrix;
        }

        public Image2DMatrix Shear(int bx, int by)
        {
            Image2DMatrix shearedMatrix = Transform(this, new Image2DMatrix(Height, Width, new byte[Height * Width * 2]), (x, y) =>
            {
                x = x + bx * y;
                y = y + by * x;

                return (x, y);
            });
            return shearedMatrix;
        }

        public Image2DMatrix Shift(int dx, int dy)
        {
            return Transform(this, new Image2DMatrix(Height, Width, BytePerPixel), (x, y) =>
            {
                x = x + dx;
                y = y + dy;
                return (x, y);
            });
        }

        private static Transformation2DMatrix ConvertToHomogeneousMatrix(int x, int y)
        {
            return new Transformation2DMatrix(new double[,]
            {
                { x },
                { y },
                { 1 }
            });
        }

        private static Image2DMatrix CreateNewSizedMatrix(Dictionary<(int x, int y), ushort> transformedPoints, int bytePerPixel)
        {
            int smallestX = transformedPoints.Select(point => point.Key.x).Min();
            int smallestY = transformedPoints.Select(point => point.Key.y).Min();

            int biggestX = transformedPoints.Select(point => point.Key.x).Max();
            int biggestY = transformedPoints.Select(point => point.Key.y).Max();

            int newHeight = Math.Min(Math.Abs(biggestY) + Math.Abs(smallestY) + 1, MAX_HEIGHT);
            int newWidth = Math.Min(Math.Abs(biggestX) + Math.Abs(smallestX) + 1, MAX_WIDTH);

            Image2DMatrix transformedMatrix = new Image2DMatrix(newHeight, newWidth, new byte[newHeight * newWidth * bytePerPixel]);

            foreach (var (x, y) in transformedPoints.Keys)
            {
                int shiftedX = x + Math.Abs(smallestX);
                int shiftedY = y + Math.Abs(smallestY);

                if (PointIsInBounds(shiftedX, shiftedY, newHeight, newWidth))
                {
                    transformedMatrix[shiftedY, shiftedX] = transformedPoints[(x, y)];
                }
            }

            return transformedMatrix;
        }

        private static ParallelOptions CreateParallelOptions(Image2DMatrix matrix)
        {
            return new ParallelOptions
            {
                CancellationToken = matrix._tokenSource.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
        }

        private int ConvertIndexToX(int index, int width) => index % width;

        private int ConvertIndexToY(int index, int width) => (int)Math.Floor((double)index / width);

        private int ConvertXYToIndex(int x, int y, int width) => x + y * width;

        private void CreateMatrix(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i += BytePerPixel)
            {
                int x = ConvertIndexToX(i / BytePerPixel, Width);
                int y = ConvertIndexToY(i, Width * BytePerPixel);

                if (BytePerPixel == 2)
                {
                    _matrix[y, x] = BitConverter.ToUInt16(bytes, i);
                }
                else if (BytePerPixel == 1)
                {
                    _matrix[y, x] = bytes[i];
                }
            }
        }
    }
}