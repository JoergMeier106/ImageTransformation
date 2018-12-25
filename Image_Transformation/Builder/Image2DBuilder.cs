using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    /// <summary>
    /// Creates an image matrix. Most of the methods are lazy, e.g. the changes will be applied after calling Build().
    /// </summary>
    public class Image2DMatrixBuilder
    {
        private readonly BilinearTransformation _bilinearOperation;
        private readonly AdjustBrightness2DOperation _brightnessOperation;
        private readonly Image2DMatrixLoader _imageMatrixLoader;
        private readonly Shifting2DTransformation _shiftingOperation;
        private IImage2DLoader _imageLoader;

        public Image2DMatrixBuilder()
        {
            //This is the loading stack of an image. It starts with reading the bytes and storing them in an image matrix.
            //Then, the brightness will be set. It is not possible to concatinate a bilinear transformation, so it will be
            //executed on its own. In the case of a source to target transformation, shifting will be executed on its own,
            //as well. Thereby, the size of the matrix does not change for shifting.
            _imageMatrixLoader = new Image2DMatrixLoader();
            _brightnessOperation = new AdjustBrightness2DOperation(_imageMatrixLoader);
            _bilinearOperation = new BilinearTransformation(_brightnessOperation);
            _shiftingOperation = new Shifting2DTransformation(_bilinearOperation);

            SetImageLoader();
        }

        /// <summary>
        /// Rotation alpha.
        /// </summary>
        public double Alpha { get; private set; }
        /// <summary>
        /// This factor will be aplied to each pixel.
        /// </summary>
        public double Brightness => _brightnessOperation.BrightnessFactor;
        /// <summary>
        /// Shearing in x direction.
        /// </summary>
        public double Bx { get; private set; }
        /// <summary>
        /// Shearing in y direction.
        /// </summary>
        public double By { get; private set; }
        /// <summary>
        /// Shifting in x direction.
        /// </summary>
        public int Dx => _shiftingOperation.Dx;
        /// <summary>
        /// Shifting in y direction.
        /// </summary>
        public int Dy => _shiftingOperation.Dy;
        /// <summary>
        /// The layer which is currently selected.
        /// </summary>
        public int Layer => _imageMatrixLoader.Layer;
        /// <summary>
        /// Tells how many layer the image has.
        /// </summary>
        public int LayerCount => _imageLoader.LayerCount;
        /// <summary>
        /// The path of the image file.
        /// </summary>
        public string Path => _imageMatrixLoader.Path;
        /// <summary>
        /// The quadrilateral which will be used as source for projective and 
        /// bilinear transformations.
        /// </summary>
        public Quadrilateral SourceQuadrilateral { get; private set; }
        public bool SourceToTargetEnabled { get; private set; }
        /// <summary>
        /// Scaling in x direction.
        /// </summary>
        public double Sx { get; private set; }
        /// <summary>
        /// Scaling in y direction.
        /// </summary>
        public double Sy { get; private set; }
        public int TargetImageHeight { get; private set; }
        public int TargetImageWidth { get; private set; }

        /// <summary>
        /// Executes the image loading stack and applies transformations.
        /// Finally, the image matrix will be converted to a bitmap image.
        /// </summary>
        /// <returns></returns>
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
                    //With target to source enabled, the inverted transformation matrix is needed.
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
                //The transformations will be concatenated here. However, it is possible that
                //some combinations of parameters do need lead to correct results, because
                //transformations are not cumulative.
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