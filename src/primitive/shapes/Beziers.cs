using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace primitive
{
    public class BezierQuadratic : Shape
    {
        public PointF P1 { get; set; }
        public PointF P2 { get; set; }
        public PointF P3 { get; set; }
        public float Width { get; set; }

        public BezierQuadratic(Worker worker)
        {
            Worker = worker;
            var rnd = Worker.Rnd;
            P1 = new PointF
            {
                X = (float)rnd.NextDouble() * worker.W,
                Y = (float)rnd.NextDouble() * worker.H
            };
            P2 = new PointF
            {
                X = P1.X + (float)rnd.NextDouble() * 40 - 20,
                Y = P1.Y + (float)rnd.NextDouble() * 40 - 20
            };
            P3 = new PointF
            {
                X = P2.X + (float)rnd.NextDouble() * 40 - 20,
                Y = P2.Y + (float)rnd.NextDouble() * 40 - 20
            };
            Width = 1.0f / 2;
        }

        public BezierQuadratic(Worker worker, PointF p1, PointF p2, PointF p3, float w)
        {
            Worker = worker;
            P1 = p1;
            P2 = p2;
            P3 = p3;
            Width = w;
        }

        public override IShape Copy()
        {
            return new BezierQuadratic(Worker, P1, P2, P3, Width);
        }

        public override IPath GetPath()
        {
            PathBuilder pb = new PathBuilder();
            pb.AddBezier(P1, P2, P3);
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
                switch (rnd.Next(3))
                {
                    case 0:
                        P1 = new PointF
                        {
                            X = (float)Util.Clamp(P1.X + rnd.NextGaussian() * 16, -m, w - 1 + m),
                            Y = (float)Util.Clamp(P1.Y + rnd.NextGaussian() * 16, -m, h - 1 + m)
                        }; break;
                    case 1:
                        P2 = new PointF
                        {
                            X = (float)Util.Clamp(P2.X + rnd.NextGaussian() * 16, -m, w - 1 + m),
                            Y = (float)Util.Clamp(P2.Y + rnd.NextGaussian() * 16, -m, h - 1 + m)
                        }; break;
                    case 2:
                        P3 = new PointF
                        {
                            X = (float)Util.Clamp(P3.X + rnd.NextGaussian() * 16, -m, w - 1 + m),
                            Y = (float)Util.Clamp(P3.Y + rnd.NextGaussian() * 16, -m, h - 1 + m)
                        }; break;
                    case 3:
                        Width = (float)Util.Clamp(Width + rnd.NextGaussian(), 1, 16); break;
                }
                if (Valid())
                    break;
            }
        }

        public override List<Scanline> Rasterize()
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
                for(int l = 0; l <n ; l++)
                {
                    var x = Util.ClampInt((int)interscertions[0].X, 0, w - 1);
                    lines.Add(new Scanline
                    {
                        Alpha = 0xffff,
                        X1 = x,
                        X2 = x,
                        Y = i
                    });
                }
            }
            return lines;
        }

        public override string SVG(string attrs)
        {
            attrs = attrs.Replace("fill", "stroke");
            return $"<path {attrs} fill=\"none\" d=\"M {P1.X} {P1.Y} Q {P2.X} {P2.Y}, {P3.X} {P3.Y}\" stroke-width=\"{Width}\" />";
        }

        private bool Valid()
        {
            var dx12 = (int)(P1.X - P2.X);
            var dy12 = (int)(P1.Y - P2.Y);
            var dx23 = (int)(P2.X - P3.X);
            var dy23 = (int)(P2.Y - P3.Y);
            var dx13 = (int)(P1.X - P3.X);
            var dy13 = (int)(P1.Y - P3.Y);
            var d12 = dx12 * dx12 + dy12 * dy12;
            var d23 = dx23 * dx23 + dy23 * dy23;
            var d13 = dx13 * dx13 + dy13 * dy13;
            return d13 > d12 && d13 > d23;
        }
    }
}
