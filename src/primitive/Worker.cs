﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using primitive.primitive;

namespace primitive
{
    public class Worker
    {
        public int W { get; set; }
        public int H { get; set; }
        public Bitmap Target { get; set; }
        public Bitmap Current { get; set; }
        public Bitmap Buffer { get; set; }
        //todo Rasterizer
        public List<Scanline> Lines { get; set; }
        public Heatmap Heatmap { get; set; }
        public Random Rnd { get; set; }
        public double Score { get; set; }
        public int Counter { get; set; }

        public Worker(Bitmap target)
        {
            var w = target.Width;
            var h = target.Height;
            W = w;
            H = h;
            Target = target;
            Buffer = new Bitmap(target.Width, target.Height);
            //todo Raserizer
            Lines = new List<Scanline>();
            Heatmap = new Heatmap(w, h);
            Rnd = new Random((int)DateTime.Now.Ticks);
        }

        public void Init(Bitmap current, double score)
        {
            Current = current;
            Score = score;
            Counter = 0;
            Heatmap.Clear();
        }

        public double Energy(IShape shape, int alpha)
        {
            Counter++;
            var lines = shape.Rasterize();
            //Heatmap.Add(lines);
            var color = Core.ComputeColor(Target, Current, lines, alpha);
            Core.CopyLines(Buffer, Current, lines);
            Core.DrawLines(Buffer, color, lines);
            return Core.DifferencePartial(Target, Current, Buffer, Score, lines);
        }

        public State BestHillClimbState(ShapeType t, int a, int n, int age, int m)
        {
            double bestEnergy = 0;
            State bestState = new State();
            for (int i = 0; i < m; i++)
            {
                var state = BestRandomState(t, a, n);
                var before = state.Energy();
                state = Optimize.HillClimb(state, age) as State;
                var energy = state.Energy();
                Logger.WriteLine(2, "{0}x random: {1:G6} -> {2}x hill climb: {3:G6} ", n, before, age, energy);
                if (i == 0 || energy < bestEnergy)
                {
                    bestEnergy = energy;
                    bestState = state;
                }
            }
            return bestState;
        }

        public State BestRandomState(ShapeType t, int a, int n)
        {
            double bestEnergy = 0;
            State bestState = new State();
            for (int i = 0; i < n; i++)
            {
                var state = RandomState(t, a);
                var energy = state.Energy();
                if (i == 0 || energy < bestEnergy)
                {
                    bestEnergy = energy;
                    bestState = state;
                }
            }
            return bestState;
        }

        public State RandomState(ShapeType t, int a)
        {
            switch (t)
            {
                default:
                    return RandomState((ShapeType)(Rnd.Next(8) + 1), a);
                //case ShapeType.ShapeTypeTriangle:
                //    return new State(this, NewRandomTriangle(this), a);
                case ShapeType.ShapeTypeRectangle:
                    return new State(this, new RectangleStrait(this), a);
                //case ShapeType.ShapeTypeEllipse:
                //    return new State(this, NewRandomEllipse(this), a);
                //case ShapeType.ShapeTypeCircle:
                //    return new State(this, NewRandomCircle(this), a);
                //case ShapeType.ShapeTypeRotatedRectangle:
                //    return new State(this, new RectangleRotated(this), a);
                //case ShapeType.ShapeTypeQuadratic:
                //    return new State(this, NewRandomQuadratic(this), a);
                //case ShapeType.ShapeTypeRotatedEllipse:
                //    return new State(this, NewRandomRotatedEllipse(this), a);
                //case ShapeType.ShapeTypePolygon:
                //    return new State(this, NewRandomPolygon(this), a);
            }
        }



    }
}