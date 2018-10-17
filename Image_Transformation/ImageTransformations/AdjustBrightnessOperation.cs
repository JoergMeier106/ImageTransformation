namespace Image_Transformation
{
    public class AdjustBrightnessOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;

        public AdjustBrightnessOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double BrightnessFactor => _imageLoader.BrightnessFactor;
        public int Height => _imageLoader.Height;
        public int LayerCount => _imageLoader.LayerCount;
        public string Path => _imageLoader.Path;
        public int Width => _imageLoader.Width;

        public Matrix GetImageMatrix()
        {
            return _imageLoader.GetImageMatrix() * BrightnessFactor;
        }
    }
}