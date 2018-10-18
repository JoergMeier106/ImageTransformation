using System.Collections.Generic;

namespace Image_Transformation
{
    public class BitmapCreatorBuilder : IBitmapCreatorBuilder
    {
        private readonly AdjustBrightnessOperation _brightnessOperation;
        private readonly ImageMatrixLoader _imageMatrixLoader;
        private readonly List<IImageOperation> _imageOperations;
        private readonly MatrixToBitmapImageConverter _matrixToBitmapConverter;
        private readonly ShearingOperation _shearingOperation;
        private readonly ShiftOperation _shiftOperation;

        public BitmapCreatorBuilder()
        {
            _imageOperations = new List<IImageOperation>();
            _imageMatrixLoader = new ImageMatrixLoader();
            _brightnessOperation = new AdjustBrightnessOperation(_imageMatrixLoader);
            _shearingOperation = new ShearingOperation(_brightnessOperation);
            _shiftOperation = new ShiftOperation(_shearingOperation);
            _matrixToBitmapConverter = new MatrixToBitmapImageConverter(_shiftOperation);
        }

        public double Brightness => _brightnessOperation.BrightnessFactor;
        public int Bx => _shearingOperation.Bx;
        public int By => _shearingOperation.By;
        public int Dx => _shiftOperation.Dx;
        public int Dy => _shiftOperation.Dy;
        public int Layer => _imageMatrixLoader.Layer;
        public string Path => _imageMatrixLoader.Path;

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
            _shiftOperation.Dx = dx;
            _shiftOperation.Dy = dy;
            return this;
        }
    }
}