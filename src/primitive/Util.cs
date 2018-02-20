using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace primitive
{
    public static class Util
    {
        public static Image LoadImage(string path)
        {
            Image image = Image.FromFile(path);
            return image;
        }

        public static void SaveFile(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public static void SavePNG(string path, Bitmap image)
        {
            image.Save(path, ImageFormat.Png);
        }

        public static void SaveJPG(string path, Bitmap image, int quality)
        {
            var jpegEncoder = getEncoder(ImageFormat.Jpeg);
            var jpegQualityEncoder = Encoder.Quality;
            var encoderParameters = new EncoderParameters(1);
            var encoderParameter = new EncoderParameter(jpegQualityEncoder, (byte)quality);
            encoderParameters.Param[0] = encoderParameter;

            image.Save(path, jpegEncoder, encoderParameters);
        }

        public static void SaveGIF(string path, Bitmap[] frames, int delay, int lastDelay)
        {
            throw new NotImplementedException();
        }

        public static void SaveGIFImageMagick(string path, Bitmap[] frames, int delay, int lastDelay)
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

        public static Bitmap ImageToRgba(Image src)
        {
            Bitmap clone = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppRgb);
            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(src, new Rectangle(0, 0, clone.Width, clone.Height));
            }
            return clone;
        }

        public static Bitmap CopyRgba(Bitmap src)
        {
            Bitmap clone = (Bitmap)src.Clone();
            return clone;
        }

        public static Bitmap UniformRgba(int width, int height, Color c)
        {
            Bitmap image = new Bitmap(width, height);
            using (Graphics gr = Graphics.FromImage(image))
            {
                gr.Clear(c);
            }
            return image;
        }

        public static Color AverageImageColor(Image image)
        {
            var rgba = ImageToRgba(image);
            Size size = rgba.Size;
            int w = size.Width;
            int h = size.Height;
            int r = 0, g = 0, b = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = rgba.GetPixel(x, y); //lockbits needed
                    r += c.R;
                    g += c.G;
                    b += c.B;
                }
            }
            r /= w * h;
            g /= w * h;
            b /= w * h;
            Color result = Color.FromArgb(255, r, g, b);
            return result;
        }

        public static Image Resize(Image image)
        {
            int width, height;
            int size = Parameters.InputResize;
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

            Image resized = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.Bilinear;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImage(image, 0, 0, width, height);
            }

            return resized;
        }


        private static ImageCodecInfo getEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
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
