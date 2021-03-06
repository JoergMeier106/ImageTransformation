﻿using System;
using System.IO;

namespace Image_Transformation
{
    /// <summary>
    /// Reads the bytes of an image file and creates an instance of Image2DMatrix out of it.
    /// </summary>
    public sealed class Image2DMatrixLoader : IImage2DLoader
    {
        private byte[] _imageBytes;
        private int _lastLayer;
        private string _lastPath;

        public int BytePerPixel { get; private set; }
        public int Height { get; private set; }
        public int Layer { get; set; }
        public int LayerCount { get; private set; }
        public bool MatrixChanged { get; private set; }
        public double MetaFileBrightnessFactor { get; private set; }
        public string Path { get; set; }
        public int Width { get; private set; }

        public Image2DMatrix GetImageMatrix()
        {
            MatrixChanged = false;
            if (_lastPath != Path || _lastLayer != Layer)
            {
                MatrixChanged = true;
                _lastPath = Path;
                _lastLayer = Layer;

                ReadMetaInformation();

                byte[] rawBytes = File.ReadAllBytes(Path);
                _imageBytes = GetLayerBytes(rawBytes, Layer, BytePerPixel);
                LayerCount = rawBytes.Length / (Width * Height * BytePerPixel);
            }
            return new Image2DMatrix(Height, Width, _imageBytes);
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
            BytePerPixel = metaInformation.BytePerPixel;
            MetaFileBrightnessFactor = metaInformation.BrightnessFactor;
        }
    }
}