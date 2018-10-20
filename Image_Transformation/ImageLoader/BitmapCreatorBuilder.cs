using System.Collections.Generic;

namespace Image_Transformation
{
    public class BitmapCreatorBuilder : IBitmapCreatorBuilder
    {
        private readonly AdjustBrightnessOperation _brightnessOperation;
        private readonly ImageMatrixLoader _imageMatrixLoader;
        private readonly List<IImageOperation> _imageOperations;
        private readonly MatrixToBitmapImageConverter _matrixToBitmapConverter;
        private readonly RotationOperation _rotationOperation;
        private readonly ScalingOperation _scalingOperation;
        private readonly ShearingOperation _shearingOperation;
        private readonly ShiftingOperation _shiftingOperation;

        public BitmapCreatorBuilder()
        {
            _imageOperations = new List<IImageOperation>();
            _imageMatrixLoader = new ImageMatrixLoader();
            _brightnessOperation = new AdjustBrightnessOperation(_imageMatrixLoader);
            _shearingOperation = new ShearingOperation(_brightnessOperation);
            _shiftingOperation = new ShiftingOperation(_shearingOperation);
            _scalingOperation = new ScalingOperation(_shiftingOperation);
            _rotationOperation = new RotationOperation(_scalingOperation);
            _matrixToBitmapConverter = new MatrixToBitmapImageConverter(_rotationOperation);
        }

        public double Alpha => _rotationOperation.Alpha;
        public double Brightness => _brightnessOperation.BrightnessFactor;
        public int Bx => _shearingOperation.Bx;
        public int By => _shearingOperation.By;
        public int Dx => _shiftingOperation.Dx;
        public int Dy => _shiftingOperation.Dy;
        public int Layer => _imageMatrixLoader.Layer;
        public string Path => _imageMatrixLoader.Path;
        public int Sx => _scalingOperation.Sx;
        public int Sy => _scalingOperation.Sy;

        public IBitmapCreatorBuilder AddTransformation(IImageOperation transformation)
        {
            _imageOperations.Add(transformation);
            return this;
        }

        public IBitmapCreator Build()
        {
            return _matrixToBitmapConverter;
        }

        public IBitmapCreatorBuilder ClearAllTransformation()
        {
            _imageOperations.Clear();
            return this;
        }

        public IBitmapCreatorBuilder Rotate(double alpha)
        {
            _rotationOperation.Alpha = alpha;
            return this;
        }

        public IBitmapCreatorBuilder Scale(int sx, int sy)
        {
            _scalingOperation.Sx = sx;
            _scalingOperation.Sy = sy;
            return this;
        }

        public IBitmapCreatorBuilder SetBrightness(double brightness)
        {
            _brightnessOperation.BrightnessFactor = brightness;
            _brightnessOperation.UseCustomBrightness = brightness > 0;
            return this;
        }

        public IBitmapCreatorBuilder SetLayer(int layer)
        {
            _imageMatrixLoader.Layer = layer;
            return this;
        }

        public IBitmapCreatorBuilder SetPath(string path)
        {
            _imageMatrixLoader.Path = path;
            return this;
        }

        public IBitmapCreatorBuilder Shear(int bx, int by)
        {
            _shearingOperation.Bx = bx;
            _shearingOperation.By = by;
            return this;
        }

        public IBitmapCreatorBuilder Shift(int dx, int dy)
        {
            _shiftingOperation.Dx = dx;
            _shiftingOperation.Dy = dy;
            return this;
        }
    }
}