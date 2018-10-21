namespace Image_Transformation
{
    public class RotationOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private double _lastAlpha;

        public RotationOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double Alpha { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged => _imageLoader.MatrixChanged;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public bool TransformationAdded { get; private set; }

        public Matrix GetImageMatrix()
        {
            Matrix imageMatrix = _imageLoader.GetImageMatrix();
            TransformationAdded = _imageLoader.TransformationAdded;

            if (MatrixMustBeUpdated())
            {
                TransformationAdded = true;
                _lastAlpha = Alpha;

                return imageMatrix.Rotate(Alpha);
            }

            return imageMatrix;
        }

        private bool MatrixMustBeUpdated()
        {
            return _lastAlpha != Alpha;
        }
    }
}