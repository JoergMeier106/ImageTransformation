using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public interface IBitmapBuilder
    {
        double Alpha { get; }
        double Brightness { get; }
        int Bx { get; }
        int By { get; }
        int Dx { get; }
        int Dy { get; }
        int Layer { get; }
        int LayerCount { get; }
        string Path { get; }
        int Sx { get; }
        int Sy { get; }

        BitmapSource GetBitmap();

        void Rotate(double alpha);

        void Scale(int sx, int sy);

        void SetBrightness(double brightness);

        void SetLayer(int layer);

        void Shear(int bx, int by);

        void Shift(int dx, int dy);
    }
}