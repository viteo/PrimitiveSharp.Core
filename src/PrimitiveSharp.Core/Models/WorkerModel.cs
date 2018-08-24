using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace PrimitiveSharp.Core
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
        private double Score { get; set; }

        public WorkerModel(Image<Rgba32> target)
        {
            var w = target.Width;
            var h = target.Height;
            W = w;
            H = h;
            Target = target;
            Buffer = new Image<Rgba32>(target.Width, target.Height);
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
            var lines = shape.GetScanlines();
            Rgba32 color = Core.ComputeColor(Target, Current, lines, alpha);
            Core.CopyLines(Buffer, Current, lines);
            Core.DrawLines(Buffer, color, lines);
            return Core.DifferencePartial(Target, Current, Buffer, Score, lines);
        }

        public StateModel BestState(ShapeType shapeType, int alpha, int shapeProbeCount, int shapeAge, int m)
        {
            double bestScore = 0;
            StateModel bestState = new StateModel();
            for (int i = 0; i < m; i++)
            {
                StateModel state = BestRandomState(shapeType, alpha, shapeProbeCount);
                //StateModel state = RandomState(t, a);
                state.HillClimb(shapeAge);
                var score = state.Score();
                if (i == 0 || score < bestScore)
                {
                    bestScore = score;
                    bestState = state;
                }
            }
            return bestState;
        }

        public StateModel BestRandomState(ShapeType shapeType, int apha, int shapeProbeCount)
        {
            double bestScore = 0;
            StateModel bestState = new StateModel();
            for (int i = 0; i < shapeProbeCount; i++)
            {
                var state = RandomState(shapeType, apha);
                var score = state.Score();
                if (i == 0 || score < bestScore)
                {
                    bestScore = score;
                    bestState = state;
                }
            }
            return bestState;
        }

        public StateModel RandomState(ShapeType shapeType, int alpha)
        {
            switch (shapeType)
            {
                default:
                    return RandomState((ShapeType)(Rnd.Next(8) + 1), alpha);
                case ShapeType.Triangle:
                    return new StateModel(this, new Polygon(this, 3, false), alpha);
                case ShapeType.Rectangle:
                    return new StateModel(this, new RectangleStraight(this), alpha);
                case ShapeType.Ellipse:
                    return new StateModel(this, new EllipseStrait(this, false), alpha);
                case ShapeType.Circle:
                    return new StateModel(this, new EllipseStrait(this, true), alpha);
                case ShapeType.RotatedRectangle:
                    return new StateModel(this, new RectangleRotated(this), alpha);
                case ShapeType.BezierQuadratic:
                    return new StateModel(this, new BezierQuadratic(this), alpha);
                case ShapeType.RotatedEllipse:
                    return new StateModel(this, new EllipseRotated(this), alpha);
                case ShapeType.Quadrilateral:
                    return new StateModel(this, new Polygon(this, 4, false), alpha);
                case ShapeType.Square:
                    return new StateModel(this, new PolygonRegular(this, 4), alpha);
                case ShapeType.Pentagon:
                    return new StateModel(this, new PolygonRegular(this, 5), alpha);
                case ShapeType.Hexagon:
                    return new StateModel(this, new PolygonRegular(this, 6), alpha);
                case ShapeType.Octagon:
                    return new StateModel(this, new PolygonRegular(this, 7), alpha);
                case ShapeType.FourPointedStar:
                    return new StateModel(this, new StarRegular(this, 4), alpha);
                case ShapeType.Pentagram:
                    return new StateModel(this, new StarRegular(this, 5), alpha);
                case ShapeType.Hexagram:
                    return new StateModel(this, new StarRegular(this, 6), alpha);
            }
        }

        public void Dispose()
        {
            Current?.Dispose();
            Buffer?.Dispose();
        }
    }
}
