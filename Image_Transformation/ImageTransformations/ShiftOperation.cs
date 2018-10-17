namespace Image_Transformation
{
    public class ShiftOperation : IImageOperation
    {
        private readonly int _dx;
        private readonly int _dy;
        private readonly IImageLoader _imageLoader;

        public ShiftOperation(IImageLoader imageLoader, int dx, int dy)
        {
            _imageLoader = imageLoader;
            _dx = dx;
            _dy = dy;
        }

        public double BrightnessFactor => _imageLoader.BrightnessFactor;
        public int Height => _imageLoader.Height;
        public int LayerCount => _imageLoader.LayerCount;
        public string Path => _imageLoader.Path;
        public int Width => _imageLoader.Width;

        public Matrix GetImageMatrix()
        {
            return _imageLoader.GetImageMatrix().Shift(_dx, _dy);
        }
    }
}