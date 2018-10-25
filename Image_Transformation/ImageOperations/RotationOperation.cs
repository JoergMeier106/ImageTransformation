namespace Image_Transformation
{
    public class RotationOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;

        public RotationOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public double Alpha { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged => _imageLoader.MatrixChanged;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;

        public ImageMatrix GetImageMatrix()
        {
            return _imageLoader.GetImageMatrix();
        }

        public TransformationMatrix GetTransformationMatrix()
        {
            ImageMatrix imageMatrix = GetImageMatrix();
            int height = imageMatrix.Height;
            int width = imageMatrix.Width;

            TransformationMatrix transformationMatrix = _imageLoader.GetTransformationMatrix();
            TransformationMatrix rotationMatrix = TransformationMatrix.GetRotationMatrix(Alpha, width / 2, height / 2);
            return transformationMatrix * rotationMatrix;
        }
    }
}