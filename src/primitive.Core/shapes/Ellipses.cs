using SixLabors.Shapes;
using System;
using System.Collections.Generic;

namespace primitive.Core
{
    public class EllipseStrait : Shape
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Rx { get; set; }
        public int Ry { get; set; }
        public bool IsCircle { get; set; }

        public EllipseStrait(WorkerModel worker, bool isCircle)
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

        public EllipseStrait(WorkerModel worker, int x, int y, int rx, int ry, bool isCircle)
        {
            Worker = worker;
            X = x; Rx = rx;
            Y = y; Ry = ry;
            IsCircle = isCircle;
        }

        public override IPath GetPath()
        {
            return new EllipsePolygon(X, Y, Rx * 2, Ry * 2);
        }

        public override string SVG(string attrs)
        {
            return $"<ellipse {attrs} cx=\"{X}\" cy=\"{Y}\" rx=\"{Rx}\" ry=\"{Ry}\" />";
        }

        public override IShape Copy()
        {
            return new EllipseStrait(Worker, X, Y, Rx, Ry, IsCircle);
        }

        public override void Mutate()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            switch (rnd.Next(3))
            {
                case 0:
                    X = (X + (int)(rnd.NextGaussian() * 16)).Clamp(0, w - 1);
                    Y = (Y + (int)(rnd.NextGaussian() * 16)).Clamp(0, h - 1);
                    break;
                case 1:
                    Rx = (Rx + (int)(rnd.NextGaussian() * 16)).Clamp(1, w - 1);
                    if (IsCircle)
                        Ry = Rx;
                    break;
                case 2:
                    Ry = (Ry + (int)(rnd.NextGaussian() * 16)).Clamp(1, h - 1);
                    if (IsCircle)
                        Rx = Ry;
                    break;
            }
        }

        public override List<ScanlineModel> Rasterize()
        {
            var w = Worker.W;
            var h = Worker.H;
            var lines = new List<ScanlineModel>();
            var aspect = (double)Rx / (double)Ry;
            for (int dy = 0; dy < Ry; dy++)
            {
                var y1 = Y - dy;
                var y2 = Y + dy;
                if ((y1 < 0 || y1 >= h) && (y2 < 0 || y2 >= h))
                    continue;
                var s = (int)(Math.Sqrt(Ry * Ry - dy * dy) * aspect);
                var x1 = X - s;
                var x2 = X + s;
                if (x1 < 0)
                    x1 = 0;
                if (x2 >= w)
                    x2 = w - 1;
                if (y1 >= 0 && y1 < h)
                    lines.Add(new ScanlineModel { Alpha = 0xffff, X1 = x1, X2 = x2, Y = y1 });
                if (y2 >= 0 && y2 < h && dy > 0)
                    lines.Add(new ScanlineModel { Alpha = 0xffff, X1 = x1, X2 = x2, Y = y2 });
            }
            return lines;
        }
    }

    public class EllipseRotated : Shape
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Rx { get; set; }
        public double Ry { get; set; }
        public double Angle { get; set; }

        public EllipseRotated(WorkerModel worker)
        {
            Worker = worker;
            var rnd = Worker.Rnd;
            X = rnd.NextDouble() * Worker.W;
            Y = rnd.NextDouble() * Worker.H;
            Rx = rnd.NextDouble() * 32 + 1;
            Ry = rnd.NextDouble() * 32 + 1;
            Angle = rnd.NextDouble() * 360;
        }

        public EllipseRotated(WorkerModel worker, double x, double y, double rx, double ry, double a)
        {
            Worker = worker;
            X = x; Rx = rx;
            Y = y; Ry = ry;
            Angle = a;
        }

        public override IPath GetPath()
        {
            return new EllipsePolygon((float)X, (float)Y, (float)Rx * 2, (float)Ry * 2).Rotate((float)Util.Radians(Angle));
        }

        public override string SVG(string attrs)
        {
            return $"<g transform=\"translate({X} {Y}) rotate({Angle}) scale({Rx} {Ry})\"><ellipse {attrs} cx=\"0\" cy=\"0\" rx=\"1\" ry=\"1\" /></g>";
        }

        public override IShape Copy()
        {
            return new EllipseRotated(Worker, X, Y, Rx, Ry, Angle);
        }

        public override void Mutate()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            switch (rnd.Next(3))
            {
                case 0:
                    X = (X + rnd.NextGaussian() * 16).Clamp(0, w - 1);
                    Y = (Y + rnd.NextGaussian() * 16).Clamp(0, h - 1); break;
                case 1:
                    Rx = (Rx + rnd.NextGaussian() * 16).Clamp(1, w - 1);
                    Ry = (Ry + rnd.NextGaussian() * 16).Clamp(1, w - 1); break;
                case 2:
                    Angle = Angle + rnd.NextGaussian() * 32; break;
            }
        }
    }
}
