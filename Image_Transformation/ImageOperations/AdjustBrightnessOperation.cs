namespace Image_Transformation
{
    public class AdjustBrightnessOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private ImageMatrix _cashedMatrix;
        private double _lastBrightnessFactor;

        public AdjustBrightnessOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double BrightnessFactor { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public bool UseCustomBrightness { get; set; }

        public ImageMatrix GetImageMatrix()
        {
            MatrixChanged = false;
            ImageMatrix sourceMatrix = _imageLoader.GetImageMatrix();
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

        private void AdjustBrightness(ImageMatrix sourceMatrix, double brightnessFactor)
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