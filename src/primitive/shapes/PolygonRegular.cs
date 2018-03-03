using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.Primitives;

namespace primitive.NetCore.primitive.shapes
{
    public class PolygonRegular
    {
        public static PointF[] CalculateVertices(int sides, double radius, double startingAngle, PointF center)
        {
            if (sides < 3)
                throw new ArgumentException("Polygon must have 3 sides or more.");

            PointF[] points = new PointF[sides];
            float step = 360.0f / sides;
            double angle = startingAngle;

            for (int i = 0; i < sides; i++)
            {
                points[i] = DegreesToXY(angle, radius, center);
                angle += step;
            }

            return points;
        }

        /// <summary>
        /// Calculates a point that is at an angle from the origin (0 is to the right)
        /// </summary>
        private static PointF DegreesToXY(double degrees, double radius, PointF origin)
        {
            PointF xy = new PointF();
            double radians = degrees * Math.PI / 180.0;

            xy.X = (float)(Math.Cos(radians) * radius) + origin.X;
            xy.Y = (float)(Math.Sin(-radians) * radius) + origin.Y;

            return xy;
        }
    }
}
