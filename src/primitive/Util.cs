using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;


namespace primitive
{
    public static class Util
    {
        public static Image<Rgba32> LoadImage(string path)
        {
            Image<Rgba32> image = Image.Load(path);
            return image;
        }

        public static void SaveFile(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public static void SavePNG(string path, Image<Rgba32> image)
        {
            image.Save(path);
        }

        public static void SaveJPG(string path, Image<Rgba32> image, int quality)
        {
            image.Save(path);
        }

        public static void SaveGIF(string path, List<Image<Rgba32>> frames, int delay, int lastDelay)
        {
            throw new NotImplementedException();
        }

        public static void SaveGIFImageMagick(string path, List<Image<Rgba32>> frames, int delay, int lastDelay)
        {
            throw new NotImplementedException();
        }

        public static string NumberString(double x)
        {
            string[] suffixes = { "", "k", "M", "G" };
            foreach (var suffix in suffixes)
            {
                if (x < 1000)
                    return String.Format("{0:0.0}" + suffix, x);
                x /= 1000;
            }
            return String.Format("{0:0.0}" + "T", x);
        }

        public static double Radians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static double Degrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double Clamp(double x, double lo, double hi)
        {
            if (x < lo)
                return lo;
            if (x > hi)
                return hi;
            return x;
        }

        public static int ClampInt(int x, int lo, int hi)
        {
            if (x < lo)
                return lo;
            if (x > hi)
                return hi;
            return x;
        }

        public static int MinInt(int a, int b)
        {
            if (a < b)
                return a;
            return b;
        }

        public static int MaxInt(int a, int b)
        {
            if (a > b)
                return a;
            return b;
        }

        public static (double rx, double ry) Rotate(double x, double y, double theta)
        {
            double rx = x * Math.Cos(theta) - y * Math.Sin(theta);
            double ry = x * Math.Sin(theta) + y * Math.Cos(theta);
            return (rx, ry);
        }

        public static Image<Rgba32> UniformRgba(int width, int height, Rgba32 c)
        {
            Image<Rgba32> image = new Image<Rgba32>(width, height);
            image.Mutate(i => i.Fill(c));
            image.Save("c:\\temp\\0uniform.png");
            return image;
        }

        public static Rgba32 AverageImageColor(Image<Rgba32> image)
        {
            int w = image.Width;
            int h = image.Height;
            int r = 0, g = 0, b = 0;

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    b += image[x, y].B;
                    g += image[x, y].G;
                    r += image[x, y].R;
                }
            r /= w * h;
            g /= w * h;
            b /= w * h;
            Rgba32 result = new Rgba32((byte)r, (byte)g, (byte)b, (byte)255);
            return result;
        }

        public static Image<Rgba32> Resize(Image<Rgba32> image)
        {
            int width, height;
            int size = Parameters.InputResize;
            if (size >= image.Width && size >= image.Height)
                return image;
            if (image.Width > image.Height)
            {
                width = size;
                height = Convert.ToInt32(image.Height * size / (double)image.Width);
            }
            else
            {
                width = Convert.ToInt32(image.Width * size / (double)image.Height);
                height = size;
            }
            image.Mutate(im => im.Resize(width, height));
            return image;
        }
    }

    public static class RandomExtensions
    {
        /// <summary>
        ///   Generates normally distributed numbers. Each operation makes two Gaussians for the price of one, and apparently they can be cached or something for better performance, but who cares.
        /// </summary>
        /// <param name="r"></param>
        /// <param name = "mu">Mean of the distribution</param>
        /// <param name = "sigma">Standard deviation</param>
        /// <returns></returns>
        public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }
    }
}
