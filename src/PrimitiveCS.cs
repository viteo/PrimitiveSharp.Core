using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Dynamic;
using McMaster.Extensions.CommandLineUtils;

namespace primitive
{
    [HelpOption]
    class PrimitiveCS
    {
        static void Main(string[] args) => CommandLineApplication.Execute<PrimitiveCS>(args);

        public static Random Rand;

        [Required]
        [Option(Description = "Required. Input file.",
            Template = "-i|--input")]
        public string Input { get; }

        [Required]
        [Option(Description = "Required. Output file.",
            Template = "-o|--output")]
        public string Output { get; }

        [Required]
        [Option(Description = "Required. Number of shapes.",
            Template = "-n|--number")]
        public int Nprimitives { get; }

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
        public bool? Verbose { get; }

        [Option(Description = "Very verbose output",
            Template = "-vv")]
        public bool? VeryVerbose { get; }


        private void OnExecute()
        {
            // parse and validate arguments
            Parameters.InputFile = Input;
            //Parameters.Outputs = Output;
            Parameters.Mode = Mode ?? 1;
            Parameters.Alpha = Alpha ?? 128;
            Parameters.Repeat = Repeat ?? 0;
            Parameters.Configs.Set(Nprimitives);
            Parameters.Nth = NthFrame ?? 1;
            Parameters.InputResize = InputResize ?? 256;
            Parameters.OutputSize = OutputSize ?? 1024;
            Parameters.Background = Background ?? "";
            Parameters.Workers = Workers ?? 0;

            // set log level
            if (Verbose ?? false)
                Parameters.LogLevel = 1;
            if (VeryVerbose ?? false)
                Parameters.LogLevel = 2;

            // seed random number generator
            Rand = new Random((int)DateTime.Now.Ticks);

            // determine worker count
            if (Parameters.Workers < 1)
                Parameters.Workers = Environment.ProcessorCount;

            // read input image
            Logger.WriteLine(1, "reading {0}", Parameters.InputFile);
            Image inputImage;
            inputImage = Util.LoadImage(Parameters.InputFile);


            // scale down input image if needed
            if (Parameters.InputResize > 0)
                inputImage = Util.Resize(inputImage);

            // determine background color
            Color bgColor;
            if (Parameters.Background == "")
                bgColor = Util.AverageImageColor(inputImage);
            else
                bgColor = ColorTranslator.FromHtml(Parameters.Background);


            // run algorithm



            Console.WriteLine("End");
        }

    }

    public static class Parameters
    {
        public static string InputFile;
        public static FlagArray Outputs;
        public static ShapeConfigArray Configs;
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
        public int Count;
        public int Mode;
        public int Alpha;
        public int Repeat;

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
        public List<shapeConfig> Configs;

        public override string ToString()
        {
            return "";
        }

        public void Set(string value)
        {
            int n = Int32.Parse(value);
            shapeConfig config = new shapeConfig(n, Parameters.Mode, Parameters.Alpha, Parameters.Repeat);
            Configs.Add(config);
        }

        public void Set(int value)
        {
            int n = value;
            shapeConfig config = new shapeConfig(n, Parameters.Mode, Parameters.Alpha, Parameters.Repeat);
            Configs.Add(config);
        }
    }

    public class FlagArray
    {
        public List<string> flags;

        public override string ToString()
        {
            return string.Join(", ", flags);
        }

        public void Set(string flag)
        {
            flags.Add(flag);
        }
    }
}
