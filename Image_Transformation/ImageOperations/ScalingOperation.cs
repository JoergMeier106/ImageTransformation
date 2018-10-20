using System;

namespace Image_Transformation
{
    public class ScalingOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;
        private Matrix _cashedMatrix;
        private double _lastSx;
        private double _lastSy;

        public ScalingOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int Height { get; private set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public string Path => _imageLoader.Path;
        public int Sy { get; set; }
        public int Sx { get; set; }
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

                    _lastSx = Sx;
                    _lastSy = Sy;

                    Height = imageMatrix.Height * Sy;
                    Width = imageMatrix.Width * Sx;

                    Matrix scaledMatrix = new Matrix(Height, Width, new byte[Height * Width * 2]);
                    _cashedMatrix = Matrix.Scale(imageMatrix, scaledMatrix, Sx, Sy);
                }

                return _cashedMatrix;
            }
            else
            {
                MatrixChanged = true;

                Height = _imageLoader.Height;
                Width = _imageLoader.Width;

                _lastSx = Sx;
                _lastSy = Sy;

                return imageMatrix;
            }
        }

        private bool MatrixMustBeUpdated()
        {
            return _lastSx != Sx || _lastSy != Sy || _imageLoader.MatrixChanged;
        }

        private bool OperationShouldBeExecuted()
        {
            return (Sx != 1 || Sy != 1) && (Sx != 0 && Sy != 0);
        }
    }
}