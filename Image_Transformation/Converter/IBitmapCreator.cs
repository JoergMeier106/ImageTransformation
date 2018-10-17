using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public interface IBitmapCreator
    {
        int Height { get; }
        int Width { get; }

        int LayerCount { get; }

        BitmapSource GetImage();
    }
}