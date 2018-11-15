namespace Image_Transformation
{
    public sealed class Image3DMatrixBuilder
    {
        private readonly AdjustBrightnessOperation _brightnessOperation;
        private readonly ImageMatrixLoader _imageMatrixLoader;
        private readonly IImageLoader _imageLoader;

        public Image3DMatrixBuilder()
        {
            _imageMatrixLoader = new ImageMatrixLoader();
            _brightnessOperation = new AdjustBrightnessOperation(_imageMatrixLoader);
            _imageLoader = _brightnessOperation;
        }

        public double Alpha { get; private set; }
        public double Brightness => _brightnessOperation.BrightnessFactor;
        public double Bx { get; private set; }
        public double By { get; private set; }
        public int Dx { get; private set; }
        public int Dy { get; private set; }
        public int LayerCount => _imageLoader.LayerCount;
        public string Path => _imageMatrixLoader.Path;
        public double Sx { get; private set; }
        public double Sy { get; private set; }

        public Image2DMatrix Build()
        {
            Image2DMatrix imageMatrix = _imageLoader.GetImageMatrix();
            Transformation2DMatrix transformationMatrix = GetTransformationMatrix(imageMatrix);
            imageMatrix = ApplyTransformationMatrix(imageMatrix, transformationMatrix);

            return imageMatrix;
        }

        public Image3DMatrixBuilder Rotate(double alpha)
        {
            Alpha = alpha;
            return this;
        }

        public Image3DMatrixBuilder Scale(double sx, double sy)
        {
            Sx = sx;
            Sy = sy;
            return this;
        }

        public Image3DMatrixBuilder SetBrightness(double brightness)
        {
            _brightnessOperation.BrightnessFactor = brightness;
            _brightnessOperation.UseCustomBrightness = brightness > 0;
            return this;
        }

        public Image3DMatrixBuilder SetLayer(int layer)
        {
            _imageMatrixLoader.Layer = layer;
            return this;
        }

        public Image3DMatrixBuilder SetPath(string path)
        {
            _imageMatrixLoader.Path = path;
            return this;
        }

        public Image3DMatrixBuilder Shear(double bx, double by)
        {
            Bx = bx;
            By = by;
            return this;
        }

        public Image3DMatrixBuilder Shift(int dx, int dy)
        {
            Dx = dx;
            Dy = dy;
            return this;
        }

        private Image2DMatrix ApplyTransformationMatrix(Image2DMatrix imageMatrix, Transformation2DMatrix transformationMatrix)
        {
            if (transformationMatrix != Transformation2DMatrix.UnitMatrix)
            {
                imageMatrix = Image2DMatrix.Transform(imageMatrix, transformationMatrix);
            }

            return imageMatrix;
        }

        private Transformation2DMatrix GetTransformationMatrix(Image2DMatrix imageMatrix)
        {
            if (imageMatrix != null)
            {
                Transformation2DMatrix transformationMatrix = Transformation2DMatrix.
                            UnitMatrix.
                            Shear(Bx, By).
                            Scale(Sx, Sy).
                            Rotate(Alpha, imageMatrix.Width / 2, imageMatrix.Height / 2).
                            Shift(Dx, Dy);

                return transformationMatrix;
            }
            return Transformation2DMatrix.UnitMatrix;
        }
    }
}