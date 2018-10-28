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
        Quadrilateral SourceQuadrilateral { get; }
        int Sx { get; }
        int Sy { get; }

        WriteableBitmap Build();

        IBitmapBuilder Project(Quadrilateral sourceQuadrilateral);

        IBitmapBuilder Rotate(double alpha);

        IBitmapBuilder Scale(int sx, int sy);

        IBitmapBuilder SetBrightness(double brightness);

        IBitmapBuilder SetLayer(int layer);

        IBitmapBuilder SetPath(string path);

        IBitmapBuilder Shear(int bx, int by);

        IBitmapBuilder Shift(int dx, int dy);
    }
}