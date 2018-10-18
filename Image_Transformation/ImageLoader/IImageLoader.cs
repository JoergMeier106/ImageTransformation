namespace Image_Transformation
{
    public interface IImageLoader
    {
        double MetaFileBrightnessFactor { get; }
        int Height { get; }
        int LayerCount { get; }
        string Path { get; }
        int Width { get; }
        bool MatrixChanged { get; }

        Matrix GetImageMatrix();
    }
}