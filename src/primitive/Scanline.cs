using System;
using System.Collections.Generic;
using System.Text;

namespace primitive
{
    public class Scanline
    {
        public uint Alpha { get; set; }
        public int Y { get; set; }
        public int X1 { get; set; }
        public int X2 { get; set; }

        public static List<Scanline> CropScanlines(List<Scanline> lines, int w, int h)
        {
            List<Scanline> result = new List<Scanline>();
            foreach (Scanline line in lines)
            {
                if (line.Y < 0 || line.Y >= h)
                    continue;
                if (line.X1 >= w)
                    continue;
                if (line.X2 < 0)
                    continue;
                line.X1 = Util.ClampInt(line.X1, 0, w - 1);
                line.X2 = Util.ClampInt(line.X2, 0, w - 1);
                if (line.X1 > line.X2)
                    continue;
                result.Add(line);
            }
            return result;
        }
    }
}
