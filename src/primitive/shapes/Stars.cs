using System;
using System.Collections.Generic;
using System.Text;
using primitive.NetCore.primitive.shapes;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace primitive
{
    public enum StarType
    {
        Pentagram = 5,
        Hexagram
    }

    public class StarRegular : Shape
    {
        public PointF Center { get; set; }
        public double Angle { get; set; }
        public double Radius { get; set; }
        public StarType Type { get; set; }
        private double Ratio { get; set; }

        public StarRegular(Worker worker, StarType type) : this(type)
        {
            Worker = worker;
            var rnd = Worker.Rnd;
            Center = new Point
            {
                X = rnd.Next(Worker.W),
                Y = rnd.Next(Worker.H)
            };
            Radius = rnd.NextDouble() * 32 + 1;
            Angle = rnd.NextDouble() * 360;
        }

        public StarRegular(Worker worker, PointF center, double angle, double radius, StarType type) : this(type)
        {
            Worker = worker;
            Center = center;
            Angle = angle;
            Radius = radius;
        }

        public StarRegular(StarType type)
        {
            Type = type;
            switch (Type)
            {
                case StarType.Pentagram: Ratio = 2.618; break;
                case StarType.Hexagram: Ratio = 1.732; break;
            }
        }

        public override IShape Copy()
        {
            return new StarRegular(Worker, Center, Angle, Radius, Type);
        }

        public override IPath GetPath()
        {
            var pExt = PolygonRegular.CalculateVertices((int)Type, Radius, Angle, Center);
            var pInt = PolygonRegular.CalculateVertices((int)Type, Radius / Ratio, Angle + 360 / 2d / (int)Type, Center);

            PathBuilder pb = new PathBuilder();
            for (int i = 0; i < (int)Type; i++)
                pb.AddLines(pExt[i], pInt[i]);
            pb.CloseFigure();
            return pb.Build();
        }

        public override void Mutate()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            switch (rnd.Next(3))
            {
                case 0:
                    Center = new PointF
                    {
                        X = (float)Util.Clamp(Center.X + rnd.NextGaussian() * 16, 0, w - 1),
                        Y = (float)Util.Clamp(Center.Y + rnd.NextGaussian() * 16, 0, h - 1)
                    }; break;
                case 1:
                    Radius = Util.Clamp(Radius + rnd.NextGaussian() * 16, 1, Math.Min(w, h) - 1); break;
                case 2:
                    Angle = Angle + rnd.NextGaussian() * 32; break;
            }
        }

        public override string SVG(string attrs)
        {
            //todo hexagon
            var p = PolygonRegular.CalculateVertices(5, Radius, Angle, Center);
            return $"<polygon {attrs} points=\"{p[0].X},{p[0].Y} {p[2].X},{p[2].Y} {p[4].X},{p[4].Y} {p[1].X},{p[1].Y} {p[3].X},{p[3].Y}\" />";
        }
    }
}
