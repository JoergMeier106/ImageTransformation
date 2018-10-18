namespace Image_Transformation
{
    public class AdjustBrightnessOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private Matrix _cashedMatrix;
        private double _lastBrightnessFactor;

        public AdjustBrightnessOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double BrightnessFactor { get; set; }
        public int Height => _imageLoader.Height;
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public string Path => _imageLoader.Path;
        public bool UseCustomBrightness { get; set; }
        public int Width => _imageLoader.Width;

        public Matrix GetImageMatrix()
        {
            MatrixChanged = false;
            Matrix sourceMatrix = _imageLoader.GetImageMatrix();
            if (UseCustomBrightness)
            {
                if (_lastBrightnessFactor != BrightnessFactor || _imageLoader.MatrixChanged)
                {
                    MatrixChanged = true;
                    _lastBrightnessFactor = BrightnessFactor;
                    _cashedMatrix = sourceMatrix * BrightnessFactor;
                }
            }
            else
            {
                if (_lastBrightnessFactor != MetaFileBrightnessFactor || _imageLoader.MatrixChanged)
                {
                    MatrixChanged = true;
                    _lastBrightnessFactor = MetaFileBrightnessFactor;
                    _cashedMatrix = sourceMatrix * MetaFileBrightnessFactor;
                }
            }

            return _cashedMatrix;
        }
    }
}