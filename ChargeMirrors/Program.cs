using System;
using System.Collections.Generic;
using Utilities;

namespace ChargeMirrors
{
    class Program
    {
        static void Main(string[] args)
        {
            Plane plane = new Plane();
            plane.d = new Direction();
            plane.p = new Point();
            Ball b1 = new Ball();
            b1.r = 1;
            b1.p = new Point();
            Charge q1 = new Charge();
            q1.q = 1;
            q1.p = new Point(0, 0, 2);
            Charge q2 = plane.Mirror(q1);
            Charge q3 = b1.Mirror(q1);
            Charge q4 = b1.MirrorC(q1);
            int i = 0;
        }
    }
}
