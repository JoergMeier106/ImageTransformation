﻿namespace Image_Transformation
{
    /// <summary>
    /// Applies a factor to each pixel of an Image2DMatrix
    /// </summary>
    public class AdjustBrightness2DOperation : IImage2DOperation
    {
        private readonly IImage2DLoader _imageLoader;
        private Image2DMatrix _cashedMatrix;
        private double _lastBrightnessFactor;

        public AdjustBrightness2DOperation(IImage2DLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double BrightnessFactor { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public bool UseCustomBrightness { get; set; }

        public Image2DMatrix GetImageMatrix()
        {
            MatrixChanged = false;
            Image2DMatrix sourceMatrix = _imageLoader.GetImageMatrix();
            if (UseCustomBrightness)
            {
                AdjustBrightness(sourceMatrix, BrightnessFactor);
            }
            else
            {
                AdjustBrightness(sourceMatrix, MetaFileBrightnessFactor);
            }

            return _cashedMatrix;
        }

        private void AdjustBrightness(Image2DMatrix sourceMatrix, double brightnessFactor)
        {
            if (_lastBrightnessFactor != brightnessFactor || _imageLoader.MatrixChanged)
            {
                MatrixChanged = true;
                _lastBrightnessFactor = brightnessFactor;
                _cashedMatrix = sourceMatrix * brightnessFactor;
            }
        }
    }
}