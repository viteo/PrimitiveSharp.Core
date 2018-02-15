using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace primitive
{
    public class Model
    {
        public int Sw { get; set; }
        public int Sh { get; set; }
        public double Scale { get; set; }
        public Color Background { get; set; }
        public Bitmap Target { get; set; }
        public Bitmap Current { get; set; }
        public Graphics Context { get; set; }
        public double Score { get; set; }
        public List<IShape> Shapes { get; set; }
        public List<Color> Colors { get; set; }
        public List<double> Scores { get; set; }
        public List<Worker> Workers { get; set; }

        public Model(Image target, Color background, int size, int numWorkers)
        {
            var w = target.Width;
            var h = target.Height;
            double aspect = (double) w / (double) h;
            int sw = 0, sh = 0;
            double scale = 0;
            if (aspect >= 1)
            {
                sw = size;
                sh = (int) ((double)size / aspect);
                scale = (double) size / (double) w;
            }
            else
            {
                sw = (int) ((double) size * aspect);
                sh = size;
                scale = (double) size / (double) h;
            }

            Sw = sw;
            Sh = sh;
            Scale = scale;
            Background = background;
            Target = Util.ImageToRgba(target);
            Current = Util.UniformRgba(target.)

        }

    }
}
