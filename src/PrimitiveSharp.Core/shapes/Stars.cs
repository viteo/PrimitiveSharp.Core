using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections.Generic;

namespace PrimitiveSharp.Core
{
    public class StarRegular : PolygonRegular
    {
        private double Ratio
        {
            get
            {
                switch (Vertices)
                {
                    case 4: 
                    case 5: return 2.618;
                    case 6: return 1.732;
                    default: throw new ArgumentException("Other stars are not implemented");
                }
            }
        }

        public StarRegular(WorkerModel worker, int vertices) : base(worker, vertices)
        {
        }

        public StarRegular(WorkerModel worker, PointF center, double angle, double radius, int vertices):
            base(worker,center,angle,radius,vertices)
        {
        }

        public override IShape Copy()
        {
            return new StarRegular(Worker, Center, Angle, Radius, Vertices);
        }

        public override IPath GetPath()
        {
            var pExt = CalculateVertices(Vertices, Radius, Angle, Center);
            var pInt = CalculateVertices(Vertices, Radius / Ratio, Angle + 360 / 2d / Vertices, Center);

            PathBuilder pb = new PathBuilder();
            for (int i = 0; i < (int)Vertices; i++)
                pb.AddLines(pExt[i], pInt[i]);
            pb.CloseFigure();
            return pb.Build();
        }

        public override string SVG(string attrs)
        {
            var pExt = CalculateVertices(Vertices, Radius, Angle, Center);
            var pInt = CalculateVertices(Vertices, Radius / Ratio, Angle + 360 / 2d / Vertices, Center);

            List<string> points = new List<string>();
            for (int i = 0; i < (int)Vertices; i++)
            {
                points.Add($"{pExt[i].X},{pExt[i].Y} {pInt[i].X},{pInt[i].Y} ");
            }
            return $"<polygon {attrs} points=\"" + String.Join(",", points) + "\" />";
        }
    }
}
