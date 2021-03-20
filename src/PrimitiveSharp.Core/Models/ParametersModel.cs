using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PrimitiveSharp.Core
{
    public class ParametersModel
    {
        public int Nprimitives { get; set; } = 100;
        public ShapeType Mode { get; set; } = ShapeType.Any;
        public byte Alpha { get; set; } = 0x80;
        public int Repeat { get; set; } = 0;
        public Rgba32 Background { get; set; } = Color.Transparent;
        public int CanvasResize { get; set; } = 256;
        public int RenderSize { get; set; } = 1024;
        public int WorkersCount { get; set; } = Environment.ProcessorCount;
        public int NthFrame { get; set; } = 1;
        public int LogLevel { get; set; } = 0;
        public int ShapeProbeCount { get; set; } = 1000;
        public int ShapeAge { get; set; } = 100;
    }
}
