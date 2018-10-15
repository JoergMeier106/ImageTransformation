namespace Image_Transformation
{
    public struct ImageMetaInformation
    {
        public ImageMetaInformation(int height, int width)
        {
            Height = height;
            Width = width;
        }

        public int Height { get; }
        public int Width { get; }
    }
}