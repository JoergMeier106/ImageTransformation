using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public sealed class CommonFormatImageLoader : IImageLoader
    {
        public CommonFormatImageLoader(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }

        public BitmapImage GetImage()
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(Path);
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}