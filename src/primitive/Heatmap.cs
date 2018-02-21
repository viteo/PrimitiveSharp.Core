using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

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

        public Bitmap Image(double gamma)
        {
            Bitmap im = new Bitmap(W, H, PixelFormat.Format16bppGrayScale);
            var data = im.LockBits(
                new Rectangle(0, 0, im.Width, im.Height),
                ImageLockMode.ReadWrite,
                im.PixelFormat);
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
                    double p = (double) count[i] / (double) hi;
                    p = Math.Pow(p, gamma);
                    Marshal.WriteInt16(data.Scan0, i * 2, (short)(p * 0xffff));
                    i++;
                }
            }

            im.UnlockBits(data);
            return im;
        }
    }
}
