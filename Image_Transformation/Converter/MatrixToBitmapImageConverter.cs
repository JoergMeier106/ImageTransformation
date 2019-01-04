using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    /// <summary>
    /// Creates BitmapImages out of Image2DMatrix or Image3Dmatrix.
    /// </summary>
    public static class MatrixToBitmapImageConverter
    {
        public static WriteableBitmap GetImage(Image2DMatrix imageMatrix)
        {
            if (imageMatrix != null)
            {
                int height = imageMatrix.Height;
                int width = imageMatrix.Width;
                int bitsPerPixel = 8 * imageMatrix.BytePerPixel;
                byte[] imageBytes = imageMatrix.GetBytes();

                WriteableBitmap bitmap = CreateWriteableBitmap(height, width, bitsPerPixel, imageBytes);
                //Must be freezed to share it between threads.
                bitmap.Freeze();

                return bitmap;
            }
            return null;
        }

        public static IEnumerable<WriteableBitmap> GetImages(Image3DMatrix imageMatrix)
        {
            if (imageMatrix != null)
            {
                int height = imageMatrix.Height;
                int width = imageMatrix.Width;
                int depth = imageMatrix.Depth;
                int bitsPerPixel = 8 * imageMatrix.BytePerPixel;

                WriteableBitmap[] bitmaps = new WriteableBitmap[depth];

                for (int layer = 0; layer < depth; layer++)
                {
                    byte[] imageBytes = imageMatrix.GetBytes(layer);
                    WriteableBitmap bitmap = CreateWriteableBitmap(height, width, bitsPerPixel, imageBytes);
                    //Must be freezed to share it between threads.
                    bitmap.Freeze();
                    bitmaps[layer] = bitmap;
                }

                return bitmaps;
            }
            return Array.Empty<WriteableBitmap>();
        }

        private static WriteableBitmap CreateWriteableBitmap(int height, int width, int bitsPerPixel, byte[] imageBytes)
        {
            int stride = (width * bitsPerPixel + 7) / 8;

            //Only grayscale images with 16 or 8 Bit per pixel are supported.
            PixelFormat pixelFormat = bitsPerPixel == 16 ? PixelFormats.Gray16 : PixelFormats.Gray8;
            BitmapSource bitmapSource = BitmapSource.Create(width, height, 96, 96, pixelFormat, null, imageBytes, stride);
            return new WriteableBitmap(bitmapSource);
        }
    }
}