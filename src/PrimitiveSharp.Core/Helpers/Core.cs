using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;

namespace primitive.Core
{
    public static class Core
    {
        public static Rgba32 ComputeColor(Image<Rgba32> target, Image<Rgba32> current, List<ScanlineModel> lines, int alpha)
        {
            long rsum = 0, gsum = 0, bsum = 0, count = 0;
            var a = 0x101 * 255 / alpha;

            var targetSpan = target.GetPixelSpan();
            var currentSpan = current.GetPixelSpan();

            foreach (var line in lines)
            {
                int i = line.X1 + line.Y * target.Width;
                for (int x = line.X1; x <= line.X2; x++)
                {
                    var tc = targetSpan[i];
                    var cc = currentSpan[i];
                    rsum += (tc.R - cc.R) * a + cc.R * 0x101;
                    gsum += (tc.G - cc.G) * a + cc.G * 0x101;
                    bsum += (tc.B - cc.B) * a + cc.B * 0x101;
                    count++; i++;
                }
            }

            if (count == 0) return Rgba32.Black;

            var r = ((int)(rsum / count) >> 8).Clamp(0, 255);
            var g = ((int)(gsum / count) >> 8).Clamp(0, 255);
            var b = ((int)(bsum / count) >> 8).Clamp(0, 255);

            return new Rgba32((byte)r, (byte)g, (byte)b, (byte)alpha);
        }

        public static void CopyLines(Image<Rgba32> dst, Image<Rgba32> src, List<ScanlineModel> lines)
        {
            Span<Rgba32> dstSpan = dst.GetPixelSpan();
            ReadOnlySpan<Rgba32> srcSpan = src.GetPixelSpan();
            foreach (var line in lines)
            {
                int i = line.X1 + line.Y * dst.Width;
                for (int x = line.X1; x < line.X2; x++, i++)
                    dstSpan[i] = srcSpan[i];
            }
        }

        public static void DrawLines(Image<Rgba32> im, Rgba32 c, List<ScanlineModel> lines)
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

            Span<Rgba32> imSpan = im.GetPixelSpan();

            foreach (var line in lines)
            {
                var ma = line.Alpha;
                var sra = sr * ma;
                var sga = sg * ma;
                var sba = sb * ma;
                var saa = sa * ma;
                var a = (m - sa * ma / m) * 0x101;
                int i = line.X1 + line.Y * im.Width;
                for (int x = line.X1; x <= line.X2; x++)
                {
                    var dc = imSpan[i];
                    c = new Rgba32(
                        (byte)((dc.R * a + sra) / m >> 8),
                        (byte)((dc.G * a + sga) / m >> 8),
                        (byte)((dc.B * a + sba) / m >> 8),
                        (byte)((dc.A * a + saa) / m >> 8));
                    imSpan[i] = c;
                    i++;
                }
            }
        }

        public static double DifferenceFull(Image<Rgba32> a, Image<Rgba32> b)
        {
            int w = a.Width;
            int h = a.Height;
            ulong total = 0;
            ReadOnlySpan<Rgba32> aSpan = a.GetPixelSpan();
            ReadOnlySpan<Rgba32> bSpan = b.GetPixelSpan();

            for (int i = 0; i < h * w; i++)
            {
                var ac = aSpan[i];
                var bc = bSpan[i];
                var dr = ac.R - bc.R;
                var dg = ac.G - bc.G;
                var db = ac.B - bc.B;
                var da = ac.A - bc.A;
                total += (ulong)(dr * dr + dg * dg + db * db + da * da);
            }
            return Math.Sqrt((double)total / (double)(w * h * 4)) / 255; ;
        }

        public static double DifferencePartial(Image<Rgba32> target, Image<Rgba32> before, Image<Rgba32> after, double score, List<ScanlineModel> lines)
        {
            int w = target.Width;
            int h = target.Height;
            var total = (ulong)(Math.Pow(score * 255, 2) * (double)(w * h * 4));
            ReadOnlySpan<Rgba32> targetSpan = target.GetPixelSpan();
            ReadOnlySpan<Rgba32> beforeSpan = before.GetPixelSpan();
            ReadOnlySpan<Rgba32> afterSpan = after.GetPixelSpan();
            foreach (var line in lines)
            {
                int i = line.X1 + line.Y * target.Width;
                for (int x = line.X1; x <= line.X2; x++)
                {
                    var tc = targetSpan[i];
                    var bc = beforeSpan[i];
                    var ac = afterSpan[i];
                    i++;
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

        public static Image<Rgba32> UniformImage(int width, int height, Rgba32 c)
        {
            Image<Rgba32> image = new Image<Rgba32>(width, height);
            image.Mutate(i => i.Fill(c));
            return image;
        }

        public static Rgba32 AverageImageColor(Image<Rgba32> image)
        {
            int w = image.Width;
            int h = image.Height;
            int r = 0, g = 0, b = 0;
            ReadOnlySpan<Rgba32> imageSpan = image.GetPixelSpan();
            for (int i = 0; i < h * w; i++)
            {
                b += imageSpan[i].B;
                g += imageSpan[i].G;
                r += imageSpan[i].R;
            }
            r /= w * h;
            g /= w * h;
            b /= w * h;
            Rgba32 result = new Rgba32((byte)r, (byte)g, (byte)b, (byte)255);
            return result;
        }
    }
}
