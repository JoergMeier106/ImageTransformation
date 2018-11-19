using System.IO;

namespace Image_Transformation
{
    public sealed class Image3DMatrixLoader : IImage3DLoader
    {
        private Image3DMatrix _image3DMatrix;
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

        public Image3DMatrix GetImageMatrix()
        {
            MatrixChanged = false;
            if (_lastPath != Path || _lastLayer != Layer)
            {
                MatrixChanged = true;
                _lastPath = Path;
                _lastLayer = Layer;

                ReadMetaInformation();

                byte[] rawBytes = File.ReadAllBytes(Path);
                LayerCount = rawBytes.Length / (Width * Height * BytePerPixel);
                _image3DMatrix = new Image3DMatrix(Height, Width, BytePerPixel, rawBytes);
            }
            return _image3DMatrix;
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