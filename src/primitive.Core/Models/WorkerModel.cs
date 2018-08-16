using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace primitive.Core
{
    public class WorkerModel : IDisposable
    {
        public int W { get; set; }
        public int H { get; set; }
        public Random Rnd { get; set; }
        public int Counter { get; set; }

        private Image<Rgba32> Target { get; set; }
        private Image<Rgba32> Current { get; set; }
        private Image<Rgba32> Buffer { get; set; }
        private List<ScanlineModel> Lines { get; set; }
        private double Score { get; set; }

        public WorkerModel(Image<Rgba32> target)
        {
            var w = target.Width;
            var h = target.Height;
            W = w;
            H = h;
            Target = target;
            Buffer = new Image<Rgba32>(target.Width, target.Height);
            Lines = new List<ScanlineModel>();
            //Rnd = new Random(1);
            Rnd = new Random((int)DateTime.Now.Ticks);
        }

        public void Init(Image<Rgba32> current, double score)
        {
            Current = current;
            Score = score;
            Counter = 0;
        }

        public double GetScore(IShape shape, int alpha)
        {
            Counter++;
            var lines = shape.Rasterize();
            Rgba32 color = Core.ComputeColor(Target, Current, lines, alpha);
            Core.CopyLines(Buffer, Current, lines);
            Core.DrawLines(Buffer, color, lines);
            return Core.DifferencePartial(Target, Current, Buffer, Score, lines);
        }

        public StateModel BestState(ShapeType t, int a, int n, int age, int m)
        {
            double bestScore = 0;
            StateModel bestState = new StateModel();
            for (int i = 0; i < m; i++)
            {
                StateModel state = BestRandomState(t, a, n);
                //StateModel state = RandomState(t, a);
                state.HillClimb(age);
                var score = state.Score;
                if (i == 0 || score < bestScore)
                {
                    bestScore = score;
                    bestState = state;
                }
            }
            return bestState;
        }

        public StateModel BestRandomState(ShapeType t, int a, int n)
        {
            double bestScore = 0;
            StateModel bestState = new StateModel();
            for (int i = 0; i < n; i++)
            {
                var state = RandomState(t, a);
                var score = state.Score;
                if (i == 0 || score < bestScore)
                {
                    bestScore = score;
                    bestState = state;
                }
            }
            return bestState;
        }

        public StateModel RandomState(ShapeType t, int a)
        {
            switch (t)
            {
                default:
                    return RandomState((ShapeType)(Rnd.Next(8) + 1), a);
                case ShapeType.Triangle:
                    return new StateModel(this, new Polygon(this, 3, false), a);
                case ShapeType.Rectangle:
                    return new StateModel(this, new RectangleStraight(this), a);
                case ShapeType.Ellipse:
                    return new StateModel(this, new EllipseStrait(this, false), a);
                case ShapeType.Circle:
                    return new StateModel(this, new EllipseStrait(this, true), a);
                case ShapeType.RotatedRectangle:
                    return new StateModel(this, new RectangleRotated(this), a);
                case ShapeType.BezierQuadratic:
                    return new StateModel(this, new BezierQuadratic(this), a);
                case ShapeType.RotatedEllipse:
                    return new StateModel(this, new EllipseRotated(this), a);
                case ShapeType.Quadrilateral:
                    return new StateModel(this, new Polygon(this, 4, false), a);
                case ShapeType.Square:
                    return new StateModel(this, new PolygonRegular(this, 4), a);
                case ShapeType.Pentagon:
                    return new StateModel(this, new PolygonRegular(this, 5), a);
                case ShapeType.Hexagon:
                    return new StateModel(this, new PolygonRegular(this, 6), a);
                case ShapeType.Octagon:
                    return new StateModel(this, new PolygonRegular(this, 7), a);
                case ShapeType.FourPointedStar:
                    return new StateModel(this, new StarRegular(this, 4), a);
                case ShapeType.Pentagram:
                    return new StateModel(this, new StarRegular(this, 5), a);
                case ShapeType.Hexagram:
                    return new StateModel(this, new StarRegular(this, 6), a);
            }
        }

        public void Dispose()
        {
            Current?.Dispose();
            Buffer?.Dispose();
        }
    }
}
