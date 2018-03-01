using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.Shapes;
using SixLabors.Primitives;

namespace primitive
{
    public enum ShapeType
    {
        ShapeTypeAny = 0,
        ShapeTypeTriangle,
        ShapeTypeRectangle,
        ShapeTypeEllipse,
        ShapeTypeCircle,
        ShapeTypeRotatedRectangle,
        ShapeTypeBezierQuadratic,
        ShapeTypeRotatedEllipse,
        ShapeTypePolygon
    }

    public interface IShape
    {
        Worker Worker { get; set; }
        IShape Copy();
        IPath GetPath();
        void Draw(Image<Rgba32> image, Rgba32 color, double scale);
        void Mutate();
        string SVG(string attrs);
        List<Scanline> Rasterize();
    }

    public abstract class Shape : IShape
    {
        public Worker Worker { get; set; }
        public abstract IShape Copy();
        public abstract IPath GetPath();
        public abstract void Mutate();
        public abstract string SVG(string attrs);

        public virtual void Draw(Image<Rgba32> image, Rgba32 color, double scale)
        {
            image.Mutate(im => im
                .Fill(color, GetPath().Transform(Matrix3x2.CreateScale((float)scale))));
        }

        public virtual List<Scanline> Rasterize()
        {
            List<Scanline> lines = new List<Scanline>();

            var w = Worker.W;
            var h = Worker.H;
            var path = GetPath();
            PointF[] interscertions = new PointF[path.MaxIntersections];

            var bounds = path.Bounds;
            var bot = Util.ClampInt((int)bounds.Bottom, 0, h - 1);
            var top = Util.ClampInt((int)bounds.Top, 0, h - 1);
            for (int i = bot; i >= top; i--)
            {
                var n = path.FindIntersections(new PointF(bounds.Left, i), new PointF(bounds.Right, i), interscertions, 0);
                var x = Util.ClampInt((int)interscertions[0].X, 0, w - 1);
                if (n == 1)
                {
                    lines.Add(new Scanline
                    {
                        Alpha = 0xffff,
                        X1 = x,
                        X2 = x,
                        Y = i
                    });
                    continue;
                }
                if (n == 2)
                {
                    var x1 = x;
                    var x2 = Util.ClampInt((int)interscertions[1].X, 0, w - 1);
                    lines.Add(new Scanline
                    {
                        Alpha = 0xffff,
                        X1 = Math.Min(x1, x2),
                        X2 = Math.Max(x1, x2),
                        Y = i
                    });
                }
                //todo something for complex shapes
            }
            return lines;
        }
    }
}
