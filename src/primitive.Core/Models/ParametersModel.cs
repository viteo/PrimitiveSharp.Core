using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.PixelFormats;

namespace primitive.Core
{
    public class ParametersModel
    {
        public int Nprimitives { get; set; } = 100;
        public ShapeType Mode { get; set; } = ShapeType.Any;
        public byte Alpha { get; set; } = 0x80;
        public int Repeat { get; set; } = 0;
        public Rgba32 Background { get; set; }
        public int CanvasResize { get; set; } = 256;
        public int RenderSize { get; set; } = 1024;
        public int WorkersCount { get; set; } = Environment.ProcessorCount;
        public int NthFrame { get; set; } = 1;
        public int LogLevel { get; set; } = 0;
    }
}
