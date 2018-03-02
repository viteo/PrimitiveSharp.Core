using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using SixLabors.ImageSharp;

namespace primitive
{
    [HelpOption]
    class PrimitiveCS
    {
        static void Main(string[] args) => CommandLineApplication.Execute<PrimitiveCS>(args);

        public static Random Rand;

        #region InputParameters
        [Required]
        [Option(Description = "Required. Input file.",
            Template = "-i|--input")]
        public string Input { get; }

        [Required]
        [Option(Description = "Required. Output file.",
            Template = "-o|--output")]
        public string OutputFiles { get; }

        [Required]
        [Option(Description = "Required. Number of shapes.",
            Template = "-n|--number")]
        public string Nprimitives { get; }

        [Option(Description =
            "Mode: 0=combo, 1=triangle, 2=rect, 3=ellipse, 4=circle, 5=rotatedrect, 6=beziers, 7=rotatedellipse, 8=polygon",
            Template = "-m|--mode")]
        public int? Mode { get; }

        [Option(Description = "Color alpha (use 0 to let the algorithm choose alpha for each shape)",
            Template = "-a|--alpha")]
        public int? Alpha { get; }

        [Option(Description = "Add N extra shapes each iteration with reduced search (mostly good for beziers)",
            Template = "-rep|--repeat")]
        public int? Repeat { get; }

        [Option(Description = "Save every Nth frame (only when %d is in output path)",
            Template = "-nth")]
        public int? NthFrame { get; }

        [Option(Description = "Resize large input images to this size before processing",
            Template = "-r|--resize")]
        public int? InputResize { get; }

        [Option(Description = "Output image size",
            Template = "-s|--size")]
        public int? OutputSize { get; }

        [Option(Description = "Starting background color (hex)",
            Template = "-bg|--background")]
        public string Background { get; }

        [Option(Description = "Number of parallel workers (default uses all cores)",
            Template = "-j")]
        public int? Workers { get; }

        [Option(Description = "Verbose output",
            Template = "-v")]
        public bool Verbose { get; }

        [Option(Description = "Very verbose output",
            Template = "-vv")]
        public bool VeryVerbose { get; }
        #endregion

        private void OnExecute()
        {
            // parse and validate arguments
            Parameters.InputFile = Input;
            foreach (var output in OutputFiles.Split(' '))
                Parameters.OutputFiles.FileList.Add(output);
            Parameters.Mode = Mode ?? 1;
            Parameters.Alpha = Alpha ?? 128;
            Parameters.Repeat = Repeat ?? 0;
            foreach (var nprim in Nprimitives.Split(' '))
                Parameters.ShapeConfigs.Set(nprim);
            Parameters.Nth = NthFrame ?? 1;
            Parameters.InputResize = InputResize ?? 256;
            Parameters.OutputSize = OutputSize ?? 1024;
            Parameters.Background = Background ?? "";
            Parameters.Workers = Workers ?? 0;

            // set log level
            if (Verbose)
                Parameters.LogLevel = 1;
            if (VeryVerbose)
                Parameters.LogLevel = 2;

            // seed random number generator
            Rand = new Random((int)DateTime.Now.Ticks);

            // determine worker count
            if (Parameters.Workers < 1)
                Parameters.Workers = Environment.ProcessorCount;

            // read input image
            Logger.WriteLine(1, "reading {0}", Parameters.InputFile);
            Image<Rgba32> inputImage;
            inputImage = Util.LoadImage(Parameters.InputFile);


            // scale down input image if needed
            if (Parameters.InputResize > 0)
                inputImage = Util.Resize(inputImage);

            // determine background color
            Rgba32 bgColor;
            if (Parameters.Background == "")
                bgColor = Util.AverageImageColor(inputImage);
            else
                bgColor = Rgba32.FromHex(Parameters.Background);


            // run algorithm
            Model model = new Model(inputImage, bgColor, Parameters.OutputSize, Parameters.Workers);
            Logger.WriteLine(1, "{0}: t={1:G3}, score={2:G6}", 0, 0.0, model.Score);
            var start = DateTime.Now;
            int frame = 0, j = 0;

            foreach (var shapeConfig in Parameters.ShapeConfigs.ConfigList)
            {
                Logger.WriteLine(1, "count={0}, mode={1}, alpha={2}, repeat={3}", shapeConfig.Count, shapeConfig.Mode, shapeConfig.Alpha, shapeConfig.Repeat);
                for (int i = 0; i < shapeConfig.Count; i++)
                {
                    frame++;

                    // find optimal shape and add it to the model
                    var t = DateTime.Now;
                    var n = model.Step((ShapeType)shapeConfig.Mode, shapeConfig.Alpha, shapeConfig.Repeat);
                    var nps = Util.NumberString((double)n / (DateTime.Now - t).TotalSeconds);
                    var elapsed = (DateTime.Now - start).TotalSeconds;
                    Logger.WriteLine(1, "{0:00}: t={1:G3}, score={2:G6}, n={3}, n/s={4}", frame, elapsed, model.Score, n, nps);

                    // write output image(s)
                    foreach (var outFile in Parameters.OutputFiles.FileList)
                    {
                        var ext = Path.GetExtension(outFile).ToLower();
                        bool percent = outFile.Contains("{");
                        bool saveFrames = percent && !ext.Equals(".gif");
                        saveFrames = saveFrames && frame % Parameters.Nth == 0;
                        var last = j == Parameters.ShapeConfigs.ConfigList.Count - 1 && i == shapeConfig.Count - 1;
                        if (saveFrames || last)
                        {
                            var path = outFile;
                            if (percent)
                                path = String.Format(outFile, frame);
                            Logger.WriteLine(1, "writing {0}", path);
                            switch (ext)
                            {
                                case ".png":
                                    Util.SavePNG(path, model.Result); break;
                                case ".jpg": case ".jpeg":
                                    Util.SaveJPG(path, model.Result, 95); break;
                                case ".svg":
                                    Util.SaveFile(path, model.SVG()); break;
                                case ".gif":
                                    var frames = model.Frames(0.001);
                                    Util.SaveGIF(path, frames, 50, 250); break;
                                default:
                                    throw new Exception("unrecognized file extension: " + ext);
                            }
                        }
                    }
                }
            }
            Console.WriteLine("End");
        }
    }

    public static class Parameters
    {
        public static string InputFile;
        public static OutputFileArray OutputFiles = new OutputFileArray();
        public static ShapeConfigArray ShapeConfigs = new ShapeConfigArray();
        public static int Mode;
        public static int Alpha;
        public static int Repeat;
        public static string Background;
        public static int InputResize;
        public static int OutputSize;
        public static int Workers;
        public static int Nth;
        public static int LogLevel;
    }

    public struct shapeConfig
    {
        public int Count { get; set; }
        public int Mode { get; set; }
        public int Alpha { get; set; }
        public int Repeat { get; set; }

        public shapeConfig(int count, int mode, int alpha, int repeat)
        {
            Count = count;
            Mode = mode;
            Alpha = alpha;
            Repeat = repeat;
        }
    }

    public class ShapeConfigArray
    {
        public List<shapeConfig> ConfigList = new List<shapeConfig>();

        public override string ToString()
        {
            return "";
        }

        public void Set(string value)
        {
            int n = Int32.Parse(value);
            Set(n);
        }

        public void Set(int value)
        {
            int n = value;
            shapeConfig config = new shapeConfig(n, Parameters.Mode, Parameters.Alpha, Parameters.Repeat);
            ConfigList.Add(config);
        }
    }

    //FlagArray
    public class OutputFileArray
    {
        public List<string> FileList = new List<string>();

        public override string ToString()
        {
            return string.Join(", ", FileList);
        }

        public void Set(string value)
        {
            FileList.Add(value);
        }
    }
}
