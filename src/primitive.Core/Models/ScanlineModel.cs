using System.Collections.Generic;

namespace primitive.Core
{
    public class ScanlineModel
    {
        public uint Alpha { get; set; }
        public int Y { get; set; }
        public int X1 { get; set; }
        public int X2 { get; set; }

        public static List<ScanlineModel> CropScanlines(List<ScanlineModel> lines, int w, int h)
        {
            List<ScanlineModel> result = new List<ScanlineModel>();
            foreach (ScanlineModel line in lines)
            {
                if (line.Y < 0 || line.Y >= h)
                    continue;
                if (line.X1 >= w)
                    continue;
                if (line.X2 < 0)
                    continue;
                line.X1 = line.X1.Clamp(0, w - 1);
                line.X2 = line.X2.Clamp(0, w - 1);
                if (line.X1 > line.X2)
                    continue;
                result.Add(line);
            }
            return result;
        }
    }
}
