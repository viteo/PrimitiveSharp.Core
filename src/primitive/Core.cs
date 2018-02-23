using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;

namespace primitive
{
    public static class Core
    {
        public static Rgba32 ComputeColor(Image<Rgba32> target, Image<Rgba32> current, List<Scanline> lines, int alpha)
        {
            long rsum = 0, gsum = 0, bsum = 0, count = 0;
            var a = 0x101 * 255 / alpha;

            foreach (var line in lines)
            {
                for (int x = line.X1; x <= line.X2; x++)
                {
                    var tb = (int)target[x, line.Y].B;
                    var tg = (int)target[x, line.Y].G;
                    var tr = (int)target[x, line.Y].R;
                    var cb = (int)current[x, line.Y].B;
                    var cg = (int)current[x, line.Y].G;
                    var cr = (int)current[x, line.Y].R;
                    rsum += (long)((tr - cr) * a + cr * 0x101);
                    gsum += (long)((tg - cg) * a + cg * 0x101);
                    bsum += (long)((tb - cb) * a + cb * 0x101);
                    count++;
                }
            }

            if (count == 0)
                return Rgba32.Black;

            var r = Util.ClampInt((int)(rsum / count) >> 8, 0, 255);
            var g = Util.ClampInt((int)(gsum / count) >> 8, 0, 255);
            var b = Util.ClampInt((int)(bsum / count) >> 8, 0, 255);

            return new Rgba32((byte)r, (byte)g, (byte)b, (byte)alpha);
        }

        public static void CopyLines(Image<Rgba32> dst, Image<Rgba32> src, List<Scanline> lines)
        {
            foreach (var line in lines)
            {
                for (int x = line.X1; x < line.X2; x++)
                    dst[x, line.Y] = src[x, line.Y];
            }
        }

        public static void DrawLines(Image<Rgba32> im, Rgba32 c, List<Scanline> lines)
        {
            const int m = 0xffff;

            uint sr = (uint)c.R;
            sr |= sr << 8;
            sr *= (uint)(c.A);
            sr /= 0xff;
            uint sg = (uint)(c.G);
            sg |= sg << 8;
            sg *= (uint)(c.A);
            sg /= 0xff;
            uint sb = (uint)(c.B);
            sb |= sb << 8;
            sb *= (uint)(c.A);
            sb /= 0xff;
            uint sa = (uint)(c.A);
            sa |= sa << 8;

            foreach (var line in lines)
            {
                var ma = line.Alpha;
                var a = (m - sa * ma / m) * 0x101;
                for (int x = line.X1; x <= line.X2; x++)
                {
                    var db = (uint)im[x, line.Y].B;
                    var dg = (uint)im[x, line.Y].G;
                    var dr = (uint)im[x, line.Y].R;
                    var da = (uint)im[x, line.Y].A;
                    c = new Rgba32(
                        (byte)((dr * a + sr * ma) / m >> 8),
                        (byte)((dg * a + sg * ma) / m >> 8),
                        (byte)((db * a + sb * ma) / m >> 8),
                        (byte)((da * a + sa * ma) / m >> 8));
                    im[x, line.Y] = c;
                }
            }
        }

        public static double DifferenceFull(Image<Rgba32> a, Image<Rgba32> b)
        {
            int w = a.Width;
            int h = a.Height;
            ulong total = 0;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int ab = (int)a[x, y].B;
                    int ag = (int)a[x, y].G;
                    int ar = (int)a[x, y].R;
                    int aa = (int)a[x, y].A;
                    int bb = (int)b[x, y].B;
                    int bg = (int)b[x, y].G;
                    int br = (int)b[x, y].R;
                    int ba = (int)b[x, y].A;
                    var dr = ar - br;
                    var dg = ag - bg;
                    var db = ab - bb;
                    var da = aa - ba;
                    total += (ulong)(dr * dr + dg * dg + db * db + da * da);
                }
            }
            return Math.Sqrt((double)total / (double)(w * h * 4)) / 255; ;
        }

        public static double DifferencePartial(Image<Rgba32> target, Image<Rgba32> before, Image<Rgba32> after, double score, List<Scanline> lines)
        {
            int w = target.Width;
            int h = target.Height;
            var total = (ulong)(Math.Pow(score * 255, 2) * (double)(w * h * 4));

            foreach (var line in lines)
            {
                for (int x = line.X1; x <= line.X2; x++)
                {
                    int tb = target[x, line.Y].B;
                    int tg = target[x, line.Y].G;
                    int tr = target[x, line.Y].R;
                    int ta = target[x, line.Y].A;
                    int bb = before[x, line.Y].B;
                    int bg = before[x, line.Y].G;
                    int br = before[x, line.Y].R;
                    int ba = before[x, line.Y].A;
                    int ab = after[x, line.Y].B;
                    int ag = after[x, line.Y].G;
                    int ar = after[x, line.Y].R;
                    int aa = after[x, line.Y].A;
                    var dr1 = tr - br;
                    var dg1 = tg - bg;
                    var db1 = tb - bb;
                    var da1 = ta - ba;
                    var dr2 = tr - ar;
                    var dg2 = tg - ag;
                    var db2 = tb - ab;
                    var da2 = ta - aa;
                    total -= (ulong)(dr1 * dr1 + dg1 * dg1 + db1 * db1 + da1 * da1);
                    total += (ulong)(dr2 * dr2 + dg2 * dg2 + db2 * db2 + da2 * da2);
                }
            }
            return Math.Sqrt((double)total / (double)(w * h * 4)) / 255;
        }
    }
}
