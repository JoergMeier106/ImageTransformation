using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public class BitmapBuilder : IBitmapBuilder
    {
        private readonly BilinearOperation _bilinearOperation;
        private readonly AdjustBrightnessOperation _brightnessOperation;
        private readonly IImageLoader _imageLoader;
        private readonly ImageMatrixLoader _imageMatrixLoader;
        private readonly List<IImageOperation> _imageOperations;
        private readonly ShiftingOperation _shiftingOperation;

        public BitmapBuilder()
        {
            _imageOperations = new List<IImageOperation>();
            _imageMatrixLoader = new ImageMatrixLoader();
            _brightnessOperation = new AdjustBrightnessOperation(_imageMatrixLoader);
            _shiftingOperation = new ShiftingOperation(_brightnessOperation);
            _bilinearOperation = new BilinearOperation(_shiftingOperation);

            _imageLoader = _bilinearOperation;
        }

        public double Alpha { get; private set; }
        public double Brightness => _brightnessOperation.BrightnessFactor;
        public int Bx { get; private set; }
        public int By { get; private set; }
        public int Dx => _shiftingOperation.Dx;
        public int Dy => _shiftingOperation.Dy;
        public int Layer => _imageMatrixLoader.Layer;
        public int LayerCount => _imageLoader.LayerCount;
        public string Path => _imageMatrixLoader.Path;
        public Quadrilateral SourceQuadrilateral { get; private set; }
        public double Sx { get; private set; }
        public double Sy { get; private set; }

        public IBitmapBuilder AddTransformation(IImageOperation transformation)
        {
            _imageOperations.Add(transformation);
            return this;
        }

        public WriteableBitmap Build()
        {
            ImageMatrix imageMatrix = _imageLoader.GetImageMatrix();

            TransformationMatrix transformationMatrix = TransformationMatrix.
                UnitMatrix.
                Shear(Bx, By).
                Scale(Sx, Sy).
                Rotate(Alpha, imageMatrix.Width / 2, imageMatrix.Height / 2).
                Project(SourceQuadrilateral);

            if (transformationMatrix != TransformationMatrix.UnitMatrix)
            {
                imageMatrix = ImageMatrix.Transform(imageMatrix, transformationMatrix);
            }

            return MatrixToBitmapImageConverter.GetImage(imageMatrix);
        }

        public IBitmapBuilder ClearAllTransformation()
        {
            _imageOperations.Clear();
            return this;
        }

        public IBitmapBuilder MapBilinear(Quadrilateral sourceQuadrilateral)
        {
            _bilinearOperation.Quadrilateral = sourceQuadrilateral;
            return this;
        }

        public IBitmapBuilder Project(Quadrilateral sourceQuadrilateral)
        {
            SourceQuadrilateral = sourceQuadrilateral;
            return this;
        }

        public IBitmapBuilder Rotate(double alpha)
        {
            Alpha = alpha;
            return this;
        }

        public IBitmapBuilder Scale(double sx, double sy)
        {
            Sx = sx;
            Sy = sy;
            return this;
        }

        public IBitmapBuilder SetBrightness(double brightness)
        {
            _brightnessOperation.BrightnessFactor = brightness;
            _brightnessOperation.UseCustomBrightness = brightness > 0;
            return this;
        }

        public IBitmapBuilder SetLayer(int layer)
        {
            _imageMatrixLoader.Layer = layer;
            return this;
        }

        public IBitmapBuilder SetPath(string path)
        {
            _imageMatrixLoader.Path = path;
            return this;
        }

        public IBitmapBuilder Shear(int bx, int by)
        {
            Bx = bx;
            By = by;
            return this;
        }

        public IBitmapBuilder Shift(int dx, int dy)
        {
            _shiftingOperation.Dx = dx;
            _shiftingOperation.Dy = dy;
            return this;
        }
    }
}