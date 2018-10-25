namespace Image_Transformation
{
    public class ShearingOperation : IImageOperation
    {
        private readonly IImageLoader _imageLoader;

        public ShearingOperation(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        public int Bx { get; set; }
        public int By { get; set; }
        public int LayerCount => _imageLoader.LayerCount;
        public bool MatrixChanged => _imageLoader.MatrixChanged;
        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;

        public ImageMatrix GetImageMatrix()
        {
            return _imageLoader.GetImageMatrix();
        }

        public TransformationMatrix GetTransformationMatrix()
        {
            TransformationMatrix transformationMatrix = _imageLoader.GetTransformationMatrix();
            TransformationMatrix shearingMatrix = TransformationMatrix.GetShearingMatrix(Bx, By);
            return transformationMatrix * shearingMatrix;
        }
    }
}