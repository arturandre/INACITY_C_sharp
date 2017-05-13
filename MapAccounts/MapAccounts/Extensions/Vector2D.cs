using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MapAccounts.Extensions
{
    public class Vector2D
    {
        private double x;
        private double y;

        public double X { get { return x; } }
        public double Y { get { return y; } }

        public Vector2D(double _x, double _y)
        {
            x = _x;
            y = _y;
        }

        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.x + b.x, a.y + b.y);
        }
        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.x - b.x, a.y - b.y);
        }
        public static Vector2D operator *(Vector2D a, double c)
        {
            return new Vector2D(a.x*c, a.y*c);
        }
        public static double dot(Vector2D a, Vector2D b)
        {
            return (a.x * b.x) + (a.y * b.y);
        }
        public static double norm(Vector2D a)
        {
            return Math.Sqrt((a.x * a.x) + (a.y * a.y));
        }
        public static double angleBetween(Vector2D a, Vector2D b)
        {
            var na = normalized(a);
            var nb = normalized(b);

            var d = Vector2D.dot(na, nb);

            return (d >= 0) ? Math.Acos(d) : Math.PI*2.0 - Math.Acos(d);

        }
        public static Vector2D normalized(Vector2D a)
        {
            double norm = Vector2D.norm(a);
            if (norm == 0) throw new DivideByZeroException("Vector should be non-zero!");
            return a * (1.0 / norm);
        }
    }
}