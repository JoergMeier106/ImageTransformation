using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Image_Transformation
{
    public class Image3DMatrix
    {
        private const int MAX_HEIGHT = 8192;
        private const int MAX_WIDTH = 8192;
        private const int MAX_DEPTH = 128;
        private readonly ushort[,,] _matrix;

        private CancellationTokenSource _tokenSource;

        public Image3DMatrix(int height, int width, int bytePerPixel, byte[] bytes)
        {
            Height = Math.Min(height, MAX_HEIGHT);
            Width = width;
            BytePerPixel = bytePerPixel;
            Depth = bytes.Length / (Width * Height * BytePerPixel);
            _matrix = new ushort[Depth, Height, Width];
            _tokenSource = new CancellationTokenSource();

            CreateMatrix(bytes);
        }

        public Image3DMatrix(int height, int width, int depth, int bytePerPixel)
        {
            Height = Math.Min(height, MAX_HEIGHT);
            Width = Math.Min(width, MAX_WIDTH);
            Depth = Math.Min(depth, MAX_DEPTH);
            _matrix = new ushort[Depth, Height, Width];
            BytePerPixel = bytePerPixel;
            _tokenSource = new CancellationTokenSource();
        }

        public int BytePerPixel { get; private set; }
        public int Depth { get; }
        public int Height { get; }
        public int Width { get; }

        public ushort this[int z, int y, int x]
        {
            get { return _matrix[z, y, x]; }
            set { _matrix[z, y, x] = value; }
        }

        public static Image3DMatrix Map(Image3DMatrix sourceMatrix, Image3DMatrix targetMatrix, Func<ushort, ushort> action)
        {
            return Map(sourceMatrix, targetMatrix, (x, y, z) => action(sourceMatrix[z, y, x]));
        }

        public static Image3DMatrix Map(Image3DMatrix sourceMatrix, Image3DMatrix targetMatrix, Func<int, int, int, ushort> action)
        {
            for (int z = 0; z < sourceMatrix.Depth; z++)
            {
                for (int y = 0; y < sourceMatrix.Height; y++)
                {
                    for (int x = 0; x < sourceMatrix.Width; x++)
                    {
                        targetMatrix[z, y, x] = action(x, y, z);
                    }
                }
            }
            return targetMatrix;
        }

        public static Image3DMatrix operator *(Image3DMatrix matrix, double value)
        {
            return Map(matrix, matrix, (sourceValue) => (ushort)(Math.Min(sourceValue * value, ushort.MaxValue)));
        }

        public static bool PointIsInBounds(int x, int y, int z, int height, int width, int depth)
        {
            return y >= 0 && y < height && x >= 0 && x < width && z >= 0 && z < depth;
        }

        public static Image3DMatrix Transform(Image3DMatrix sourceMatrix, Func<int, int, int, (int X, int Y, int Z)> transformFunction)
        {
            Image3DMatrix targetMatrix = new Image3DMatrix(sourceMatrix.Height, sourceMatrix.Width, sourceMatrix.Depth, sourceMatrix.BytePerPixel);
            return Transform(sourceMatrix, targetMatrix, transformFunction);
        }

        public static Image3DMatrix Transform(Image3DMatrix sourceMatrix, TransformationMatrix transformationMatrix)
        {
            return Transform(sourceMatrix, (x, y, z) =>
            {
                return ApplyTransformationMatrix(x, y, z, transformationMatrix);
            });
        }

        public static Image3DMatrix Transform(Image3DMatrix sourceMatrix, Image3DMatrix imageMatrix, TransformationMatrix transformationMatrix)
        {
            return Transform(sourceMatrix, imageMatrix, (x, y, z) =>
            {
                return ApplyTransformationMatrix(x, y, z, transformationMatrix);
            });
        }

        public static Image3DMatrix Transform(Image3DMatrix sourceMatrix, Image3DMatrix targetMatrix,
            Func<int, int, int, (int X, int Y_, int Z_)> transformFunction)
        {
            SizeInfo size = GetSizeAfterTransformation(sourceMatrix, transformFunction);

            targetMatrix = new Image3DMatrix(size.Height, size.Width, size.Depth, sourceMatrix.BytePerPixel);

            for (int z = 0; z < sourceMatrix.Depth; z++)
            {
                for (int y = 0; y < sourceMatrix.Height; y++)
                {
                    for (int x = 0; x < sourceMatrix.Width; x++)
                    {
                        ushort targetValue = sourceMatrix[z, y, x];
                        var (x_, y_, z_) = transformFunction(x, y, z);

                        int shiftedX = x_ + Math.Abs(size.SmallestX);
                        int shiftedY = y_ + Math.Abs(size.SmallestY);
                        int shiftedZ = z_ + Math.Abs(size.SmallestZ);

                        if (PointIsInBounds(shiftedX, shiftedY, shiftedZ, size.Height, size.Width, size.Depth))
                        {
                            targetMatrix[shiftedZ, shiftedY, shiftedX] = targetValue;
                        }
                    }
                }
            }
            return targetMatrix;
        }

        private static SizeInfo GetSizeAfterTransformation(Image3DMatrix sourceMatrix,
            Func<int, int, int, (int X_, int Y_, int Z_)> transformFunction)
        {
            SizeInfo size = new SizeInfo();

            size.SmallestX = int.MaxValue;
            size.SmallestY = int.MaxValue;
            size.SmallestZ = int.MaxValue;

            size.BiggestX = int.MinValue;
            size.BiggestY = int.MinValue;
            size.BiggestZ = int.MinValue;

            for (int z = 0; z < sourceMatrix.Depth; z++)
            {
                for (int y = 0; y < sourceMatrix.Height; y++)
                {
                    for (int x = 0; x < sourceMatrix.Width; x++)
                    {
                        var (X_, Y_, Z_) = transformFunction(x, y, z);
                        size.SmallestX = Math.Min(size.SmallestX, X_);
                        size.SmallestY = Math.Min(size.SmallestY, Y_);
                        size.SmallestZ = Math.Min(size.SmallestZ, Z_);

                        size.BiggestX = Math.Max(size.BiggestX, X_);
                        size.BiggestY = Math.Max(size.BiggestY, Y_);
                        size.BiggestZ = Math.Max(size.BiggestZ, Z_);
                    }
                }
            }
            size.Height = Math.Min(Math.Abs(size.BiggestY) + Math.Abs(size.SmallestY) + 1, MAX_HEIGHT);
            size.Width = Math.Min(Math.Abs(size.BiggestX) + Math.Abs(size.SmallestX) + 1, MAX_WIDTH);
            size.Depth = Math.Min(Math.Abs(size.BiggestZ) + Math.Abs(size.SmallestX) + 1, MAX_DEPTH);

            return size;
        }

        public static Image3DMatrix TransformTargetToSource(Image3DMatrix sourceMatrix, Image3DMatrix imageMatrix,
                            TransformationMatrix transformationMatrix)
        {
            return TransformTargetToSource(sourceMatrix, imageMatrix, (x, y, z) =>
            {
                return ApplyTransformationMatrix(x, y, z, transformationMatrix);
            });
        }

        public static Image3DMatrix TransformTargetToSource(Image3DMatrix sourceMatrix, Image3DMatrix targetMatrix,
            Func<int, int, int, (int x, int y, int z)> transformFunction)
        {
            sourceMatrix.CancelParallelTransformation();
            ParallelOptions options = CreateParallelOptions(sourceMatrix);

            try
            {
                Parallel.For(0, targetMatrix.Depth, options, (z) =>
                {
                    try
                    {
                        Parallel.For(0, targetMatrix.Height, options, (y) =>
                        {
                            try
                            {
                                Parallel.For(0, targetMatrix.Width, options, (x) =>
                                {
                                    var (x_, y_, z_) = transformFunction(x, y, z);

                                    if (PointIsInBounds(x_, y_, z_, sourceMatrix.Height, sourceMatrix.Width, sourceMatrix.Depth))
                                    {
                                        targetMatrix[z, y, x] = sourceMatrix[z_, y_, x_];
                                    }
                                });
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine("Operation Cancelled");
                            }
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Operation Cancelled");
                    }
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

        public byte[] GetBytes(int layer)
        {
            byte[] bytes = new byte[Height * Width * BytePerPixel];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int targetIndex = GetByteOffset(x, y);
                    byte[] targetBytes = BitConverter.GetBytes(_matrix[layer, y, x]);

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

        private static (int X, int Y, int Z) ApplyTransformationMatrix(int x, int y, int z, TransformationMatrix transformationMatrix)
        {
            TransformationMatrix sourcePoint = ConvertToHomogeneousMatrix(x, y, z);
            TransformationMatrix transformedMatrix = transformationMatrix * sourcePoint;

            double w = transformedMatrix[3, 0];
            double x_ = transformedMatrix[0, 0] / w;
            double y_ = transformedMatrix[1, 0] / w;
            double z_ = transformedMatrix[2, 0] / w;
            return ((int)x_, (int)y_, (int)z_);
        }

        private static TransformationMatrix ConvertToHomogeneousMatrix(int x, int y, int z)
        {
            return new TransformationMatrix(new double[,]
            {
                { x },
                { y },
                { z },
                { 1 }
            });
        }

        private static Image3DMatrix CreateNewSizedMatrix(Dictionary<(int x, int y, int z), ushort> transformedPoints, int bytePerPixel)
        {
            int smallestX = transformedPoints.Select(point => point.Key.x).Min();
            int smallestY = transformedPoints.Select(point => point.Key.y).Min();
            int smallestZ = transformedPoints.Select(point => point.Key.z).Min();

            int biggestX = transformedPoints.Select(point => point.Key.x).Max();
            int biggestY = transformedPoints.Select(point => point.Key.y).Max();
            int biggestZ = transformedPoints.Select(point => point.Key.z).Max();

            int newHeight = Math.Min(Math.Abs(biggestY) + Math.Abs(smallestY) + 1, MAX_HEIGHT);
            int newWidth = Math.Min(Math.Abs(biggestX) + Math.Abs(smallestX) + 1, MAX_WIDTH);
            int newDepth = Math.Min(Math.Abs(biggestZ) + Math.Abs(smallestZ) + 1, MAX_DEPTH);

            Image3DMatrix transformedMatrix = new Image3DMatrix(newHeight, newWidth, newDepth, bytePerPixel);

            foreach (var (x, y, z) in transformedPoints.Keys)
            {
                int shiftedX = x + Math.Abs(smallestX);
                int shiftedY = y + Math.Abs(smallestY);
                int shiftedZ = z + Math.Abs(smallestZ);

                if (PointIsInBounds(shiftedX, shiftedY, shiftedZ, newHeight, newWidth, newDepth))
                {
                    transformedMatrix[shiftedZ, shiftedY, shiftedX] = transformedPoints[(x, y, z)];
                }
            }

            return transformedMatrix;
        }

        private static ParallelOptions CreateParallelOptions(Image3DMatrix matrix)
        {
            return new ParallelOptions
            {
                CancellationToken = matrix._tokenSource.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
        }

        private void CreateMatrix(byte[] bytes)
        {
            for (int z = 0; z < Depth; z++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = GetByteOffset(z, y, x);

                        if (BytePerPixel == 2)
                        {
                            _matrix[z, y, x] = BitConverter.ToUInt16(bytes, offset);
                        }
                        else if (BytePerPixel == 1)
                        {
                            _matrix[z, y, x] = bytes[offset];
                        }
                    }
                }
            }
        }

        private int GetByteOffset(int x, int y) => (x + y * Width) * BytePerPixel;

        private int GetByteOffset(int z, int y, int x) => (x + y * Width + z * Height * Width) * BytePerPixel;
    }
}