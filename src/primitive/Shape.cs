using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;

namespace primitive
{
    public interface IShape
    {
        List<Scanline> Rasterize();
        IShape Copy();
        void Mutate();
        void Draw(Image<Rgba32> image, Rgba32 color, double scale);
        string SVG(string attrs);
    }

    public enum ShapeType
    {
        ShapeTypeAny = 0,
        ShapeTypeTriangle,
        ShapeTypeRectangle,
        ShapeTypeEllipse,
        ShapeTypeCircle,
        ShapeTypeRotatedRectangle,
        ShapeTypeQuadratic,
        ShapeTypeRotatedEllipse,
        ShapeTypePolygon
    }
}
