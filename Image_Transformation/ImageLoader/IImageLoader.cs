namespace Image_Transformation
{
    public interface IImageLoader
    {
        int LayerCount { get; }
        bool MatrixChanged { get; }
        double MetaFileBrightnessFactor { get; }

        ImageMatrix GetImageMatrix();
    }
}