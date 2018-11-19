namespace Image_Transformation
{
    public interface IImage2DLoader
    {
        int LayerCount { get; }
        bool MatrixChanged { get; }
        double MetaFileBrightnessFactor { get; }

        Image2DMatrix GetImageMatrix();
    }
}