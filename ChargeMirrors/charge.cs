using System;
using Utilities;

namespace ChargeMirrors
{
    public enum Domains { Substrate, LeftBall, RightBall, Undef };

    public class Charge
    {
        public double q;
        public Point p;
        public Domains dom;
        public Vector ElField(Point p)
        {
            Vector r = (p - this.p);
            double mE = q / (4 * Math.PI * 8.85418781762E-12) / (r.R * r.R);
            r.R = 1;
            Vector E = mE * r;
            return E;
        }
    }

    public class Ball
    {
        public double r;
        public Point p;
        public Domains dom;

        public Charge Mirror(Charge q)
        {
            Vector chloc = q.p - p;
            Vector mirloc = r * r / chloc.R * chloc.GetDirection();
            Charge q2 = new Charge();
            q2.q = -q.q * r / chloc.R;
            q2.p = mirloc.ToPoint(p);
            q2.dom = dom;
            return q2;
        }
        public Charge MirrorC(Charge q)
        {
            Vector chloc = q.p - p;
            Charge q2 = new Charge();
            q2.q = q.q * r / chloc.R;
            q2.p = p;
            q2.dom = dom;
            return q2;
        }
    }

    public class Plane
    {
        public Point p;
        public Direction d;
        public Domains dom = Domains.Substrate;

        public Charge Mirror(Charge q)
        {
            Vector chloc = q.p - p;
            Vector mirloc = chloc - (2 * chloc * d) * d;
            Charge q2 = new Charge();
            q2.q = -q.q;
            q2.p = mirloc.ToPoint(p);
            q2.dom = Domains.Substrate;
            return q2;
        }
    }

    public class Polarization
    {
        
        double q;
        double alpha;
        Vector pol;

        const double dCO = 0.114687;

        public double Q
        {
            get
            {
                return q;
            }
        }

        public double Alpha
        {
            get
            {
                return alpha;
            }
        }

        public Vector Pol
        {
            get
            {
                return pol;
            }
        }

        public Polarization(Vector E, Vector MO)
        {
            MO.R = 1;
            double Ep = E * MO;
            double Dp = Ep * (0.5468 - 0.1252) / 5.142206707E9 + 0.1252;

            Vector T = (MO ^ E) ^ MO;
            T.R = 1;
            double En = E * T;
            double Dn = En * 0.2425 / 5.142206707E9;

            q = Math.Sqrt(Dp * Dp + Dn * Dn) * 0.333564095 / 1.6022 / 10.0 / dCO;
            alpha = Math.Atan2(Dn, Dp);
            Vector D = (Dp * MO) + (Dn * T);
            pol = new Vector(0.5 * dCO, D.GetDirection());
        }
    }
}
