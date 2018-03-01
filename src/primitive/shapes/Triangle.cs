using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.Shapes;

namespace primitive
{
    public class Triangle : Shape
    {
        public int X1 { get; set; }
        public int X2 { get; set; }
        public int X3 { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }
        public int Y3 { get; set; }

        public Triangle(Worker worker)
        {
            Worker = worker;
            var rnd = Worker.Rnd;
            X1 = rnd.Next(Worker.W);
            Y1 = rnd.Next(Worker.H);
            X2 = X1 + rnd.Next(31) - 15;
            Y2 = Y1 + rnd.Next(31) - 15;
            X3 = X1 + rnd.Next(31) - 15;
            Y3 = Y1 + rnd.Next(31) - 15;
            Mutate();
        }

        public Triangle(Worker worker, int x1, int x2, int x3, int y1, int y2, int y3)
        {
            Worker = worker;
            X1 = x1; X2 = x2; X3 = x3;
            Y1 = y1; Y2 = y2; Y3 = y3;
        }

        public override string SVG(string attrs)
        {
            return $"<polygon {attrs} points=\"{X1},{Y1} {X2},{Y2} {X3},{Y3}\" />";
        }

        public override IShape Copy()
        {
            return new Triangle(Worker, X1, X2, X3, Y1, Y2, Y3);
        }

        public override void Mutate()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            const int m = 16;
            do
            {
                switch (rnd.Next(3))
                {
                    case 0:
                        X1 = Util.ClampInt(X1 + (int)(rnd.NextGaussian() * 16), -m, w - 1 + m);
                        Y1 = Util.ClampInt(Y1 + (int)(rnd.NextGaussian() * 16), -m, h - 1 + m);
                        break;
                    case 1:
                        X2 = Util.ClampInt(X2 + (int)(rnd.NextGaussian() * 16), -m, w - 1 + m);
                        Y2 = Util.ClampInt(Y2 + (int)(rnd.NextGaussian() * 16), -m, h - 1 + m);
                        break;
                    case 2:
                        X3 = Util.ClampInt(X3 + (int)(rnd.NextGaussian() * 16), -m, w - 1 + m);
                        Y3 = Util.ClampInt(Y3 + (int)(rnd.NextGaussian() * 16), -m, h - 1 + m);
                        break;
                }
            } while (Valid());
        }

        private bool Valid()
        {
            const int minDegrees = 15;
            double a1, a2, a3;
            {
                var x1 = (double)X2 - X1;
                var y1 = (double)Y2 - Y1;
                var x2 = (double)X3 - X1;
                var y2 = (double)Y3 - Y1;
                var d1 = Math.Sqrt(x1 * x1 + y1 * y1);
                var d2 = Math.Sqrt(x2 * x2 + y2 * y2);
                x1 /= d1;
                y1 /= d1;
                x2 /= d2;
                y2 /= d2;
                a1 = Util.Degrees(Math.Acos(x1 * x2 + y1 * y2));
            }
            {
                var x1 = (double)X1 - X2;
                var y1 = (double)Y1 - Y2;
                var x2 = (double)X3 - X2;
                var y2 = (double)Y3 - Y2;
                var d1 = Math.Sqrt(x1 * x1 + y1 * y1);
                var d2 = Math.Sqrt(x2 * x2 + y2 * y2);
                x1 /= d1;
                y1 /= d1;
                x2 /= d2;
                y2 /= d2;
                a2 = Util.Degrees(Math.Acos(x1 * x2 + y1 * y2));
            }
            a3 = 180 - a1 - a2;
            return a1 > minDegrees && a2 > minDegrees && a3 > minDegrees;
        }

        public override List<Scanline> Rasterize()
        {
            var buf= new List<Scanline>();
            buf = rasterizeTriangle(X1, Y1, X2, Y2, X3, Y3, buf);
            return Scanline.CropScanlines(buf, Worker.W, Worker.H);
        }

        private List<Scanline> rasterizeTriangle(int x1, int y1, int x2, int y2, int x3, int y3, List<Scanline> buf)
        {
            if (y1 > y3)
            {
                (x1, x3) = (x3, x1);
                (y1, y3) = (y3, y1);
            }
            if (y1 > y2)
            {
                (x1, x2) = (x2, x1);
                (y1, y2) = (y2, y1);
            }
            if (y2 > y3)
            {
                (x2, x3) = (x3, x2);
                (y2, y3) = (y3, y2);
            }
            if (y2 == y3)
                return rasterizeTriangleBottom(x1, y1, x2, y2, x3, y3, buf);
            else if (y1 == y2)
                return rasterizeTriangleTop(x1, y1, x2, y2, x3, y3, buf);
            else
            {
                var x4 = x1 + (int) (((double) (y2 - y1) / (double) (y3 - y1)) * (double) (x3 - x1));
                var y4 = y2;
                buf = rasterizeTriangleBottom(x1, y1, x2, y2, x4, y4, buf);
                buf = rasterizeTriangleTop(x2, y2, x4, y4, x3, y3, buf);
                return buf;
            }
        }

        private List<Scanline> rasterizeTriangleBottom(int x1, int y1, int x2, int y2, int x3, int y3, List<Scanline> buf)
        {
            var s1 = (double)(x2 - x1) / (double)(y2 - y1);
            var s2 = (double)(x3 - x1) / (double)(y3 - y1);
            var ax = (double)x1;
            var bx = (double)x1;
            for (int y = y1; y <= y2; y++)
            {
                var a = (int)ax;
                var b = (int)bx;
                ax += s1;
                bx += s2;
                if (a > b)
                    (a, b) = (b, a);
                buf.Add(new Scanline() { Alpha = 0xffff, X1 = a, X2 = b, Y = y });
            }
            return buf;
        }

        private List<Scanline> rasterizeTriangleTop(int x1, int y1, int x2, int y2, int x3, int y3, List<Scanline> buf)
        {
            var s1 = (double)(x3 - x1) / (double)(y3 - y1);
            var s2 = (double)(x3 - x2) / (double)(y3 - y2);
            var ax = (double)x3;
            var bx = (double)x3;
            for (int y = y3; y > y1; y--)
            {
                ax -= s1;
                bx -= s2;
                var a = (int)ax;
                var b = (int)bx;
                if (a > b)
                    (a, b) = (b, a);
                buf.Add(new Scanline() { Alpha = 0xffff, X1 = a, X2 = b, Y = y });
            }
            return buf;
        }

        public override IPath GetPath()
        {
            PathBuilder pb = new PathBuilder();
            pb.AddLine(X1, Y1, X2, Y2);
            pb.AddLine(X2, Y2, X3, Y3);
            pb.AddLine(X3, Y3, X1, Y1);
            return pb.Build();
        }
    }
}
