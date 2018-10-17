using System.Collections.Generic;

namespace Image_Transformation
{
    public class BitmapCreatorBuilder : IBitmapCreatorBuilder
    {
        private readonly List<IImageOperation> _imageOperations;
        private bool _shiftEnabled;

        public BitmapCreatorBuilder()
        {
            _imageOperations = new List<IImageOperation>();
        }

        public int Dx { get; private set; }
        public int Dy { get; private set; }
        public int Layer { get; private set; }
        public string Path { get; private set; }

        public IBitmapCreatorBuilder AddTransformation(IImageOperation transformation)
        {
            _imageOperations.Add(transformation);
            return this;
        }

        public IBitmapCreator Build()
        {
            ImageMatrixLoader imageByteLoader = new ImageMatrixLoader(Path, Layer);
            AdjustBrightnessOperation brightnessOperation = new AdjustBrightnessOperation(imageByteLoader);
            ShiftOperation shiftOperation = new ShiftOperation(brightnessOperation, Dx, Dy);

            return new MatrixToBitmapImageConverter(brightnessOperation);
        }

        public IBitmapCreatorBuilder ClearAllTransformation()
        {
            _imageOperations.Clear();
            return this;
        }

        public IBitmapCreatorBuilder SetLayer(int layer)
        {
            Layer = layer;
            return this;
        }

        public IBitmapCreatorBuilder SetPath(string path)
        {
            Path = path;
            return this;
        }

        public IBitmapCreatorBuilder Shift(int dx, int dy)
        {
            _shiftEnabled = true;
            Dx = dx;
            Dy = dy;
            return this;
        }
    }
}