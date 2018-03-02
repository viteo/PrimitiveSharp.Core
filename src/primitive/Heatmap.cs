using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace primitive
{
    public class Heatmap
    {
        private int H, W;
        private UInt64[] count;

        //NewHeatmap
        public Heatmap(int h, int w)
        {
            H = h;
            W = w;
            count = new ulong[h * w];
        }

        public void Clear()
        {
            for (int i = 0; i < count.Length; i++)
                count[i] = 0;
        }

        public void Add(List<Scanline> lines)
        {
            foreach (var line in lines)
            {
                int i = line.Y * W + line.X1;
                for (var x = line.X1; x <= line.X2; x++)
                {
                    count[i] += (uint)line.Alpha;
                    i++;
                }
            }
        }

        public void AddHeatmap(Heatmap a)
        {
            for (int i = 0; i < a.count.Length; i++)
                count[i] += a.count[i];
        }

        public Image<Short2> Image(double gamma)
        {
            Image<Short2> im = new Image<Short2>(W, H);
            ulong hi = 0;
            foreach (var h in count)
            {
                if (h > hi)
                    hi = h;
            }

            int i = 0;
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    double p = (double)count[i] / (double)hi;
                    p = Math.Pow(p, gamma);
                    im[x, y] = new Short2((short)(p * 0xffff), (short)(p * 0xffff));
                }
            }
            return im;
        }
    }
}
