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

        public int Height => _imageOperation.Height;
        public int LayerCount => _imageOperation.LayerCount;
        public int Width => _imageOperation.Width;

        public BitmapSource GetImage()
        {
            byte[] imageBytes = _imageOperation.GetImageMatrix().GetBytes();

            int bitsPerPixel = 16;
            int stride = (Width * bitsPerPixel + 7) / 8;

            var bitmap = BitmapSource.Create(Height, Width, 96, 96, PixelFormats.Gray16, null, imageBytes, stride);
            //MemoryStream memory = new MemoryStream();

            //var encoder = new BmpBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(bitmap));
            //encoder.Save(memory);

            //BitmapImage bitmapImage = new BitmapImage();
            //bitmapImage.BeginInit();
            //bitmapImage.StreamSource = memory;
            //bitmapImage.EndInit();
            return bitmap;
        }
    }
}