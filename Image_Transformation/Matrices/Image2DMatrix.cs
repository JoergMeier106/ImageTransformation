using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Image_Transformation
{
    /// <summary>
    /// Abstractions for a 2D Image (One or two Byte greyscale only). Provides methods for transformations.
    /// </summary>
    public class Image2DMatrix
    {
        //The max height and width is necessary to prevent OutOfMemoryExceptions because of too large images.
        private const int MAX_HEIGHT = 8192;
        private const int MAX_WIDTH = 8192;
        private readonly ushort[,] _matrix;

        private CancellationTokenSource _tokenSource;

        public Image2DMatrix(int height, int width, byte[] bytes) : this(height, width)
        {
            BytePerPixel = bytes.Length / (Height * Width);
            CreateMatrix(bytes);
        }

        public Image2DMatrix(int height, int width, int bytePerPixel) : this(height, width)
        {
            BytePerPixel = bytePerPixel;
            _tokenSource = new CancellationTokenSource();
        }

        private Image2DMatrix(int height, int width)
        {
            Height = Math.Min(height, MAX_HEIGHT);
            Width = Math.Min(width, MAX_WIDTH);
            _matrix = new ushort[Height, Width];
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

        /// <summary>
        /// Applies a function to every pixel of the image.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the original pixel values.</param>
        /// <param name="targetMatrix">The image where the new values will be stored.</param>
        /// <param name="action">This function gets the original pixel value and returns a new one.</param>
        /// <returns>The targetMatrix will be returned.</returns>
        public static Image2DMatrix Map(Image2DMatrix sourceMatrix, Image2DMatrix targetMatrix, Func<ushort, ushort> action)
        {
            return Map(sourceMatrix, targetMatrix, (x, y) => action(sourceMatrix[y, x]));
        }

        /// <summary>
        /// Applies a function to every pixel of the image.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the original pixel values.</param>
        /// <param name="targetMatrix">The image where the new values will be stored.</param>
        /// <param name="action">This function gets the position of the original pixel and returns a value for it.</param>
        /// /// <returns>The targetMatrix will be returned.</returns>
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

        /// <summary>
        /// Checks if x is between 0 and height and if y is between 0 and width.
        /// </summary>
        public static bool PointIsInBounds(int x, int y, int height, int width)
        {
            return y >= 0 && y < height && x >= 0 && x < width;
        }

        /// <summary>
        /// Transforms each pixel position from the source image to a new position.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the pixel values.</param>
        /// <param name="transformFunction">This function gets the original position and returns a new position.</param>
        public static Image2DMatrix Transform(Image2DMatrix sourceMatrix, Func<int, int, (int X, int Y)> transformFunction)
        {
            //Before returning a new matrix, the transformed points are collected. In the last step, these points will
            //be analyzed to receive the new height and width of the matrix after the transformation.
            Dictionary<(int X, int Y), ushort> transformedPoints = new Dictionary<(int X, int Y), ushort>();

            for (int y = 0; y < sourceMatrix.Height; y++)
            {
                for (int x = 0; x < sourceMatrix.Width; x++)
                {
                    ushort targetValue = sourceMatrix[y, x];

                    var targetPoint = transformFunction(x, y);
                    transformedPoints[targetPoint] = targetValue;
                }
            }

            return CreateNewSizedMatrix(transformedPoints, sourceMatrix.BytePerPixel);
        }

        /// <summary>
        /// Transforms each pixel position from the source image to a new position.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the pixel values.</param>
        /// <param name="transformationMatrix">This matrix will be applied to the original positions to get a new position.</param>
        public static Image2DMatrix Transform(Image2DMatrix sourceMatrix, TransformationMatrix transformationMatrix)
        {
            return Transform(sourceMatrix, (x, y) =>
            {
                return ApplyTransformationMatrix(x, y, transformationMatrix);
            });
        }

        /// <summary>
        /// Transforms each pixel position from the source image to a new position.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the pixel values.</param>
        /// <param name="targetMatrix">The image to which the transformed pixel are written to.</param>
        /// <param name="transformationMatrix">This matrix will be applied to the original positions to get a new position.</param>
        public static Image2DMatrix Transform(Image2DMatrix sourceMatrix, Image2DMatrix targetMatrix, TransformationMatrix transformationMatrix)
        {
            return Transform(sourceMatrix, targetMatrix, (x, y) =>
            {
                return ApplyTransformationMatrix(x, y, transformationMatrix);
            });
        }

        /// <summary>
        /// Transforms each pixel position from the source image to a new position.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the pixel values.</param>
        /// <param name="targetMatrix">The image to which the transformed pixel are written to.</param>
        /// <param name="transformFunction">This function gets the original position and returns a new position.</param>
        public static Image2DMatrix Transform(Image2DMatrix sourceMatrix, Image2DMatrix targetMatrix, Func<int, int, (int X, int Y)> transformFunction)
        {
            for (int y = 0; y < sourceMatrix.Height; y++)
            {
                for (int x = 0; x < sourceMatrix.Width; x++)
                {
                    ushort targetValue = sourceMatrix[y, x];
                    var (X_, Y_) = transformFunction(x, y);

                    if (PointIsInBounds(X_, Y_, targetMatrix.Height, targetMatrix.Width))
                    {
                        targetMatrix[Y_, X_] = targetValue;
                    }
                }
            }
            return targetMatrix;
        }

        /// <summary>
        /// Transforms each pixel position from the target image to a new position.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the pixel values.</param>
        /// <param name="targetMatrix">The image to which the transformed pixel are written to.</param>
        /// <param name="transformationMatrix">This matrix will be applied to the original positions to get a new position.</param>
        /// <returns></returns>
        public static Image2DMatrix TransformTargetToSource(Image2DMatrix sourceMatrix, Image2DMatrix targetMatrix,
                            TransformationMatrix transformationMatrix)
        {
            return TransformTargetToSource(sourceMatrix, targetMatrix, (x, y) =>
            {
                return ApplyTransformationMatrix(x, y, transformationMatrix);
            });
        }

        /// <summary>
        /// Transforms each pixel position from the target image to a new position.
        /// </summary>
        /// <param name="sourceMatrix">The image which will provide the pixel values.</param>
        /// <param name="targetMatrix">The image to which the transformed pixel are written to.</param>
        /// <param name="transformFunction">This function gets the original position and returns a new position.</param>
        public static Image2DMatrix TransformTargetToSource(Image2DMatrix sourceMatrix, Image2DMatrix targetMatrix,
            Func<int, int, (int x, int y)> transformFunction)
        {
            sourceMatrix.CancelParallelTransformation();
            ParallelOptions options = CreateParallelOptions(sourceMatrix);

            try
            {
                //Due to using target to source exactly one value will be asigned for each position, so it is save
                //to run the process with parallel for loops.
                Parallel.For(0, targetMatrix.Height, options, (y) =>
                {
                    Parallel.For(0, targetMatrix.Width, options, (x) =>
                    {
                        var (X_, Y_) = transformFunction(x, y);

                        if (PointIsInBounds(X_, Y_, sourceMatrix.Height, sourceMatrix.Width))
                        {
                            targetMatrix[y, x] = sourceMatrix[Y_, X_];
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

        /// <summary>
        /// Converts the matrix back to bytes.
        /// </summary>
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[Height * Width * BytePerPixel];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int targetIndex = GetByteOffset(x, y);
                    byte[] targetBytes = BitConverter.GetBytes(_matrix[y, x]);

                    //The higher byte is only relevant for images with two bytes per pixel
                    if (BytePerPixel == 2)
                    {
                        bytes[targetIndex + 1] = targetBytes[1];
                    }
                    //Due to operations it is possible that the pixel value gets greater than one byte.
                    //This is why the value will be set to max value in this is case and any higher bits will be omitted.
                    else if (BytePerPixel == 1 && targetBytes[1] > 0) 
                    {
                        targetBytes[0] = byte.MaxValue;
                    }
                    bytes[targetIndex] = targetBytes[0];
                }
            }
            return bytes;
        }

        private static (int X_, int Y_) ApplyTransformationMatrix(int x, int y, TransformationMatrix transformationMatrix)
        {
            TransformationMatrix homogeneousMatrix = ConvertToHomogeneousMatrix(x, y);
            TransformationMatrix transformedMatrix = transformationMatrix * homogeneousMatrix;

            double w = transformedMatrix[2, 0];
            double x_ = transformedMatrix[0, 0] / w;
            double y_ = transformedMatrix[1, 0] / w;

            return ((int)x_, (int)y_);
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

        private static Image2DMatrix CreateNewSizedMatrix(Dictionary<(int x, int y), ushort> transformedPoints, int bytePerPixel)
        {
            int smallestX = transformedPoints.Select(point => point.Key.x).Min();
            int smallestY = transformedPoints.Select(point => point.Key.y).Min();

            int biggestX = transformedPoints.Select(point => point.Key.x).Max();
            int biggestY = transformedPoints.Select(point => point.Key.y).Max();

            int newHeight = Math.Min(Math.Abs(biggestY) + Math.Abs(smallestY) + 1, MAX_HEIGHT);
            int newWidth = Math.Min(Math.Abs(biggestX) + Math.Abs(smallestX) + 1, MAX_WIDTH);

            Image2DMatrix transformedMatrix = new Image2DMatrix(newHeight, newWidth, bytePerPixel);

            foreach (var (x, y) in transformedPoints.Keys)
            {
                //This step is necessary to shift all negative points to the positive range.
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

        private void CancelParallelTransformation()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new CancellationTokenSource();
        }

        private void CreateMatrix(byte[] bytes)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int offset = GetByteOffset(x, y);

                    if (BytePerPixel == 2)
                    {
                        _matrix[y, x] = BitConverter.ToUInt16(bytes, offset);
                    }
                    else if (BytePerPixel == 1)
                    {
                        _matrix[y, x] = bytes[offset];
                    }
                }
            }
        }

        private int GetByteOffset(int x, int y) => (x + y * Width) * BytePerPixel;
    }
}