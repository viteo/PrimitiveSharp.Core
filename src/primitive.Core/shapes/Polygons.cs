using SixLabors.Primitives;
using SixLabors.Shapes;
using System;
using System.Collections.Generic;

namespace primitive.Core
{
    public class Polygon : Shape
    {
        public int Vertices { get; set; }
        public bool Convex { get; set; }
        public PointF[] XY { get; set; }

        public Polygon(WorkerModel worker, int vertices, bool convex)
        {
            Worker = worker;
            Vertices = vertices;
            Convex = convex;
            var rnd = Worker.Rnd;
            XY = new PointF[vertices];
            XY[0].X = (float)rnd.NextDouble() * (float)Worker.W;
            XY[0].Y = (float)rnd.NextDouble() * (float)Worker.H;
            for (int i = 1; i < vertices; i++)
            {
                XY[i].X = XY[0].X + (float)rnd.NextDouble() * 40 - 20;
                XY[i].Y = XY[0].Y + (float)rnd.NextDouble() * 40 - 20;
            }
        }

        public Polygon(WorkerModel worker, int vertices, bool convex, PointF[] xy)
        {
            Worker = worker;
            Vertices = vertices;
            Convex = convex;
            XY = new PointF[Vertices];
            xy.CopyTo(XY, 0);
        }

        public override IShape Copy()
        {
            return new Polygon(Worker, Vertices, Convex, XY);
        }

        public override IPath GetPath()
        {
            PathBuilder pb = new PathBuilder();
            pb.AddLines(XY);
            pb.CloseFigure();
            return pb.Build();
        }

        protected override void MutateImpl()
        {
            const int m = 16;
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            for (; ; )
            {
                if (rnd.NextDouble() < 0.25)
                {
                    var i = rnd.Next(Vertices);
                    var j = rnd.Next(Vertices);
                    (XY[i].X, XY[i].Y, XY[j].X, XY[j].Y) = (XY[j].X, XY[j].Y, XY[i].X, XY[i].Y);
                }
                else
                {
                    var i = rnd.Next(Vertices);
                    XY[i].X = (float)(XY[i].X + rnd.NextGaussian() * 16).Clamp(-m, w - 1 + m);
                    XY[i].Y = (float)(XY[i].Y + rnd.NextGaussian() * 16).Clamp(-m, h - 1 + m);
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
            for (int a = 0; a < Vertices; a++)
            {
                var i = (a + 0) % Vertices;
                var j = (a + 1) % Vertices;
                var k = (a + 2) % Vertices;
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

    public class PolygonRegular : Shape
    {
        public PointF Center { get; set; }
        public double Angle { get; set; }
        public double Radius { get; set; }
        public int Vertices { get; set; }

        public PolygonRegular(WorkerModel worker, int vertices)
        {
            Worker = worker;
            var rnd = Worker.Rnd;
            Vertices = vertices;
            Center = new Point
            {
                X = rnd.Next(Worker.W),
                Y = rnd.Next(Worker.H)
            };
            Radius = rnd.NextDouble() * 32 + 1;
            Angle = rnd.NextDouble() * 360;
        }

        public PolygonRegular(WorkerModel worker, PointF center, double angle, double radius, int vertices)
        {
            Worker = worker;
            Vertices = vertices;
            Center = center;
            Angle = angle;
            Radius = radius;
        }

        public override IShape Copy()
        {
            return new PolygonRegular(Worker, Center, Angle, Radius, Vertices);
        }

        public override IPath GetPath()
        {
            var points = PolygonRegular.CalculateVertices(Vertices, Radius, Angle, Center);
            PathBuilder pb = new PathBuilder();
            pb.AddLines(points);
            pb.CloseFigure();
            return pb.Build();
        }

        protected override void MutateImpl()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            switch (rnd.Next(3))
            {
                case 0:
                    Center = new PointF
                    {
                        X = (float)(Center.X + rnd.NextGaussian() * 16).Clamp(0, w - 1),
                        Y = (float)(Center.Y + rnd.NextGaussian() * 16).Clamp(0, h - 1)
                    }; break;
                case 1:
                    Radius = (Radius + rnd.NextGaussian() * 16).Clamp(1, Math.Min(w, h) - 1); break;
                case 2:
                    Angle = Angle + rnd.NextGaussian() * 32; break;
            }
        }

        public override string SVG(string attrs)
        {
            var p = PolygonRegular.CalculateVertices(Vertices, Radius, Angle, Center);
            List<string> points = new List<string>();
            foreach (var xy in p)
                points.Add($"{xy.X},{xy.Y}");
            return $"<polygon {attrs} points=\"" + String.Join(",", points) + "\" />";
        }

        public static PointF[] CalculateVertices(int sides, double radius, double startingAngle, PointF center)
        {
            if (sides < 3)
                throw new ArgumentException("Polygon must have 3 sides or more.");

            PointF[] points = new PointF[sides];
            float step = 360.0f / sides;
            double angle = startingAngle;

            for (int i = 0; i < sides; i++)
            {
                points[i] = DegreesToXY(angle, radius, center);
                angle += step;
            }

            return points;
        }

        /// <summary>
        /// Calculates a point that is at an angle from the origin (0 is to the right)
        /// </summary>
        private static PointF DegreesToXY(double degrees, double radius, PointF origin)
        {
            PointF xy = new PointF();
            double radians = degrees * Math.PI / 180.0;

            xy.X = (float)(Math.Cos(radians) * radius) + origin.X;
            xy.Y = (float)(Math.Sin(-radians) * radius) + origin.Y;

            return xy;
        }
    }
}
