namespace Image_Transformation
{
    /// <summary>
    /// Applies a factor to each voxel of an Image3DMatrix
    /// </summary>
    public class AdjustBrightness3DOperation : IImage3DOperation
    {
        private readonly IImage3DLoader _imageLoader;
        private Image3DMatrix _cashedMatrix;
        private double _lastBrightnessFactor;

        public AdjustBrightness3DOperation(IImage3DLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double BrightnessFactor { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public bool UseCustomBrightness { get; set; }

        public Image3DMatrix GetImageMatrix()
        {
            MatrixChanged = false;
            Image3DMatrix sourceMatrix = _imageLoader.GetImageMatrix();
            if (UseCustomBrightness)
            {
                AdjustBrightness(sourceMatrix, BrightnessFactor);
            }
            else
            {
                AdjustBrightness(sourceMatrix, MetaFileBrightnessFactor);
            }

            return _cashedMatrix;
        }

        private void AdjustBrightness(Image3DMatrix sourceMatrix, double brightnessFactor)
        {
            if (_lastBrightnessFactor != brightnessFactor || _imageLoader.MatrixChanged)
            {
                MatrixChanged = true;
                _lastBrightnessFactor = brightnessFactor;
                _cashedMatrix = sourceMatrix * brightnessFactor;
            }
        }
    }
}