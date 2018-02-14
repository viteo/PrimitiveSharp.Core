using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace primitive
{
    public static class Core
    {
        public static Color ComputeColor(Bitmap target, Bitmap current, List<Scanline> lines, int alpha)
        {
            long rsum = 0, gsum = 0, bsum = 0, count = 0;
            var a = 0x101 * 255 / alpha;

            var dataTarget = target.LockBits(
                new Rectangle(0, 0, target.Width, target.Height),
                ImageLockMode.ReadOnly, target.PixelFormat);
            var dataCurrent = current.LockBits(
                new Rectangle(0, 0, current.Width, current.Height),
                ImageLockMode.ReadOnly, current.PixelFormat);
            int numBytesTarget = dataTarget.Stride * target.Height;
            int numBytesCurrent = dataCurrent.Stride * current.Height;
            byte[] rgbaTarget = new byte[numBytesTarget];
            byte[] rgbaCurrent = new byte[numBytesCurrent];
            Marshal.Copy(dataTarget.Scan0, rgbaTarget, 0, numBytesTarget);
            Marshal.Copy(dataCurrent.Scan0, rgbaCurrent, 0, numBytesCurrent);
            int stride = dataTarget.Stride;
            target.UnlockBits(dataTarget);
            current.UnlockBits(dataCurrent);

            foreach (var line in lines)
            {
                int i = pixOffset(line.X1, line.Y, stride);
                for (int x = line.X1; x <= line.X2; x++)
                {
                    var tr = (int)rgbaTarget[i];
                    var tg = (int)rgbaTarget[i + 1];
                    var tb = (int)rgbaTarget[i + 2];
                    var cr = (int)rgbaCurrent[i];
                    var cg = (int)rgbaCurrent[i + 1];
                    var cb = (int)rgbaCurrent[i + 2];
                    i += 4;
                    rsum += (long)((tr - cr) * a + cr * 0x101);
                    gsum += (long)((tg - cg) * a + cg * 0x101);
                    bsum += (long)((tb - cb) * a + cb * 0x101);
                    count++;
                }
            }

            if (count == 0)
                return Color.Black;

            var r = Util.ClampInt((int)(rsum / count) >> 8, 0, 255);
            var g = Util.ClampInt((int)(gsum / count) >> 8, 0, 255);
            var b = Util.ClampInt((int)(bsum / count) >> 8, 0, 255);

            return Color.FromArgb(r, g, b);
        }

        public static void CopyLines(Bitmap dst, Bitmap src, List<Scanline> lines)
        {
            var dataDst = dst.LockBits(
                new Rectangle(0, 0, dst.Width, dst.Height),
                ImageLockMode.ReadWrite, dst.PixelFormat);
            var dataSrc = src.LockBits(
                new Rectangle(0, 0, src.Width, src.Height),
                ImageLockMode.ReadWrite, src.PixelFormat);
            int numBytesDst = dataDst.Stride * dst.Height;
            int numBytesSrc = dataSrc.Stride * src.Height;
            byte[] rgbaDst = new byte[numBytesDst];
            byte[] rgbaSrc = new byte[numBytesSrc];
            Marshal.Copy(dataDst.Scan0, rgbaDst, 0, numBytesDst);
            Marshal.Copy(dataSrc.Scan0, rgbaSrc, 0, numBytesSrc);

            foreach (var line in lines)
            {
                int a = pixOffset(line.X1, line.Y, dataDst.Stride);
                int b = a + (line.X2 - line.X1 + 1) * 4;
                Array.Copy(rgbaSrc, a, rgbaDst, a, b - a);
            }

            Marshal.Copy(rgbaDst, 0, dataDst.Scan0, numBytesDst);
            Marshal.Copy(rgbaSrc, 0, dataSrc.Scan0, numBytesSrc);
            dst.UnlockBits(dataDst);
            src.UnlockBits(dataSrc);
        }

        public static void DrawLines(Bitmap im, Color c, List<Scanline> lines)
        {
            const int m = 0xffff;
            uint sr = c.R;
            uint sg = c.G;
            uint sb = c.B;
            uint sa = c.A;

            var dataIm = im.LockBits(
                new Rectangle(0, 0, im.Width, im.Height),
                ImageLockMode.ReadWrite, im.PixelFormat);
            int numBytesIm = dataIm.Stride * im.Height;
            byte[] rgbaIm = new byte[numBytesIm];
            Marshal.Copy(dataIm.Scan0, rgbaIm, 0, numBytesIm);


            foreach (var line in lines)
            {
                var ma = line.Alpha;
                var a = (m - sa * ma / m) * 0x101;
                var i = pixOffset(line.X1, line.Y, dataIm.Stride);
                for (int x = line.X1; x <= line.X2; x++)
                {
                    var dr = (uint)(rgbaIm[i + 0]);
                    var dg = (uint)(rgbaIm[i + 1]);
                    var db = (uint)(rgbaIm[i + 2]);
                    var da = (uint)(rgbaIm[i + 3]);
                    rgbaIm[i + 0] = (byte)((dr * a + sr * ma) / m >> 8);
                    rgbaIm[i + 1] = (byte)((dg * a + sg * ma) / m >> 8);
                    rgbaIm[i + 2] = (byte)((db * a + sb * ma) / m >> 8);
                    rgbaIm[i + 3] = (byte)((da * a + sa * ma) / m >> 8);
                    i += 4;
                }
            }
            Marshal.Copy(rgbaIm, 0, dataIm.Scan0, numBytesIm);
            im.UnlockBits(dataIm);
        }

        public static double DifferenceFull(Bitmap a, Bitmap b)
        {
            int w = a.Width;
            int h = a.Height;
            ulong total = 0;

            var dataA = a.LockBits(
                new Rectangle(0, 0, a.Width, a.Height),
                ImageLockMode.ReadOnly, a.PixelFormat);
            int numBytesA = dataA.Stride * a.Height;
            byte[] rgbaA = new byte[numBytesA];
            Marshal.Copy(dataA.Scan0, rgbaA, 0, numBytesA);

            var dataB = b.LockBits(
                new Rectangle(0, 0, b.Width, b.Height),
                ImageLockMode.ReadOnly, b.PixelFormat);
            int numBytesB = dataB.Stride * b.Height;
            byte[] rgbaB = new byte[numBytesB];
            Marshal.Copy(dataB.Scan0, rgbaB, 0, numBytesB);

            var stride = dataA.Stride;
            a.UnlockBits(dataA);
            b.UnlockBits(dataB);

            for (int y = 0; y < h; y++)
            {
                var i = pixOffset(0, y, stride);
                for (int x = 0; i < w; x++)
                {
                    int ar = (int)rgbaA[i];
                    int ag = (int)rgbaA[i + 1];
                    int ab = (int)rgbaA[i + 2];
                    int aa = (int)rgbaA[i + 3];
                    int br = (int)rgbaA[i];
                    int bg = (int)rgbaA[i + 1];
                    int bb = (int)rgbaA[i + 2];
                    int ba = (int)rgbaA[i + 3];
                    i += 4;
                    var dr = ar - br;
                    var dg = ag - bg;
                    var db = ab - bb;
                    var da = aa - ba;
                    total += (ulong)(dr * dr + dg * dg + db * db + da * da);
                }
            }

            return Math.Sqrt((double)total / (double)(w * h * 4)) / 255;
        }

        public static double DifferencePartial(Bitmap target, Bitmap before, Bitmap after, double score, List<Scanline> lines)
        {
            int w = target.Width;
            int h = target.Height;
            ulong total = (ulong)(Math.Pow(score * 255, 2) * (double)(w * h * 4));

            var dataTarget = target.LockBits(
                new Rectangle(0, 0, target.Width, target.Height),
                ImageLockMode.ReadOnly, target.PixelFormat);
            var dataBefore = before.LockBits(
                new Rectangle(0, 0, before.Width, before.Height),
                ImageLockMode.ReadOnly, before.PixelFormat);
            var dataAfter = after.LockBits(
                new Rectangle(0, 0, after.Width, after.Height),
                ImageLockMode.ReadOnly, after.PixelFormat);
            int numBytesTarget = dataTarget.Stride * target.Height;
            int numBytesBefore = dataBefore.Stride * before.Height;
            int numBytesAfter = dataAfter.Stride * after.Height;
            byte[] rgbaTarget = new byte[numBytesTarget];
            byte[] rgbaBefore = new byte[numBytesBefore];
            byte[] rgbaAfter = new byte[numBytesAfter];
            Marshal.Copy(dataTarget.Scan0, rgbaTarget, 0, numBytesTarget);
            Marshal.Copy(dataBefore.Scan0, rgbaBefore, 0, numBytesBefore);
            Marshal.Copy(dataAfter.Scan0, rgbaAfter, 0, numBytesAfter);
            int stride = dataTarget.Stride;
            target.UnlockBits(dataTarget);
            before.UnlockBits(dataBefore);
            after.UnlockBits(dataAfter);

            foreach (var line in lines)
            {
                int i = pixOffset(line.X1, line.Y, stride);
                for (int x = line.X1; x <= line.X2; x++)
                {
                    int tr = rgbaTarget[i];
                    int tg = rgbaTarget[i + 1];
                    int tb = rgbaTarget[i + 2];
                    int ta = rgbaTarget[i + 3];
                    int br = rgbaBefore[i];
                    int bg = rgbaBefore[i + 1];
                    int bb = rgbaBefore[i + 2];
                    int ba = rgbaBefore[i + 3];
                    int ar = rgbaAfter[i];
                    int ag = rgbaAfter[i + 1];
                    int ab = rgbaAfter[i + 2];
                    int aa = rgbaAfter[i + 3];
                    i += 4;
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

        private static int pixOffset(int x, int y, int stride)
        {
            return y * stride + x * 4;
        }

    }

}
