using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public static class MatrixToBitmapImageConverter
    {
        public static BitmapSource GetImage(ImageMatrix imageMatrix)
        {
            int height = imageMatrix.Height;
            int width = imageMatrix.Width;

            byte[] imageBytes = imageMatrix.GetBytes();

            int bitsPerPixel = 8 * imageMatrix.BytePerPixel;
            int stride = (width * bitsPerPixel + 7) / 8;

            PixelFormat pixelFormat = imageMatrix.BytePerPixel == 2 ? PixelFormats.Gray16 : PixelFormats.Gray8;
            return BitmapSource.Create(width, height, 96, 96, pixelFormat, null, imageBytes, stride);
        }
    }
}