namespace Image_Transformation
{
    public class Shifting2DTransformation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private Image2DMatrix _cashedMatrix;
        private int _lastDx;
        private int _lastDy;

        public Shifting2DTransformation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int Dx { get; set; }
        public int Dy { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;

        public Image2DMatrix GetImageMatrix()
        {
            MatrixChanged = false;
            Image2DMatrix sourceMatrix = _imageLoader.GetImageMatrix();

            if (OperationShouldBeExecuted())
            {
                if (MatrixMustBeUpdated())
                {
                    MatrixChanged = true;
                    _lastDx = Dx;
                    _lastDy = Dy;

                    Transformation2DMatrix shiftingMatrix = Transformation2DMatrix.UnitMatrix.Shift(Dx, Dy);
                    _cashedMatrix = Image2DMatrix.Transform(sourceMatrix,
                        new Image2DMatrix(sourceMatrix.Height, sourceMatrix.Width, sourceMatrix.BytePerPixel), shiftingMatrix);
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