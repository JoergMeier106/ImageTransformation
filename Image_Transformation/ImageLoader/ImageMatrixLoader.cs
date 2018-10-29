using System;
using System.IO;

namespace Image_Transformation
{
    public sealed class ImageMatrixLoader : IImageLoader
    {
        private byte[] _imageBytes;
        private int _lastLayer;
        private string _lastPath;

        public int Height { get; private set; }
        public int Layer { get; set; }
        public int LayerCount { get; private set; }
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor { get; private set; }
        public string Path { get; set; }
        public int Width { get; private set; }

        public ImageMatrix GetImageMatrix()
        {
            MatrixChanged = false;
            if (_lastPath != Path || _lastLayer != Layer)
            {
                MatrixChanged = true;
                _lastPath = Path;
                _lastLayer = Layer;

                ReadMetaInformation();

                byte[] rawBytes = File.ReadAllBytes(Path);
                int bytesPerPixel = rawBytes.Length / (Height * Width);
                _imageBytes = GetLayerBytes(rawBytes, Layer, bytesPerPixel);
                LayerCount = rawBytes.Length / (Width * Height * bytesPerPixel);
            }
            return new ImageMatrix(Height, Width, _imageBytes);
        }

        private byte[] GetLayerBytes(byte[] rawBytes, int layer, int bytesPerPixel)
        {
            int imageSize = Height * Width * bytesPerPixel;
            int imagePosition = imageSize * layer;

            byte[] targetRawBytes = new byte[imageSize];

            Array.Copy(rawBytes, imagePosition, targetRawBytes, 0, imageSize);

            return targetRawBytes;
        }

        private void ReadMetaInformation()
        {
            string metaInformationPath = System.IO.Path.ChangeExtension(Path, ".json");
            ImageMetaInformation metaInformation = JsonParser.Parse<ImageMetaInformation>(metaInformationPath);

            Width = metaInformation.Width;
            Height = metaInformation.Height;
            MetaFileBrightnessFactor = metaInformation.BrightnessFactor;
        }
    }
}