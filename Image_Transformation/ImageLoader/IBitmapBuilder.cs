namespace Image_Transformation
{
    public interface IBitmapBuilder
    {
        double Alpha { get; }
        double Brightness { get; }
        double Bx { get; }
        double By { get; }
        int Dx { get; }
        int Dy { get; }
        int Layer { get; }
        int LayerCount { get; }
        string Path { get; }
        Quadrilateral SourceQuadrilateral { get; }
        double Sx { get; }
        double Sy { get; }
        int TargetImageHeight { get; }
        int TargetImageWidth { get; }
        bool SourceToTargetEnabled { get; }

        ImageMatrix Build();

        IBitmapBuilder MapBilinear(Quadrilateral sourceQuadrilateral);

        IBitmapBuilder Project(Quadrilateral sourceQuadrilateral);

        IBitmapBuilder Rotate(double alpha);

        IBitmapBuilder Scale(double sx, double sy);

        IBitmapBuilder SetBrightness(double brightness);

        IBitmapBuilder SetLayer(int layer);

        IBitmapBuilder SetPath(string path);

        IBitmapBuilder Shear(double bx, double by);

        IBitmapBuilder Shift(int dx, int dy);

        IBitmapBuilder SetSourceToTargetEnabled(bool value);

        IBitmapBuilder SetTargetImageHeight(int imageHeight);

        IBitmapBuilder SetTargetImageWidth(int imageWidth);
    }
}