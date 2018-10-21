namespace Image_Transformation
{
    public class ShearingOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private int _lastBx;
        private int _lastBy;

        public ShearingOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int Bx { get; set; }
        public int By { get; set; }
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
                _lastBx = Bx;
                _lastBy = By;

                return imageMatrix.Shear(Bx, By);
            }

            return imageMatrix;
        }

        private bool MatrixMustBeUpdated()
        {
            return _lastBx != Bx || _lastBy != By;
        }
    }
}