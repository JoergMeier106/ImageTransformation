namespace Image_Transformation
{
    public interface IBitmapCreatorBuilder
    {
        double Brightness { get; }
        int Bx { get; }
        int By { get; }
        int Dx { get; }
        int Dy { get; }
        int Layer { get; }
        string Path { get; }

        IBitmapCreator Build();

        IBitmapCreatorBuilder SetBrightness(double brightness);

        IBitmapCreatorBuilder SetLayer(int layer);

        IBitmapCreatorBuilder SetPath(string path);

        IBitmapCreatorBuilder Shear(int bx, int by);

        IBitmapCreatorBuilder Shift(int dx, int dy);
    }
}