using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using primitive.Core;

namespace primitive.Console
{
    [HelpOption]
    class PrimitiveCS
    {
        static void Main(string[] args) => CommandLineApplication.Execute<PrimitiveCS>(args);

        #region InputParameters
        [Required]
        [Option(Description = "Required. Input file.",
            Template = "-i|--input")]
        public string InputFile { get; }

        [Required]
        [Option(Description = "Required. Output file.",
            Template = "-o|--output")]
        public string OutputFiles { get; }

        [Required]
        [Option(Description = "Required. Number of shapes.",
            Template = "-n|--number")]
        public int? Nprimitives { get; }

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
            ParametersModel parameters = new ParametersModel();
            // parse and validate arguments
            parameters.Mode = (ShapeType)Mode;
            parameters.Alpha = (byte)(Alpha ?? 128);
            parameters.Repeat = Repeat ?? 0;
            parameters.Nprimitives = Nprimitives ?? 10;
            parameters.NthFrame = NthFrame ?? 1;
            parameters.CanvasResize = InputResize ?? 256;
            parameters.RenderSize = OutputSize ?? 1024;
            parameters.WorkersCount = Workers ?? 0;

            // set log level
            if (Verbose)
                parameters.LogLevel = 1;
            if (VeryVerbose)
                parameters.LogLevel = 2;

            // determine worker count
            if (parameters.WorkersCount < 1)
                parameters.WorkersCount = Environment.ProcessorCount;

            // read input image
            Logger.WriteLine(1, "reading {0}", InputFile);
            Image<Rgba32> inputImage = Util.LoadImage(InputFile);

            // determine background color
            if (String.IsNullOrEmpty(Background))
                parameters.Background = Util.AverageImageColor(inputImage);
            else
                parameters.Background = Rgba32.FromHex(Background);

            // run algorithm
            RendererModel model = new RendererModel(inputImage, parameters);
            model.RunRenderer();

            // write output image(s)
            foreach (var outFile in OutputFiles.Split(' '))
            {
                var ext = Path.GetExtension(outFile).ToLower();
                bool percent = outFile.Contains("{0");
                bool saveFrames = percent && !ext.Equals(".gif");
                
                Logger.WriteLine(1, "writing {0}", outFile);
                switch (ext)
                {
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                        Util.SaveFrames(outFile, model.GetFrames(saveFrames, parameters.NthFrame));
                        break;
                    case ".svg":
                        Util.SaveSVG(outFile, model.GetSVG(saveFrames, parameters.NthFrame));
                        break;
                    case ".gif":
                        Util.SaveGIF(outFile, model.GetFrames(0.001), 0, 0);
                        break;
                    default:
                        throw new Exception("unrecognized file extension: " + ext);
                }

            }

            System.Console.WriteLine("End");
        }
    }
}
