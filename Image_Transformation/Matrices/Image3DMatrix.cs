using System;
using System.Threading;
using System.Threading.Tasks;

namespace Image_Transformation
{
    public class Image3DMatrix
    {
        private const int MAX_Depth = 64;
        private const int MAX_HEIGHT = 8192;
        private const int MAX_WIDTH = 8192;
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
            Depth = Math.Min(depth, MAX_Depth);
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
            for (int z = 0; z < sourceMatrix.Depth; z++)
            {
                for (int y = 0; y < sourceMatrix.Height; y++)
                {
                    for (int x = 0; x < sourceMatrix.Width; x++)
                    {
                        ushort targetValue = sourceMatrix[z, y, x];
                        var (x_, y_, z_) = transformFunction(x, y, z);

                        if (PointIsInBounds(x_, y_, z_, targetMatrix.Height, targetMatrix.Width, targetMatrix.Depth))
                        {
                            targetMatrix[z_, y_, x_] = targetValue;
                        }
                    }
                }
            }
            return targetMatrix;
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