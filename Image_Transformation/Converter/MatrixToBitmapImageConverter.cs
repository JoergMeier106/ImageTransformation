using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public class MatrixToBitmapImageConverter : IBitmapCreator
    {
        private readonly IImageOperation _imageOperation;

        public MatrixToBitmapImageConverter(IImageOperation imageOperation)
        {
            _imageOperation = imageOperation;
        }

        public int Height { get; private set; }
        public int LayerCount => _imageOperation.LayerCount;
        public int Width { get; private set; }

        public BitmapSource GetImage()
        {
            Matrix imageMatrix = _imageOperation.GetImageMatrix();
            imageMatrix = imageMatrix.ExecuteTransformations();

            Height = imageMatrix.Height;
            Width = imageMatrix.Width;

            byte[] imageBytes = imageMatrix.GetBytes();

            int bitsPerPixel = 16;
            int stride = (Width * bitsPerPixel + 7) / 8;

            PixelFormat pixelFormat = imageMatrix.BytePerPixel == 2 ? PixelFormats.Gray16 : PixelFormats.Gray8;
            return BitmapSource.Create(Width, Height, 96, 96, pixelFormat, null, imageBytes, stride);
        }
    }
}