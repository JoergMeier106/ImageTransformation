using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    /// <summary>
    /// Creates an image matrix. Most of the methods are lazy, e.g. the changes will be applied after calling Build().
    /// All transofrmations are executed with target to source.
    /// </summary>
    public sealed class Image3DBuilder
    {
        private readonly AdjustBrightness3DOperation _brightnessOperation;
        private readonly IImage3DLoader _imageLoader;
        private readonly Image3DMatrixLoader _imageMatrixLoader;

        public Image3DBuilder()
        {
            //The image loading stack is smaller than in Image2DBuilder. After loading the bytes, only the
            //brightness will be set. Bilinear and projective transformations are not supported in 3D by this
            //implementation.
            _imageMatrixLoader = new Image3DMatrixLoader();
            _brightnessOperation = new AdjustBrightness3DOperation(_imageMatrixLoader);
            _imageLoader = _brightnessOperation;
        }

        #region Rotation
        public double AlphaX { get; private set; }
        public double AlphaY { get; private set; }
        public double AlphaZ { get; private set; }
        #endregion
        /// <summary>
        /// This factor will be applied to each voxel of the image.
        /// </summary>
        public double Brightness => _brightnessOperation.BrightnessFactor;
        #region Shearing
        public double Bxy { get; private set; }
        public double Bxz { get; private set; }
        public double Byx { get; private set; }
        public double Byz { get; private set; }
        public double Bzx { get; private set; }
        public double Bzy { get; private set; }
        #endregion
        #region Shifting
        public int Dx { get; private set; }
        public int Dy { get; private set; }
        public int Dz { get; private set; }
        #endregion
        /// <summary>
        /// The path of the image file.
        /// </summary>
        public string Path => _imageMatrixLoader.Path;
        #region Scaling
        public double Sx { get; private set; }
        public double Sy { get; private set; }
        public double Sz { get; private set; }
        #endregion

        public IEnumerable<WriteableBitmap> Build()
        {
            Image3DMatrix imageMatrix = _imageLoader.GetImageMatrix();
            TransformationMatrix transformationMatrix = GetTransformationMatrix(imageMatrix);
            imageMatrix = ApplyTransformationMatrix(imageMatrix, transformationMatrix);

            return MatrixToBitmapImageConverter.GetImages(imageMatrix);
        }

        public Image3DBuilder RotateX(double alpha)
        {
            AlphaX = alpha;
            return this;
        }

        public Image3DBuilder RotateY(double alpha)
        {
            AlphaY = alpha;
            return this;
        }

        public Image3DBuilder RotateZ(double alpha)
        {
            AlphaZ = alpha;
            return this;
        }

        public Image3DBuilder Scale(double sx, double sy, double sz)
        {
            Sx = sx;
            Sy = sy;
            Sz = sz;
            return this;
        }

        public Image3DBuilder SetBrightness(double brightness)
        {
            _brightnessOperation.BrightnessFactor = brightness;
            _brightnessOperation.UseCustomBrightness = brightness > 0;
            return this;
        }

        public Image3DBuilder SetPath(string path)
        {
            _imageMatrixLoader.Path = path;
            return this;
        }

        public Image3DBuilder Shear(double bxy, double byx, double bxz, double bzx, double byz, double bzy)
        {
            Bxy = bxy;
            Byx = byx;
            Bxz = bxz;
            Bzx = bzx;
            Byz = byz;
            Bzy = bzy;
            return this;
        }

        public Image3DBuilder Shift(int dx, int dy, int dz)
        {
            Dx = dx;
            Dy = dy;
            Dz = dz;
            return this;
        }

        private Image3DMatrix ApplyTransformationMatrix(Image3DMatrix imageMatrix, TransformationMatrix transformationMatrix)
        {
            if (transformationMatrix != TransformationMatrix.UnitMatrix4x4)
            {
                imageMatrix = Image3DMatrix.Transform(imageMatrix, transformationMatrix);
            }

            return imageMatrix;
        }

        private TransformationMatrix GetTransformationMatrix(Image3DMatrix imageMatrix)
        {
            if (imageMatrix != null)
            {
                //The transformations will be concatenated here. However, it is possible that
                //some combinations of parameters do need lead to correct results, because
                //transformations are not cumulative.
                TransformationMatrix transformationMatrix = TransformationMatrix.
                            UnitMatrix4x4.
                            Shear3D(Bxy, Byx, Bxz, Bzx, Byz, Bzy).
                            Scale3D(Sx, Sy, Sz).
                            RotateX3D(AlphaX).
                            RotateY3D(AlphaY).
                            RotateZ3D(AlphaZ).
                            Shift3D(Dx, Dy, Dz);

                return transformationMatrix;
            }
            return TransformationMatrix.UnitMatrix4x4;
        }
    }
}