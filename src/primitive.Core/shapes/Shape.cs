using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Shapes;
using SixLabors.Primitives;

namespace primitive.Core
{
    public enum ShapeType
    {
        Any = 0,
        Triangle,
        Rectangle,
        RotatedRectangle,
        Ellipse,
        RotatedEllipse,
        Circle,
        BezierQuadratic,
        Quadrilateral,
        Square,
        Pentagon,
        Hexagon,
        Octagon,
        FourPointedStar,
        Pentagram,
        Hexagram,
        Crescent
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
        private static readonly Comparer<PointF> comparer = Comparer<PointF>.Create((a, b) => a.X.CompareTo(b.X));
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
            var bot = Util.Clamp((int)bounds.Bottom, 0, h - 1);
            var top = Util.Clamp((int)bounds.Top, 0, h - 1);

            for (int y = bot; y >= top; y--)
            {
                var n = path.FindIntersections(new PointF(bounds.Left, y), new PointF(bounds.Right, y), interscertions, 0);
                Array.Sort<PointF>(interscertions, 0, n, comparer);
                if (n % 2 == 0 && n > 1)
                {
                    for (int i = 0; i < n; i += 2)
                    {
                        lines.Add(new Scanline
                        {
                            Alpha = 0xffff,
                            X1 = Util.Clamp((int)interscertions[i].X, 0, w - 1),
                            X2 = Util.Clamp((int)interscertions[i + 1].X, 0, w - 1),
                            Y = y
                        });
                    }
                }
                else if (n == 1)
                {
                    var x = Util.Clamp((int)interscertions[0].X, 0, w - 1);
                    lines.Add(new Scanline
                    {
                        Alpha = 0xffff,
                        X1 = x,
                        X2 = x,
                        Y = y
                    });
                }
            }
            return lines;
        }
    }
}
