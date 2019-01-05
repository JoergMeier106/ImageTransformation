namespace Image_Transformation
{
    public interface IImage3DLoader
    {
        int LayerCount { get; }
        double MetaFileBrightnessFactor { get; }

        Image3DMatrix GetImageMatrix();
    }
}