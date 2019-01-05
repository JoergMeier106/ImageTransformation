using System.IO;

namespace Image_Transformation
{
    /// <summary>
    /// Reads the bytes of an image file and creates an instance of Image3DMatrix out of it.
    /// </summary>
    public sealed class Image3DMatrixLoader : IImage3DLoader
    {
        private int _lastLayer;
        private string _lastPath;
        public int BytePerPixel { get; private set; }
        public int Height { get; private set; }
        public int Layer { get; set; }
        public int LayerCount { get; private set; }
        public double MetaFileBrightnessFactor { get; private set; }
        public string Path { get; set; }
        public int Width { get; private set; }

        public Image3DMatrix GetImageMatrix()
        {
            _lastPath = Path;
            _lastLayer = Layer;

            ReadMetaInformation();

            byte[] rawBytes = File.ReadAllBytes(Path);
            LayerCount = rawBytes.Length / (Width * Height * BytePerPixel);
            return new Image3DMatrix(Height, Width, BytePerPixel, rawBytes);
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