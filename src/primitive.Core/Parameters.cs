using System;
using System.Collections.Generic;
using System.Text;

namespace primitive.Core
{
    public static class Parameters
    {
            public static string InputFile;
            public static List<string> OutputFiles = new List<string>();
            public static List<ShapeConfig> ShapeConfigs = new List<ShapeConfig>();
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

    public struct ShapeConfig
    {
        public int Count { get; set; }
        public int Mode { get; set; }
        public int Alpha { get; set; }
        public int Repeat { get; set; }
    }
}
