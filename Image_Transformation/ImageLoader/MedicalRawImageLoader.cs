using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public class MedicalRawImageLoader : IImageLoader
    {
        public MedicalRawImageLoader(string path, int layerIndex, int width, int height)
        {
            Path = path;
            LayerIndex = layerIndex;
            Width = width;
            Height = height;
        }

        public int Height { get; }
        public int LayerIndex { get; }
        public string Path { get; }
        public int Width { get; }

        public BitmapImage GetImage()
        {
            byte[] rawBytes = File.ReadAllBytes(Path);
            int imageLength = Height * Width;
            int imagePosition = imageLength * LayerIndex;
            byte[] imageBytes = new byte[imageLength];

            Array.Copy(rawBytes, imagePosition, imageBytes, 0, imageLength);
            Stream stream = new MemoryStream(imageBytes);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}