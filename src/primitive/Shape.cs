using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace primitive
{
    public interface IShape
    {
        List<Scanline> Rasterize();
        IShape Copy();
        void Mutate();
        void Draw(Graphics dc, SolidBrush brush, double scale);
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
