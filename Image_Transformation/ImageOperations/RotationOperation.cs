namespace Image_Transformation
{
    public class RotationOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;

        public RotationOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double Alpha { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged => _imageLoader.MatrixChanged;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;

        public Matrix GetImageMatrix()
        {
            Matrix imageMatrix = _imageLoader.GetImageMatrix();
            return imageMatrix.Rotate(Alpha);
        }
    }
}