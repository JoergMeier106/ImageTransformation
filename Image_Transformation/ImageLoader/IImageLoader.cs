using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public interface IImageLoader
    {
        string Path { get; }

        BitmapImage GetImage();
    }
}