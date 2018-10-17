using System;
using System.IO;

namespace Image_Transformation
{
    public sealed class ImageMatrixLoader : IImageLoader
    {
        private readonly Matrix _imageMatrix;

        public ImageMatrixLoader(string path, int layer)
        {
            Path = path;
            ReadMetaInformation();

            byte[] rawBytes = File.ReadAllBytes(Path);
            byte[] imageBytes = GetLayerBytes(rawBytes, layer);
            LayerCount = rawBytes.Length / (Width * Height * 2);

            _imageMatrix = new Matrix(Height, Width, imageBytes);
        }

        public double BrightnessFactor { get; private set; }

        public int Height { get; private set; }

        public int LayerCount { get; private set; }

        public string Path { get; private set; }

        public int Width { get; private set; }

        public Matrix GetImageMatrix() => _imageMatrix;

        private byte[] GetLayerBytes(byte[] rawBytes, int layer)
        {
            int imageSize = Height * Width * 2;
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
            BrightnessFactor = metaInformation.BrightnessFactor;
        }
    }
}