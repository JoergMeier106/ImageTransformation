namespace Image_Transformation
{
    public class ImageLoaderBuilder : IImageLoaderBuilder
    {
        public ImageMetaInformation ImageMetaInformation { get; private set; }
        public int Layer { get; private set; }
        public string Path { get; private set; }

        public IImageLoader Build()
        {
            string fileExtension = System.IO.Path.GetExtension(Path);

            if (fileExtension.ToLower() == "raw")
            {
                return new MedicalRawImageLoader(Path, Layer, ImageMetaInformation.Width, ImageMetaInformation.Height);
            }
            else
            {
                return new CommonFormatImageLoader(Path);
            }
        }

        public IImageLoaderBuilder SetLayer(int layer)
        {
            Layer = layer;
            return this;
        }

        public IImageLoaderBuilder SetMetaInformation(ImageMetaInformation metaInformation)
        {
            ImageMetaInformation = metaInformation;
            return this;
        }

        public IImageLoaderBuilder SetPath(string path)
        {
            Path = path;
            return this;
        }
    }
}