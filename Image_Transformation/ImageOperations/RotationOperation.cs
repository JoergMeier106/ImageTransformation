namespace Image_Transformation
{
    public class RotationOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;

        private Matrix _cashedMatrix;

        private double _lastAlpha;

        public RotationOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double Alpha { get; set; }
        public int Height => _imageLoader.Height;
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public string Path => _imageLoader.Path;
        public int Width => _imageLoader.Width;

        public Matrix GetImageMatrix()
        {
            MatrixChanged = false;
            Matrix imageMatrix = _imageLoader.GetImageMatrix();

            if (OperationShouldBeExecuted())
            {
                if (MatrixMustBeUpdated())
                {
                    MatrixChanged = true;
                    _lastAlpha = Alpha;

                    Matrix scaledMatrix = new Matrix(Height, Width, new byte[Height * Width * 2]);
                    _cashedMatrix = Matrix.Rotate(imageMatrix, scaledMatrix, Alpha);
                }

                return _cashedMatrix;
            }
            else
            {
                MatrixChanged = true;

                _lastAlpha = Alpha;

                return imageMatrix;
            }
        }

        private bool MatrixMustBeUpdated()
        {
            return _lastAlpha != Alpha || _imageLoader.MatrixChanged;
        }

        private bool OperationShouldBeExecuted()
        {
            return Alpha != 10;
        }
    }
}