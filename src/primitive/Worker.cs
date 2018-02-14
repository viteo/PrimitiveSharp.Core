using System;
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
            Core.DrawLines(Buffer,color, lines);
            return Core.DifferencePartial(Target, Current, Buffer, Score, lines);
        }


    }
}
