using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

using System.Collections.Generic;

namespace primitive.Core
{
    public static class File
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
                System.IO.File.WriteAllText(framePath, contents[i]);
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
    }
}
