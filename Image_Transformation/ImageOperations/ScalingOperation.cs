namespace Image_Transformation
{
    public class ScalingOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;

        public ScalingOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged => _imageLoader.MatrixChanged;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
        public int Sx { get; set; }
        public int Sy { get; set; }

        public ImageMatrix GetImageMatrix()
        {
            return _imageLoader.GetImageMatrix();
        }

        public TransformationMatrix GetTransformationMatrix()
        {
            TransformationMatrix transformationMatrix = _imageLoader.GetTransformationMatrix();
            TransformationMatrix scalingMatrix = TransformationMatrix.GetScalingMatrix(Sx, Sy);
            return transformationMatrix * scalingMatrix;
        }
    }
}