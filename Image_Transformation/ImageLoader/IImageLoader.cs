namespace Image_Transformation
{
    public interface IImageLoader
    {
        double BrightnessFactor { get; }
        int Height { get; }
        int LayerCount { get; }
        string Path { get; }
        int Width { get; }

        Matrix GetImageMatrix();
    }
}