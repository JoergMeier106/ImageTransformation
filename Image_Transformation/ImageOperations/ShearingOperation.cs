using System;

namespace Image_Transformation
{
    public class ShearingOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private Matrix _cashedMatrix;
        private int _lastBx;
        private int _lastBy;

        public ShearingOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int Bx { get; set; }
        public int By { get; set; }
        public int Height { get; private set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public string Path => _imageLoader.Path;
        public int Width { get; private set; }

        public Matrix GetImageMatrix()
        {
            MatrixChanged = false;
            Matrix imageMatrix = _imageLoader.GetImageMatrix();

            if (OperationShouldBeExecuted())
            {
                if (MatrixMustBeUpdated())
                {
                    MatrixChanged = true;
                    _lastBx = Bx;
                    _lastBy = By;

                    int absoluteBx = Math.Abs(Bx);
                    int absoluteBy = Math.Abs(By);

                    Height = imageMatrix.Height * (absoluteBy + 1 + (absoluteBy * absoluteBx));
                    Width = imageMatrix.Width * (absoluteBx + 1);

                    Matrix shearedMatrix = new Matrix(Height, Width, new byte[Height * Width * 2]);
                    _cashedMatrix = Matrix.Shear(imageMatrix, shearedMatrix, Bx, By);
                }
                return _cashedMatrix;
            }
            else
            {
                Height = _imageLoader.Height;
                Width = _imageLoader.Width;

                MatrixChanged = true;

                _lastBx = 0;
                _lastBy = 0;

                return imageMatrix;
            }
        }

        private bool MatrixMustBeUpdated()
        {
            return _lastBx != Bx || _lastBy != By || _imageLoader.MatrixChanged;
        }

        private bool OperationShouldBeExecuted()
        {
            return Bx != 0 || By != 0;
        }
    }
}