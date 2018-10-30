﻿using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Image_Transformation
{
    public interface IImageMatrixBuilder
    {
        double Alpha { get; }
        double Brightness { get; }
        int Bx { get; }
        int By { get; }
        int Dx { get; }
        int Dy { get; }
        int Layer { get; }
        int LayerCount { get; }
        string Path { get; }
        Quadrilateral SourceQuadrilateral { get; }
        double Sx { get; }
        double Sy { get; }
        int TargetImageHeight { get; }
        int TargetImageWidth { get; }
        bool SourceToTargetEnabled { get; }

        ImageMatrix Build();

        IImageMatrixBuilder MapBilinear(Quadrilateral sourceQuadrilateral);

        IImageMatrixBuilder Project(Quadrilateral sourceQuadrilateral);

        IImageMatrixBuilder Rotate(double alpha);

        IImageMatrixBuilder Scale(double sx, double sy);

        IImageMatrixBuilder SetBrightness(double brightness);

        IImageMatrixBuilder SetLayer(int layer);

        IImageMatrixBuilder SetPath(string path);

        IImageMatrixBuilder Shear(int bx, int by);

        IImageMatrixBuilder Shift(int dx, int dy);

        IImageMatrixBuilder SetSourceToTargetEnabled(bool value);

        IImageMatrixBuilder SetTargetImageHeight(int imageHeight);

        IImageMatrixBuilder SetTargetImageWidth(int imageWidth);
    }
}