using System.Threading.Tasks;

namespace Image_Transformation
{
    public class ShiftingOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private ImageMatrix _cashedMatrix;
        private int _lastDx;
        private int _lastDy;

        public ShiftingOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int Dx { get; set; }
        public int Dy { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;

        public ImageMatrix GetImageMatrix()
        {
            MatrixChanged = false;
            ImageMatrix sourceMatrix = _imageLoader.GetImageMatrix();

            if (OperationShouldBeExecuted())
            {
                if (MatrixMustBeUpdated())
                {
                    MatrixChanged = true;
                    _lastDx = Dx;
                    _lastDy = Dy;

                    TransformationMatrix shiftingMatrix = TransformationMatrix.UnitMatrix.Shift(Dx, Dy);
                    _cashedMatrix = ImageMatrix.Transform(sourceMatrix,
                        new ImageMatrix(sourceMatrix.Height, sourceMatrix.Width, sourceMatrix.BytePerPixel), shiftingMatrix);
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