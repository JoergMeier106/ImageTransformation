namespace Image_Transformation
{
    public interface IImageLoader
    {
        int LayerCount { get; }
        bool MatrixChanged { get; }
        bool TransformationAdded { get; }
        double MetaFileBrightnessFactor { get; }

        Matrix GetImageMatrix();
    }
}