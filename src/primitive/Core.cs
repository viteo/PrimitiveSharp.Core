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
                for (int x = line.X1; x <= line.X2; x++)
                {
                    var tc = target[x, line.Y];
                    var cc = current[x, line.Y];
                    rsum += (tc.R - cc.R) * a + cc.R * 0x101;
                    gsum += (tc.G - cc.G) * a + cc.G * 0x101;
                    bsum += (tc.B - cc.B) * a + cc.B * 0x101;
                    count++;
                }

            if (count == 0) return Rgba32.Black;

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
                    var dc = im[x, line.Y];
                    c = new Rgba32(
                        (byte)((dc.R * a + sr * ma) / m >> 8),
                        (byte)((dc.G * a + sg * ma) / m >> 8),
                        (byte)((dc.B * a + sb * ma) / m >> 8),
                        (byte)((dc.A * a + sa * ma) / m >> 8));
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
                    var ac = a[x, y];
                    var bc = b[x, y];
                    var dr = ac.R - bc.R;
                    var dg = ac.G - bc.G;
                    var db = ac.B - bc.B;
                    var da = ac.A - bc.A;
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
                    var tc = target[x, line.Y];
                    var bc = before[x, line.Y];
                    var ac = after[x, line.Y];
                    var dr1 = tc.R - bc.R;
                    var dg1 = tc.G - bc.G;
                    var db1 = tc.B - bc.B;
                    var da1 = tc.A - bc.A;
                    var dr2 = tc.R - ac.R;
                    var dg2 = tc.G - ac.G;
                    var db2 = tc.B - ac.B;
                    var da2 = tc.A - ac.A;
                    total -= (ulong)(dr1 * dr1 + dg1 * dg1 + db1 * db1 + da1 * da1);
                    total += (ulong)(dr2 * dr2 + dg2 * dg2 + db2 * db2 + da2 * da2);
                }
            }
            return Math.Sqrt((double)total / (double)(w * h * 4)) / 255;
        }
    }
}
