﻿using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;

namespace primitive.Core
{
    public static class Util
    {
        public static Image<Rgba32> LoadImage(string path)
        {
            Image<Rgba32> image = Image.Load(path);
            return image;
        }

        public static void SaveSVG(string path, List<string> contents)
        {
            for (int i = 0; i < contents.Count; i++)
            {
                string framePath = String.Format(path, i + 1);
                File.WriteAllText(framePath, contents[i]);
            }
        }

        public static void SaveImage(string path, Image<Rgba32> image)
        {
            image.Save(path);
        }

        public static void SaveFrames(string path, Image<Rgba32> images)
        {
            for (int i = images.Frames.Count - 1; i >= 0; i--)
            {
                string framePath = String.Format(path, i);
                images.Frames.CloneFrame(i).Save(framePath);
            }
        }

        public static void SaveGIF(string path, Image<Rgba32> frames, int delay, int lastDelay)
        {
            frames.Save(path);
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


        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
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
            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            var rand_normal = mu + sigma * rand_std_normal;
            return rand_normal;
        }
    }
}
