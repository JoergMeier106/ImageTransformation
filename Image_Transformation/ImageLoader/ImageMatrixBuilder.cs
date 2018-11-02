namespace Image_Transformation
{
    public class ImageMatrixBuilder : IImageMatrixBuilder
    {
        private readonly BilinearOperation _bilinearOperation;
        private readonly AdjustBrightnessOperation _brightnessOperation;
        private readonly ImageMatrixLoader _imageMatrixLoader;
        private readonly ShiftingOperation _shiftingOperation;
        private IImageLoader _imageLoader;

        public ImageMatrixBuilder()
        {
            _imageMatrixLoader = new ImageMatrixLoader();
            _brightnessOperation = new AdjustBrightnessOperation(_imageMatrixLoader);
            _bilinearOperation = new BilinearOperation(_brightnessOperation);
            _shiftingOperation = new ShiftingOperation(_bilinearOperation);

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

        public ImageMatrix Build()
        {
            SetImageLoader();

            ImageMatrix imageMatrix = _imageLoader.GetImageMatrix();
            TransformationMatrix transformationMatrix = GetTransformationMatrix(imageMatrix);
            imageMatrix = ApplyTransformationMatrix(imageMatrix, transformationMatrix);

            return imageMatrix;
        }

        public IImageMatrixBuilder MapBilinear(Quadrilateral sourceQuadrilateral)
        {
            _bilinearOperation.Quadrilateral = sourceQuadrilateral;
            return this;
        }

        public IImageMatrixBuilder Project(Quadrilateral sourceQuadrilateral)
        {
            SourceQuadrilateral = sourceQuadrilateral;
            return this;
        }

        public IImageMatrixBuilder Rotate(double alpha)
        {
            Alpha = alpha;
            return this;
        }

        public IImageMatrixBuilder Scale(double sx, double sy)
        {
            Sx = sx;
            Sy = sy;
            return this;
        }

        public IImageMatrixBuilder SetBrightness(double brightness)
        {
            _brightnessOperation.BrightnessFactor = brightness;
            _brightnessOperation.UseCustomBrightness = brightness > 0;
            return this;
        }

        public IImageMatrixBuilder SetLayer(int layer)
        {
            _imageMatrixLoader.Layer = layer;
            return this;
        }

        public IImageMatrixBuilder SetPath(string path)
        {
            _imageMatrixLoader.Path = path;
            return this;
        }

        public IImageMatrixBuilder SetSourceToTargetEnabled(bool value)
        {
            SourceToTargetEnabled = value;
            return this;
        }

        public IImageMatrixBuilder SetTargetImageHeight(int imageHeight)
        {
            TargetImageHeight = imageHeight;
            return this;
        }

        public IImageMatrixBuilder SetTargetImageWidth(int imageWidth)
        {
            TargetImageWidth = imageWidth;
            return this;
        }

        public IImageMatrixBuilder Shear(double bx, double by)
        {
            Bx = bx;
            By = by;
            return this;
        }

        public IImageMatrixBuilder Shift(int dx, int dy)
        {
            _shiftingOperation.Dx = dx;
            _shiftingOperation.Dy = dy;
            return this;
        }

        private ImageMatrix ApplyTransformationMatrix(ImageMatrix imageMatrix, TransformationMatrix transformationMatrix)
        {
            if (transformationMatrix != TransformationMatrix.UnitMatrix)
            {
                if (SourceToTargetEnabled)
                {
                    imageMatrix = ImageMatrix.Transform(imageMatrix, transformationMatrix);
                }
                else
                {
                    ImageMatrix targetMatrix = new ImageMatrix(TargetImageHeight, TargetImageWidth, imageMatrix.BytePerPixel);
                    imageMatrix = ImageMatrix.TransformTargetToSource(imageMatrix, targetMatrix, transformationMatrix.Invert());
                }
            }

            return imageMatrix;
        }

        private TransformationMatrix GetTransformationMatrix(ImageMatrix imageMatrix)
        {
            TransformationMatrix transformationMatrix = TransformationMatrix.
                            UnitMatrix.
                            Shear(Bx, By).
                            Scale(Sx, Sy).
                            Rotate(Alpha, imageMatrix.Width / 2, imageMatrix.Height / 2).
                            Project(SourceQuadrilateral);

            if (!SourceToTargetEnabled)
            {
                transformationMatrix = transformationMatrix.Shift(Dx, Dy);
            }

            return transformationMatrix;
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