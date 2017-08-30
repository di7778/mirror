using System;
namespace Utilities
{
    static class HCs
    {
        static Random rnd = new Random();
        static HCs()
        {
            BF = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        }

        public static System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BF;
        public static System.IO.StreamWriter SW;
        public static System.IO.StreamReader SR;
        public static double NextDouble()
        {
            return rnd.NextDouble();
        }
        public static int NextInt(int a, int b)
        {
            return rnd.Next(a, b);
        }
        public static byte Parse(string str)
        {
            return Byte.Parse(str.Substring(str.Length - 1));
        }
        public static byte Parse(object obj)
        {
            string str = obj.ToString();
            return Byte.Parse(str.Substring(str.Length - 1));
        }
        public static double ParseD(string str)
        {
            if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator == ",") return Double.Parse(str.Replace('.', ','));
            else return Double.Parse(str.Replace(',', '.'));
        }
    }

    [Serializable]
    public class Point
    {
        private double x;
        private double y;
        private double z;

        public Point(double X = 0, double Y = 0, double Z = 0)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public double X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public double Z
        {
            get
            {
                return z;
            }
            set
            {
                z = value;
            }
        }

        public static Vector operator -(Point op1, Point op2)
        {
            return new Vector(op1.X  - op2.X, op1.Y - op2.Y, op1.Z - op2.Z);
        }

        public static Point operator +(Point op1, Vector op2)
        {
            return new Point(op1.X + op2.X, op1.Y + op2.Y, op1.Z + op2.Z);
        }

        public static Point operator -(Point op1, Vector op2)
        {
            return new Point(op1.X - op2.X, op1.Y - op2.Y, op1.Z - op2.Z);
        }
    }

    [Serializable]
    public class Direction
    {
        protected double theta;
        protected double phi;

        public Direction(double Theta = 0, double Phi = 0)
        {
            theta = Theta;
            phi = Phi;
        }

        public double Phi
        {
            get
            {
                return phi;
            }
            set
            {
                if (value > 2 * Math.PI || value < 0) throw new ArgumentOutOfRangeException();
                phi = value;
            }
        }

        public double Theta
        {
            get
            {
                return theta;
            }
            set
            {
                if (value > Math.PI || value < 0) throw new ArgumentOutOfRangeException();
                theta = value;
            }
        }

        public static Direction operator -(Direction op1)
        {
            Direction dir = new Direction();
            dir.Theta = Math.PI - op1.Theta;
            if (op1.Phi < Math.PI) dir.Phi = op1.Phi + Math.PI;
            else dir.Phi = op1.Phi - Math.PI;
            return dir;
        }
        public static double operator *(Direction op1, Direction op2)
        {
            return Math.Cos(op1.phi - op2.phi) * Math.Sin(op1.theta) * Math.Sin(op2.theta) + Math.Cos(op1.theta) * Math.Cos(op2.theta);
        }

        public static Vector operator *(double op1, Direction op2)
        {
            return (op1 > 0)? new Vector(op1, op2): new Vector(-op1, -op2);
        }
    }

    [Serializable]
    public class Vector : Direction
    {
        public double R;

        public double X
        {
            get
            {
                return R * Math.Sin(theta) * Math.Cos(phi);
            }
        }

        public double Y
        {
            get
            {
                return R * Math.Sin(theta) * Math.Sin(phi);
            }
        }

        public double Z
        {
            get
            {
                return R * Math.Cos(theta);
            }
        }

        public Point ToPoint(Point p0)
        {
            Point p = new Point();
            p.X = X + p0.X;
            p.Y = Y + p0.Y;
            p.Z = Z + p0.Z;
            return p;
        }

        public Direction GetDirection()
        {
            return this;
        }

        public Vector(double X, double Y, double Z)
        {
            R = Math.Sqrt(X * X + Y * Y + Z * Z);
            theta = Math.Atan2(Math.Sqrt(X * X + Y * Y), Z);
            phi = Math.Atan2(Y, X);
        }

        public Vector(double X, double Y)
        {
            R = Math.Sqrt(X * X + Y * Y);
            theta = 0.5 * Math.PI;
            phi = Math.Atan2(Y, X);
        }

        public Vector(double radius, Direction direction)
        {
            theta = direction.Theta;
            phi = direction.Phi;
            R = radius;
        }

        public Vector Rotate(Vector n, double theta)
        {
            n.R = 1;
            return new Vector((Math.Cos(theta) + (1 - Math.Cos(theta)) * n.X * n.X) * X + ((1 - Math.Cos(theta)) * n.X * n.Y - Math.Sin(theta) * n.Z) * Y + ((1 - Math.Cos(theta)) * n.X * n.Z + Math.Sin(theta) * n.Y) * Z,
                              ((1 - Math.Cos(theta)) * n.Y * n.X + Math.Sin(theta) * n.Z) * X + (Math.Cos(theta) + (1 - Math.Cos(theta)) * n.Y * n.Y) * Y + ((1 - Math.Cos(theta)) * n.Y * n.Z - Math.Sin(theta) * n.X) * Z,
                              ((1 - Math.Cos(theta)) * n.Z * n.X - Math.Sin(theta) * n.Y) * X + ((1 - Math.Cos(theta)) * n.Z * n.Y + Math.Sin(theta) * n.X) * Y + (Math.Cos(theta) + (1 - Math.Cos(theta)) * n.Z * n.Z) * Z);
        }

        public static Vector operator ^(Vector op1, Vector op2)
        {
            return new Vector(op1.Y * op2.Z - op1.Z * op2.Y, op1.Z * op2.X - op1.X * op2.Z, op1.X * op2.Y - op1.Y * op2.X);
        }

        public static double operator *(Vector op1, Vector op2)
        {
            return op1.X * op2.X + op1.Y * op2.Y + op1.Z * op2.Z;
        }

        public static double operator *(Direction op1, Vector op2)
        {
            return new Vector(1, op1) * op2;
        }

        public static double operator *(Vector op1, Direction op2)
        {

            return new Vector(1, op2) * op1;
        }

        public static Vector operator +(Vector op1, Vector op2)
        {
            return new Vector(op1.X + op2.X, op1.Y + op2.Y, op1.Z + op2.Z);
        }

        public static Vector operator -(Vector op1, Vector op2)
        {
            return new Vector(op1.X - op2.X, op1.Y - op2.Y, op1.Z - op2.Z);
        }

        public static Vector operator -(Vector op1)
        {
            return new Vector(-op1.X, -op1.Y, -op1.Z);
        }

        public static Vector operator *(double op1, Vector op2)
        {
            return new Vector(op1 * op2.X, op1 * op2.Y, op1 * op2.Z);
        }

        public static Vector operator *(Vector op1, double op2)
        {
            return new Vector(op1.X * op2, op1.Y * op2, op1.Z * op2);
        }

        public Vector Copy()
        {
            return new Vector(this.X, this.Y, this.Z);
        }
    }
}
