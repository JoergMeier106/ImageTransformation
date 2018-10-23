namespace Image_Transformation
{
    public class ScalingOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;

        public ScalingOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged => _imageLoader.MatrixChanged;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public int Sx { get; set; }
        public int Sy { get; set; }

        public Matrix GetImageMatrix()
        {
            Matrix imageMatrix = _imageLoader.GetImageMatrix();
            return imageMatrix.Scale(Sx, Sy);
        }
    }
}