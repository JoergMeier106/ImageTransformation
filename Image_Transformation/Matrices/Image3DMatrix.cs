using System;
using System.Threading;
using System.Threading.Tasks;

namespace Image_Transformation
{
    /// <summary>
    /// Abstractions for a 3D Image (One or two Byte greyscale only). Provides methods for transformations.
    /// </summary>
    public class Image3DMatrix
    {
        //The max height, width and depth is necessary to prevent OutOfMemoryExceptions because of too large images.
        private const int MAX_DEPTH = 256;
        private const int MAX_HEIGHT = 8192;
        private const int MAX_WIDTH = 8192;
        private readonly ushort[,,] _matrix;

        private CancellationTokenSource _tokenSource;

        public Image3DMatrix(int height, int width, int bytePerPixel, byte[] bytes)
        {
            Height = Math.Min(height, MAX_HEIGHT);
            Width = Math.Min(width, MAX_WIDTH);
            BytePerPixel = bytePerPixel;
            Depth = Math.Min(bytes.Length / (Width * Height * BytePerPixel), MAX_DEPTH);
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

        /// <summary>
        /// Applies a function to every voxel of the image.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the original voxel values.</param>
        /// <param name="targetMatrix">The image where the new values will be stored.</param>
        /// <param name="action">This function gets the original voxel value and returns a new one.</param>
        /// <returns>The targetMatrix will be returned.</returns>
        public static Image3DMatrix Map(Image3DMatrix sourceMatrix, Image3DMatrix targetMatrix, Func<ushort, ushort> action)
        {
            return Map(sourceMatrix, targetMatrix, (x, y, z) => action(sourceMatrix[z, y, x]));
        }

        /// <summary>
        /// Applies a function to every voxel of the image.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the original voxel values.</param>
        /// <param name="targetMatrix">The image where the new values will be stored.</param>
        /// <param name="action">This function gets the position of the original voxel and returns a value for it.</param>
        /// /// <returns>The targetMatrix will be returned.</returns>
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

        /// <summary>
        /// Checks if x is between 0 and height and if y is between 0 and width.
        /// </summary>
        public static bool PointIsInBounds(int x, int y, int z, int height, int width, int depth)
        {
            return y >= 0 && y < height && x >= 0 && x < width && z >= 0 && z < depth;
        }

        /// <summary>
        /// Transforms each voxel position from the source image to a new position.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the voxel values.</param>
        /// <param name="transformationMatrix">This matrix will be applied to the original positions to get a new position.</param>
        /// <returns></returns>
        public static Image3DMatrix Transform(Image3DMatrix sourceMatrix, TransformationMatrix transformationMatrix)
        {
            sourceMatrix.CancelParallelTransformation();
            ParallelOptions options = CreateParallelOptions(sourceMatrix);

            SizeInfo size = GetSizeAfterTransformation(sourceMatrix, transformationMatrix);

            Image3DMatrix targetMatrix = new Image3DMatrix(size.Height, size.Width, size.Depth, sourceMatrix.BytePerPixel);
            TransformationMatrix invertedTransformationMatrix = transformationMatrix.Invert3D();

            try
            {
                //Due to using target to source exactly one value will be asigned for each position, so it is save
                //to run the process with parallel for loops.
                Parallel.For(0, targetMatrix.Depth, options, (z) =>
                {
                    Parallel.For(0, targetMatrix.Height, options, (y) =>
                    {
                        Parallel.For(0, targetMatrix.Width, options, (x) =>
                        {
                            var (x_, y_, z_) = ApplyTransformationMatrix(x, y, z, invertedTransformationMatrix);

                            int shiftedX = x_ + Math.Abs(size.SmallestX);
                            int shiftedY = y_ + Math.Abs(size.SmallestY);
                            int shiftedZ = z_ + Math.Abs(size.SmallestZ);

                            if (PointIsInBounds(shiftedX, shiftedY, shiftedZ, sourceMatrix.Height, sourceMatrix.Width, sourceMatrix.Depth))
                            {
                                targetMatrix[z, y, x] = sourceMatrix[shiftedZ, shiftedY, shiftedX]; ;
                            }
                        });
                    });
                });
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation Cancelled");
            }
            return targetMatrix;
        }

        /// <summary>
        /// Converts one layer of the matrix back to bytes.
        /// </summary>
        /// <param name="layer">The layer which should be converted.</param>
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

        private static SizeInfo GetSizeAfterTransformation(Image3DMatrix sourceMatrix, TransformationMatrix transformationMatrix)
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
                        var (X_, Y_, Z_) = ApplyTransformationMatrix(x, y, z, transformationMatrix);
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

        private void CancelParallelTransformation()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        private void CreateMatrix(byte[] bytes)
        {
            for (int z = 0; z < Depth; z++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int offset = GetByteOffset(x, y, z);

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

        private int GetByteOffset(int x, int y, int z) => (x + y * Width + z * Height * Width) * BytePerPixel;
    }
}