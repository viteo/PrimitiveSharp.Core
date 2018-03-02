using System;
using System.Collections.Generic;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace primitive
{
    public class Polygon : Shape
    {
        public int Order { get; set; }
        public bool Convex { get; set; }
        public PointF[] XY { get; set; }

        public Polygon(Worker worker, int order, bool convex)
        {
            Worker = worker;
            Order = order;
            Convex = convex;
            var rnd = Worker.Rnd;
            XY = new PointF[order];
            XY[0].X = (float)rnd.NextDouble() * (float)Worker.W;
            XY[0].Y = (float)rnd.NextDouble() * (float)Worker.H;
            for (int i = 1; i < order; i++)
            {
                XY[i].X = XY[0].X + (float)rnd.NextDouble() * 40 - 20;
                XY[i].Y = XY[0].Y + (float)rnd.NextDouble() * 40 - 20;
            }
        }

        public Polygon(Worker worker, int order, bool convex, PointF[] xy)
        {
            Worker = worker;
            Order = order;
            Convex = convex;
            XY = new PointF[Order];
            xy.CopyTo(XY, 0);
        }

        public override IShape Copy()
        {
            return new Polygon(Worker, Order, Convex, XY);
        }

        public override IPath GetPath()
        {
            PathBuilder pb = new PathBuilder();
            pb.AddLines(XY);
            pb.CloseFigure();
            return pb.Build();
        }

        public override void Mutate()
        {
            const int m = 16;
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            for (; ; )
            {
                if (rnd.NextDouble() < 0.25)
                {
                    var i = rnd.Next(Order);
                    var j = rnd.Next(Order);
                    (XY[i].X, XY[i].Y, XY[j].X, XY[j].Y) = (XY[j].X, XY[j].Y, XY[i].X, XY[i].Y);
                }
                else
                {
                    var i = rnd.Next(Order);
                    XY[i].X = (float)Util.Clamp(XY[i].X + rnd.NextGaussian() * 16, -m, w - 1 + m);
                    XY[i].Y = (float)Util.Clamp(XY[i].Y + rnd.NextGaussian() * 16, -m, h - 1 + m);
                }
                if (Valid())
                    break;
            }
        }
        
        public override string SVG(string attrs)
        {
            List<string> points = new List<string>();
            foreach (var xy in XY)
                points.Add($"{xy.X},{xy.Y}");
            return $"<polygon {attrs} points=\"" + String.Join(",", points) + "\" />";
        }

        private bool Valid()
        {
            if (!Convex)
                return true;
            bool sign = false;
            for (int a = 0; a < Order; a++)
            {
                var i = (a + 0) % Order;
                var j = (a + 1) % Order;
                var k = (a + 2) % Order;
                var c = cross3(XY[i].X, XY[i].Y, XY[j].X, XY[j].Y, XY[k].X, XY[k].Y);
                if (a == 0)
                    sign = c > 0;
                else if (c > 0 != sign)
                    return false;
            }
            return true;
        }

        private float cross3(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            var dx1 = x2 - x1;
            var dy1 = y2 - y1;
            var dx2 = x3 - x2;
            var dy2 = y3 - y2;
            return dx1 * dy2 - dy1 * dx2;
        }
    }
}
