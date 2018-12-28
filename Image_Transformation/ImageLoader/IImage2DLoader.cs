namespace Image_Transformation
{
    public interface IImage2DLoader
    {
        int LayerCount { get; }
        /// <summary>
        /// Can be used to check if cached data can be used.
        /// </summary>
        bool MatrixChanged { get; }
        double MetaFileBrightnessFactor { get; }

        Image2DMatrix GetImageMatrix();
    }
}