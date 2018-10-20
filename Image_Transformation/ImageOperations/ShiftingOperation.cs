namespace Image_Transformation
{
    public class ShiftingOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private Matrix _cashedMatrix;
        private int _lastDx;
        private int _lastDy;

        public ShiftingOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int Dx { get; set; }
        public int Dy { get; set; }
        public int Height => _imageLoader.Height;
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public string Path => _imageLoader.Path;
        public int Width => _imageLoader.Width;

        public Matrix GetImageMatrix()
        {
            MatrixChanged = false;
            Matrix sourceMatrix = _imageLoader.GetImageMatrix();

            if (OperationShouldBeExecuted())
            {
                if (MatrixMustBeUpdated())
                {
                    _lastDx = Dx;
                    _lastDy = Dy;

                    Matrix shiftedMatrix = new Matrix(Height, Width, new byte[Height * Width * 2]);
                    _cashedMatrix = Matrix.Shift(sourceMatrix, shiftedMatrix, Dx, Dy);
                }

                return _cashedMatrix;
            }
            else
            {
                MatrixChanged = true;
                _lastDx = Dx;
                _lastDy = Dy;
                return sourceMatrix;
            }
        }

        private bool MatrixMustBeUpdated()
        {
            return _lastDx != Dx || _lastDy != Dy || _imageLoader.MatrixChanged;
        }

        private bool OperationShouldBeExecuted()
        {
            return Dx != 0 || Dy != 0;
        }
    }
}