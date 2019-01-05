namespace Image_Transformation
{
    /// <summary>
    /// Applies a factor to each voxel of an Image3DMatrix
    /// </summary>
    public class AdjustBrightness3DOperation : IImage3DOperation
    {
        private readonly IImage3DLoader _imageLoader;

        public AdjustBrightness3DOperation(IImage3DLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double BrightnessFactor { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public bool UseCustomBrightness { get; set; }

        public Image3DMatrix GetImageMatrix()
        {
            Image3DMatrix sourceMatrix = _imageLoader.GetImageMatrix();
            if (UseCustomBrightness)
            {
                return sourceMatrix * BrightnessFactor;
            }
            else
            {
                return sourceMatrix * MetaFileBrightnessFactor;
            }
        }
    }
}