﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace PrimitiveSharp.Core
{
    public static class Extentions
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
            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            var rand_normal = mu + sigma * rand_std_normal;
            return rand_normal;
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

        public static void Resize(this Image<Rgba32> image, int canvasSize)
        {
            int width, height;
            if (canvasSize >= image.Width && canvasSize >= image.Height)
                return;
            if (image.Width > image.Height)
            {
                width = canvasSize;
                height = Convert.ToInt32(image.Height * canvasSize / (double)image.Width);
            }
            else
            {
                width = Convert.ToInt32(image.Width * canvasSize / (double)image.Height);
                height = canvasSize;
            }
            image.Mutate(im => im.Resize(width, height));
        }
    }
}
