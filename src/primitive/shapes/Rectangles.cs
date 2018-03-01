using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System.Linq;

namespace primitive
{
    public class RectangleStraight : Shape
    {
        public int X1 { get; set; }
        public int X2 { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }

        public RectangleStraight(Worker worker)
        {
            var rnd = worker.Rnd;
            X1 = rnd.Next(worker.W);
            Y1 = rnd.Next(worker.H);
            X2 = Util.ClampInt(X1 + rnd.Next(32) + 1, 0, worker.W - 1);
            Y2 = Util.ClampInt(Y1 + rnd.Next(32) + 1, 0, worker.H - 1);
            Worker = worker;
        }

        public RectangleStraight(Worker worker, int x1, int y1, int x2, int y2)
        {
            Worker = worker;
            X1 = x1; Y1 = y1;
            X2 = x2; Y2 = y2;
        }

        private void CheckBounds()
        {
            int x1 = X1, y1 = Y1;
            int x2 = X2, y2 = Y2;
            if (x1 > x2) { X1 = x2; X2 = x1; }
            else { X1 = x1; X2 = x2; }
            if (y1 > y2) { Y1 = y2; Y2 = y1; }
            else { Y1 = y1; Y2 = y2; }
        }

        public override IPath GetPath()
        {
            CheckBounds();
            PathBuilder pb = new PathBuilder();
            pb.AddLine(X1, Y1, X1, Y2);
            pb.AddLine(X1, Y2, X2, Y2);
            pb.AddLine(X2, Y2, X2, Y1);
            pb.AddLine(X2, Y1, X1, Y1);
            return pb.Build();
        }

        public override string SVG(string attrs)
        {
            CheckBounds();
            var w = X2 - X1 + 1;
            var h = Y2 - Y1 + 1;
            return $"<rect {attrs} x=\"{X1}\" y=\"{Y1}\" width=\"{w}\" height=\"{h}\" />";
        }

        public override IShape Copy()
        {
            return new RectangleStraight(Worker, X1, Y1, X2, Y2);
        }

        public override void Mutate()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            switch (rnd.Next(2))
            {
                case 0:
                    X1 = Util.ClampInt(X1 + (int)(rnd.NextGaussian() * 16), 0, w - 1);
                    Y1 = Util.ClampInt(Y1 + (int)(rnd.NextGaussian() * 16), 0, h - 1); break;
                case 1:
                    X2 = Util.ClampInt(X2 + (int)(rnd.NextGaussian() * 16), 0, w - 1);
                    Y2 = Util.ClampInt(Y2 + (int)(rnd.NextGaussian() * 16), 0, h - 1); break;
            }
        }

        public override List<Scanline> Rasterize()
        {
            CheckBounds();
            var lines = new List<Scanline>();
            for (int y = Y1; y <= Y2; y++)
                lines.Add(new Scanline() { Y = y, X1 = this.X1, X2 = this.X2, Alpha = 0xffff });
            return lines;
        }
    }

    public class RectangleRotated : Shape
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Sx { get; set; }
        public int Sy { get; set; }
        public int Angle { get; set; }

        public RectangleRotated(Worker worker)
        {
            var rnd = worker.Rnd;
            X = rnd.Next(worker.W);
            Y = rnd.Next(worker.H);
            Sx = rnd.Next(32) + 1;
            Sy = rnd.Next(32) + 1;
            Angle = rnd.Next(360);
            Worker = worker;
            Mutate();
        }

        public RectangleRotated(Worker worker, int x, int y, int sx, int sy, int angle)
        {
            Worker = worker;
            X = x; Y = y;
            Sx = sx; Sy = sy;
            Angle = angle;
        }

        public override IPath GetPath()
        {
            PathBuilder pb = new PathBuilder();
            var x1 = X - Sx / 2;
            var x2 = X + Sx / 2;
            var y1 = Y - Sy / 2;
            var y2 = Y + Sy / 2;
            pb.AddLine(x1, y1, x1, y2);
            pb.AddLine(x1, y2, x2, y2);
            pb.AddLine(x2, y2, x2, y1);
            pb.AddLine(x2, y1, x1, y1);
            return pb.Build().RotateDegree(Angle);
        }

        public override string SVG(string attrs)
        {
            return $"<g transform=\"translate({X} {Y}) rotate({Angle}) scale({Sx} {Sy})\"><rect {attrs} x=\"-0.5\" y=\"-0.5\" width=\"1\" height=\"1\" /></g>";
        }

        public override IShape Copy()
        {
            return new RectangleRotated(Worker, X, Y, Sx, Sy, Angle);
        }

        public override void Mutate()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;

            switch (rnd.Next(3))
            {
                case 0:
                    X = Util.ClampInt(X + (int)(rnd.NextGaussian() * 16), 0, w - 1);
                    Y = Util.ClampInt(Y + (int)(rnd.NextGaussian() * 16), 0, h - 1); break;
                case 1:
                    Sx = Util.ClampInt(Sx + (int)(rnd.NextGaussian() * 16), 0, w - 1);
                    Sy = Util.ClampInt(Sy + (int)(rnd.NextGaussian() * 16), 0, h - 1); break;
                case 3:
                    Angle = Angle + (int)(rnd.NextGaussian() * 32); break;
            }
            //while (!Valid())
            //{
            //    Sx = Util.ClampInt(Sx + (int)(rnd.NextGaussian() * 16), 0, w - 1);
            //    Sy = Util.ClampInt(Sy + (int)(rnd.NextGaussian() * 16), 0, h - 1);
            //}
        }

        private bool Valid()
        {
            int a = Sx, b = Sy;
            if (a < b) { a = Sy; b = Sx; }
            var aspect = (double)a / (double)b;
            return aspect <= 5;
        }
    }
}
