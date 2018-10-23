using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public class BitmapBuilder : IBitmapBuilder
    {
        private Matrix _cashedMatrix;
        private int _originalHeight;
        private double _metaFileBrightnessFactor;
        private int _originalWidth;

        public BitmapBuilder(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ReadMetaInformation();
            ReadBytesToMatrix();
        }

        public double Alpha { get; private set; }

        public double Brightness { get; private set; }

        public int Bx { get; private set; }

        public int By { get; private set; }

        public int Dx { get; private set; }

        public int Dy { get; private set; }

        public int Layer { get; private set; }

        public int LayerCount { get; private set; }

        public string Path { get; private set; }

        public int Sx { get; private set; }

        public int Sy { get; private set; }

        public BitmapSource GetBitmap()
        {
            int currentWidth = _cashedMatrix.Width;
            int currentHeight = _cashedMatrix.Height;

            byte[] imageBytes = _cashedMatrix.GetBytes();

            int bitsPerPixel = 16;
            int stride = (currentWidth * bitsPerPixel + 7) / 8;

            return BitmapSource.Create(currentWidth, currentHeight, 96, 96, PixelFormats.Gray16, null, imageBytes, stride);
        }

        public void Rotate(double alpha)
        {
            Alpha = alpha;
            _cashedMatrix = _cashedMatrix.Rotate(alpha);
        }

        public void Scale(int sx, int sy)
        {
            Sx = sx;
            Sy = sy;

            _cashedMatrix = _cashedMatrix.Scale(sx, sy);
        }

        public void SetBrightness(double brightness)
        {
            brightness = brightness == 0 ? _metaFileBrightnessFactor : brightness;
            double brightnessFactor = brightness;

            if (Brightness != 0 && Brightness != brightness)
            {
                brightnessFactor = brightness / Brightness;
            }

            Brightness = brightness;

            _cashedMatrix = _cashedMatrix * brightnessFactor;
        }

        public void SetLayer(int layer)
        {
            Layer = layer;

            ReadBytesToMatrix();
        }

        public void Shear(int bx, int by)
        {
            if (bx != 0 || by != 0)
            {
                _cashedMatrix = Matrix.Transform(_cashedMatrix, (x, y) =>
                {
                    x = x - Bx * y;
                    y = y - By * x;

                    return (x, y);
                });

                _cashedMatrix = _cashedMatrix.Shear(bx, by);

                Bx = bx;
                By = by;
                
            }
        }

        public void Shift(int dx, int dy)
        {
            if (dx != 0 || dy != 0)
            {
                _cashedMatrix = _cashedMatrix.Shift(dx - Dx, dy - Dy);

                Dx = dx;
                Dy = dy;
            }
        }

        private byte[] GetLayerBytes(byte[] rawBytes, int layer)
        {
            int imageSize = _originalHeight * _originalWidth * 2;
            int imagePosition = imageSize * layer;

            byte[] targetRawBytes = new byte[imageSize];

            Array.Copy(rawBytes, imagePosition, targetRawBytes, 0, imageSize);

            return targetRawBytes;
        }

        private void ReadBytesToMatrix()
        {
            byte[] rawBytes = File.ReadAllBytes(Path);
            byte[] imageBytes = GetLayerBytes(rawBytes, Layer);
            LayerCount = rawBytes.Length / (_originalWidth * _originalHeight * 2) - 1;

            _cashedMatrix = new Matrix(_originalHeight, _originalWidth, imageBytes);
            SetBrightness(Brightness);
        }

        private void ReadMetaInformation()
        {
            string metaInformationPath = System.IO.Path.ChangeExtension(Path, ".json");
            ImageMetaInformation metaInformation = JsonParser.Parse<ImageMetaInformation>(metaInformationPath);

            _originalWidth = metaInformation.Width;
            _originalHeight = metaInformation.Height;
            _metaFileBrightnessFactor = metaInformation.BrightnessFactor;
        }
    }
}