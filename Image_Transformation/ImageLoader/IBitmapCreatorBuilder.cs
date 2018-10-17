namespace Image_Transformation
{
    public interface IBitmapCreatorBuilder
    {
        int Layer { get; }
        string Path { get; }
        int Dx { get; }
        int Dy { get; }

        IBitmapCreator Build();

        IBitmapCreatorBuilder SetLayer(int layer);

        IBitmapCreatorBuilder SetPath(string path);

        IBitmapCreatorBuilder Shift(int dx, int dy);
    }
}