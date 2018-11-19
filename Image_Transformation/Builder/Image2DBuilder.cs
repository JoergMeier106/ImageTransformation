using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public class Image2DMatrixBuilder
    {
        private readonly BilinearTransformation _bilinearOperation;
        private readonly AdjustBrightness2DOperation _brightnessOperation;
        private readonly Image2DMatrixLoader _imageMatrixLoader;
        private readonly Shifting2DTransformation _shiftingOperation;
        private IImage2DLoader _imageLoader;

        public Image2DMatrixBuilder()
        {
            _imageMatrixLoader = new Image2DMatrixLoader();
            _brightnessOperation = new AdjustBrightness2DOperation(_imageMatrixLoader);
            _bilinearOperation = new BilinearTransformation(_brightnessOperation);
            _shiftingOperation = new Shifting2DTransformation(_bilinearOperation);

            SetImageLoader();
        }

        public double Alpha { get; private set; }
        public double Brightness => _brightnessOperation.BrightnessFactor;
        public double Bx { get; private set; }
        public double By { get; private set; }
        public int Dx => _shiftingOperation.Dx;
        public int Dy => _shiftingOperation.Dy;
        public int Layer => _imageMatrixLoader.Layer;
        public int LayerCount => _imageLoader.LayerCount;
        public string Path => _imageMatrixLoader.Path;
        public Quadrilateral SourceQuadrilateral { get; private set; }
        public bool SourceToTargetEnabled { get; private set; }
        public double Sx { get; private set; }
        public double Sy { get; private set; }
        public int TargetImageHeight { get; private set; }
        public int TargetImageWidth { get; private set; }

        public WriteableBitmap Build()
        {
            SetImageLoader();

            Image2DMatrix imageMatrix = _imageLoader.GetImageMatrix();
            TransformationMatrix transformationMatrix = GetTransformationMatrix(imageMatrix);
            imageMatrix = ApplyTransformationMatrix(imageMatrix, transformationMatrix);

            return MatrixToBitmapImageConverter.GetImage(imageMatrix);
        }

        public Image2DMatrixBuilder MapBilinear(Quadrilateral sourceQuadrilateral)
        {
            _bilinearOperation.Quadrilateral = sourceQuadrilateral;
            return this;
        }

        public Image2DMatrixBuilder Project(Quadrilateral sourceQuadrilateral)
        {
            SourceQuadrilateral = sourceQuadrilateral;
            return this;
        }

        public Image2DMatrixBuilder Rotate(double alpha)
        {
            Alpha = alpha;
            return this;
        }

        public Image2DMatrixBuilder Scale(double sx, double sy)
        {
            Sx = sx;
            Sy = sy;
            return this;
        }

        public Image2DMatrixBuilder SetBrightness(double brightness)
        {
            _brightnessOperation.BrightnessFactor = brightness;
            _brightnessOperation.UseCustomBrightness = brightness > 0;
            return this;
        }

        public Image2DMatrixBuilder SetLayer(int layer)
        {
            _imageMatrixLoader.Layer = layer;
            return this;
        }

        public Image2DMatrixBuilder SetPath(string path)
        {
            _imageMatrixLoader.Path = path;
            return this;
        }

        public Image2DMatrixBuilder SetSourceToTargetEnabled(bool value)
        {
            SourceToTargetEnabled = value;
            return this;
        }

        public Image2DMatrixBuilder SetTargetImageHeight(int imageHeight)
        {
            TargetImageHeight = imageHeight;
            return this;
        }

        public Image2DMatrixBuilder SetTargetImageWidth(int imageWidth)
        {
            TargetImageWidth = imageWidth;
            return this;
        }

        public Image2DMatrixBuilder Shear(double bx, double by)
        {
            Bx = bx;
            By = by;
            return this;
        }

        public Image2DMatrixBuilder Shift(int dx, int dy)
        {
            _shiftingOperation.Dx = dx;
            _shiftingOperation.Dy = dy;
            return this;
        }

        private Image2DMatrix ApplyTransformationMatrix(Image2DMatrix imageMatrix, TransformationMatrix transformationMatrix)
        {
            if (transformationMatrix != TransformationMatrix.UnitMatrix3x3)
            {
                if (SourceToTargetEnabled)
                {
                    imageMatrix = Image2DMatrix.Transform(imageMatrix, transformationMatrix);
                }
                else
                {
                    Image2DMatrix targetMatrix = new Image2DMatrix(TargetImageHeight, TargetImageWidth, imageMatrix.BytePerPixel);
                    imageMatrix = Image2DMatrix.TransformTargetToSource(imageMatrix, targetMatrix, transformationMatrix.Invert2D());
                }
            }

            return imageMatrix;
        }

        private TransformationMatrix GetTransformationMatrix(Image2DMatrix imageMatrix)
        {
            if (imageMatrix != null)
            {
                TransformationMatrix transformationMatrix = TransformationMatrix.
                            UnitMatrix3x3.
                            Shear2D(Bx, By).
                            Scale2D(Sx, Sy).
                            Rotate2D(Alpha, imageMatrix.Width / 2, imageMatrix.Height / 2).
                            Project2D(SourceQuadrilateral);

                if (!SourceToTargetEnabled)
                {
                    transformationMatrix = transformationMatrix.Shift2D(Dx, Dy);
                }

                return transformationMatrix;
            }
            return TransformationMatrix.UnitMatrix3x3;
        }

        private void SetImageLoader()
        {
            if (SourceToTargetEnabled)
            {
                _imageLoader = _shiftingOperation;
            }
            else
            {
                _imageLoader = _bilinearOperation;
            }
        }
    }
}