using System;
using System.Collections.Generic;
using System.Text;

namespace primitive
{
    public class Raster
    {
        public List<Scanline> Lines { get; set; }

        public void Paint(List<Scanline> spans, bool done)
        {
            foreach(var span in spans)
            {
                Lines.Add(new Scanline() { Y = span.Y, X1 = span.X1, X2 = span.X2 - 1, Alpha = span.Alpha });
            }
        }

        //public List<Scanline> FillPath(Worker worker, GraphicsPath path)
        //{
        //    var r = worker.Rasterizer;
        //}
    }
}
