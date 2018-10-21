namespace Image_Transformation
{
    public class ScalingOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private double _lastSx;
        private double _lastSy;

        public ScalingOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged => _imageLoader.MatrixChanged;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public int Sx { get; set; }
        public int Sy { get; set; }
        public bool TransformationAdded { get; private set; }

        public Matrix GetImageMatrix()
        {
            Matrix imageMatrix = _imageLoader.GetImageMatrix();
            TransformationAdded = _imageLoader.TransformationAdded;

            if (MatrixMustBeUpdated())
            {
                TransformationAdded = true;
                _lastSx = Sx;
                _lastSy = Sy;

                return imageMatrix.Scale(Sx, Sy);
            }

            return imageMatrix;
        }

        private bool MatrixMustBeUpdated()
        {
            return _lastSx != Sx || _lastSy != Sy;
        }
    }
}