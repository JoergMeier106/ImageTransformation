//using System;
//using System.Collections.Generic;
//using System.Windows;

//namespace Image_Transformation
//{
//    public class ProjectiveMappingOperation : IImageOperation
//    {
//        private const double RATIO = 1.414;
//        private readonly IImageLoader _imageLoader;

//        public ProjectiveMappingOperation(IImageLoader imageLoader)
//        {
//            _imageLoader = imageLoader;
//        }

//        public int LayerCount => _imageLoader.LayerCount;
//        public bool MatrixChanged { get; private set; }
//        public double MetaFileBrightnessFactor => _imageLoader.MetaFileBrightnessFactor;
//        public Rectangel SourceRectangel { get; set; }
//        public Rectangel TargetRectangel { get; private set; }
//        public Rectangel UnitRectangel { get; private set; }

//        public ImageMatrix GetImageMatrix()
//        {
//            ImageMatrix imageMatrix = _imageLoader.GetImageMatrix();
//            if (SourceRectangel != null)
//            {
                

//                return ImageMatrix.Transform(projectedMatrix, projectiveMapping);
//            }
//            return imageMatrix;
//        }
//    }
//}