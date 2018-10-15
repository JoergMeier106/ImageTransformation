namespace Image_Transformation
{
    public interface IImageLoaderBuilder
    {
        ImageMetaInformation ImageMetaInformation { get; }

        int Layer { get; }

        string Path { get; }

        IImageLoader Build();

        IImageLoaderBuilder SetLayer(int layer);

        IImageLoaderBuilder SetMetaInformation(ImageMetaInformation metaInformation);

        IImageLoaderBuilder SetPath(string path);
    }
}