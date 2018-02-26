using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;

namespace primitive
{
    public class EllipseStrait : IShape
    {
        public Worker Worker { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Rx { get; set; }
        public int Ry { get; set; }
        public bool IsCircle { get; set; }

        public EllipseStrait(Worker worker, bool isCircle)
        {
            Worker = worker;
            IsCircle = isCircle;
            var rnd = Worker.Rnd;
            X = rnd.Next(Worker.W);
            Y = rnd.Next(Worker.H);
            Rx = rnd.Next(32) + 1;
            if (IsCircle)
                Ry = Rx;
            else
                Ry = rnd.Next(32) + 1;

        }

        public EllipseStrait(Worker worker, int x, int y, int rx, int ry, bool isCircle)
        {
            Worker = worker;
            X = x; Rx = rx;
            Y = y; Ry = ry;
            IsCircle = isCircle;
        }

        public void Draw(Image<Rgba32> image, Rgba32 color, double scale)
        {
            //throw new NotImplementedException();
        }

        public string SVG(string attrs)
        {
            return $"<ellipse {attrs} cx=\"{X}\" cy=\"{Y}\" rx=\"{Rx}\" ry=\"{Ry}\" />";
        }

        public IShape Copy()
        {
            return new EllipseStrait(Worker, X, Y, Rx, Ry, IsCircle);
        }

        public void Mutate()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            switch (rnd.Next(3))
            {
                case 0:
                    X = Util.ClampInt(X + (int)(rnd.NextGaussian() * 16), 0, w - 1);
                    Y = Util.ClampInt(Y + (int)(rnd.NextGaussian() * 16), 0, h - 1);
                    break;
                case 1:
                    Rx = Util.ClampInt(Rx + (int)(rnd.NextGaussian() * 16), 1, w - 1);
                    if (IsCircle)
                        Ry = Rx;
                    break;
                case 2:
                    Ry = Util.ClampInt(Ry + (int)(rnd.NextGaussian() * 16), 1, h - 1);
                    if (IsCircle)
                        Rx = Ry;
                    break;
            }
        }

        public List<Scanline> Rasterize()
        {
            var w = Worker.W;
            var h = Worker.H;
            var lines = new List<Scanline>();
            var aspect = (double)Rx / (double)Ry;
            for (int dy = 0; dy < Ry; dy++)
            {
                var y1 = Y - dy;
                var y2 = Y + dy;
                if ((y1 < 0 || y1 >= h) && (y2 < 0 || y2 >= h))
                    continue;
                var s = (int)Math.Sqrt((Ry * Ry - dy * dy) * aspect);
                var x1 = X - s;
                var x2 = X + s;
                if (x1 < 0)
                    x1 = 0;
                if (x2 >= w)
                    x2 = w - 1;
                if (y1 >= 0 && y1 < h)
                    lines.Add(new Scanline { Alpha = 0xffff, X1 = x1, X2 = x2, Y = y1 });
                if (y2 >= 0 && y2 < h && dy > 0)
                    lines.Add(new Scanline { Alpha = 0xffff, X1 = x1, X2 = x2, Y = y2 });
            }
            return lines;
        }
    }
}
