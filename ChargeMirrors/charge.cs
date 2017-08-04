using Utilities;

namespace ChargeMirrors
{
    public class Charge
    {
        public double q;
        public Point p;
    }

    public class Ball
    {
        public double r;
        public Point p;
        public Charge Mirror(Charge q)
        {
            Vector chloc = q.p - p;
            Vector mirloc = r * r / chloc.R * chloc.GetDirection();
            Charge q2 = new Charge();
            q2.q = -q.q * r / chloc.R;
            q2.p = mirloc.ToPoint(p);
            return q2;
        }
        public Charge MirrorC(Charge q)
        {
            Vector chloc = q.p - p;
            Charge q2 = new Charge();
            q2.q = q.q * r / chloc.R;
            q2.p = p;
            return q2;
        }
    }

    public class Plane
    {
        public Point p;
        public Direction d;
        public Charge Mirror(Charge q)
        {
            Vector chloc = q.p - p;
            Vector mirloc = chloc - (2 * chloc * d) * d;
            Charge q2 = new Charge();
            q2.q = -q.q;
            q2.p = mirloc.ToPoint(p);
            return q2;
        }
    }
}
