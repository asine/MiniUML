using System;
using System.Globalization;
using System.Windows;
using System.Xml.Linq;

/* This assembly contains functionality common to all MiniUML assemblies and should hence not depend on any other MiniUML assemblies! */

namespace MiniUML.Framework
{
    public static class FrameworkUtilities
    {
        public static String GetStringAttribute(this XElement elm, String attributeName)
        {
            return elm.GetStringAttribute(attributeName, "");
        }

        public static String GetStringAttribute(this XElement elm, String attributeName, String fallback)
        {
            XAttribute attrib = elm.Attribute(attributeName);
            if (attrib == null) return fallback;
            return attrib.Value;
        }

        public static double GetDoubleAttribute(this XElement elm, String attributeName, double fallback)
        {
            XAttribute attrib = elm.Attribute(attributeName);
            if (attrib == null) return fallback;

            double result;
            if (Double.TryParse(attrib.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) return result;
            return fallback;
        }

        public static Point GetPositionAttributes(this XElement elm)
        {
            return new Point(elm.GetDoubleAttribute("Left", 0.0), elm.GetDoubleAttribute("Top", 0.0));
        }

        public static void SetPositionAttributes(this XElement elm, Point pos)
        {
            elm.SetAttributeValue("Top", pos.Y);
            elm.SetAttributeValue("Left", pos.X);
        }

        public static double GetAngularCoordinate(this Vector v)
        {
            if (v.X > 0) return Math.Atan(v.Y / v.X);
            else if (v.X < 0 && v.Y >= 0) return Math.Atan(v.Y / v.X) + Math.PI;
            else if (v.X < 0 && v.Y < 0) return Math.Atan(v.Y / v.X) - Math.PI;
            else if (v.X == 0 && v.Y > 0) return Math.PI / 2;
            else if (v.X == 0 && v.Y < 0) return -Math.PI / 2;
            else return 0; // x = y = 0
        }

        // Computes the vector resolute or projection of v onto o.
        public static Vector VectorProjection(this Vector v, Vector o)
        {
            return (o * v) / o.LengthSquared * o;
        }

        // Computes the scalar resolute or projection of v onto o.
        public static double ScalarProjection(this Vector v, Vector o)
        {
            return (o * v) / o.Length;
        }

        public static double NormalizeAngle(double a)
        {
            return a - (Math.Floor(a / (2 * Math.PI))) * (2 * Math.PI);
        }

        public static double RadiansToDegrees(double rad)
        {
            return rad * 180 / Math.PI;
        }

        public static double DegreesToRadians(double rad)
        {
            return rad / 180 * Math.PI;
        }
    }
}
