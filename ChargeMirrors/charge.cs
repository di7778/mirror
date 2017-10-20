using System;
using System.Collections.Generic;
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
            const double normE = 1.6022E-19 / 1E-18 / (4 * Math.PI * 8.85418781762E-12);
            double mE = normE * q / (r.R * r.R); // поле вольт на м
            r.R = 1;
            Vector E = mE * r;
            return E;
        }
        public static Vector GetElField(Queue<Charge> queue, Point point)
        {
            Charge[] array = new Charge[queue.Count];
            queue.CopyTo(array, 0);
            Vector E = new Vector(0, 0, 0);
            for (int i = 0; i < queue.Count; i++)
            {
                Charge q = array[i];
                if (q.dom != Domains.Undef)
                {
                    E += q.ElField(point);
                }
            }
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
            Charge q2 = new Charge
            {
                q = -q.q * r / chloc.R,
                p = mirloc.ToPoint(p),
                dom = dom
            };
            return q2;
        }
        public Charge MirrorC(Charge q)
        {
            Vector chloc = q.p - p;
            Charge q2 = new Charge
            {
                q = q.q * r / chloc.R,
                p = p,
                dom = dom
            };
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
            Charge q2 = new Charge
            {
                q = -q.q,
                p = mirloc.ToPoint(p),
                dom = Domains.Substrate
            };
            return q2;
        }
    }

    public class Polarization
    {

        double q;
        double alpha;
        Vector pc;
        Vector po;

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

        public Vector pC
        {
            get
            {
                return pc;
            }
        }
        public Vector pO
        {
            get
            {
                return po;
            }
        }

        public Polarization(Vector E, Vector MC)
        {
            const double mO = 16; // масса О в кг
            const double mC = 12; // масса С в кг

            MC.R = 1;
            
            double Ep = E * MC;
            double Dp = Ep * (0.5468 - 0.1252) / 5.142206707E9 + 0.1252;
            double Epmod = 2E10 * (1 - 1 / (Ep / 2E10 + 1));
            double Dpmod = Epmod * (0.5468 - 0.1252) / 5.142206707E9 + 0.1252;


            Vector T = (MC ^ E) ^ MC;
            T.R = 1;
            double En = E * T;

            double Dn = En * 0.2425 / 5.142206707E9;
            double Enmod = 2E10 * (1 - 1 / (Ep / 2E10 + 1));
            double Dnmod = Enmod * 0.2425 / 5.142206707E9;


            q = Math.Sqrt(Dp * Dp + Dn * Dn) * 0.333564095 / 1.6022 / 10.0 / dCO;
            alpha = Math.Atan2(Dn, Dp);
            Vector D = (Dp * MC) + (Dn * T);
            pc = new Vector(mO / (mO + mC) * dCO, D.GetDirection());
            po = new Vector(mC / (mO + mC) * dCO, -D.GetDirection());

        }
    }

    public class Molecule
    {

        public Molecule(Point r0, Vector v0, Vector dir, double Erot)
        {
            R0 = r0.ToVector();
            V0 = v0;
            phi = dir.Copy();
            phi.R = 1;
            omega = new Vector(0, Math.Sqrt(2 * q * Erot / (m1 * (r1.R * r1.R) + m2 * (r2.R * r2.R))),0);
        }

        const double aem = 6.022E23;
        const double dCO = 0.114687;
        double Qp;
        const double q = 1.6022E-19;

        const double m1 = 0.001 * 16 / aem; // масса О в кг
        const double m2 = 0.001 * 12 / aem; // масса С в кг


        Vector E1;
        Vector E2;

        public Vector R0;
        Vector V0;

        public Vector phi;
        public Vector omega;

        Vector F1
        {
            get
            {
                return +Qp * q * E1; // сила в Н на атом С
            }
        }
        Vector F2
        {
            get
            {
                return -Qp * q * E2;
            }
        }
        Vector a_cm
        {
            get
            {
                return (F1 + F2) / (m1 + m2); // м / с^2
            }
        }
        Vector r1
        {
            get
            {
                return phi * m2 / (m1 + m2) * 1E-9; // координаты атома С в м
            }
        }
        Vector r2
        {
            get
            {
                return -phi * m1 / (m1 + m2) * 1E-9; // координаты атома О в м
            }
        }
        public Vector alpha
        {
            get
            {
                return ((r1 ^ F1) + (r2 ^ F2)) / (m1 * (r1.R * r1.R) + m2 * (r2.R * r2.R));
            }
        }

        public double Etr
        {
            get
            {
                return (m1 + m2) * V0 * V0 / 2 / q;
            }
        }

        public double Erot
        {
            get
            {
                return (m1 * (r1 * r1) + m2 * (r2 * r2)) * omega * omega / 2 / q;
            }
        }

        Vector g(Vector L, Vector U)
        {
            return a_cm;
        }

        Vector f(Vector L, Vector U)
        {
            return U / 1E-9;
        }

        Vector g_rot(Vector L, Vector U)
        {
            return alpha;
        }

        Vector f_rot(Vector L, Vector U)
        {
            return U ^ L;
        }

        public void doStepRK(Vector e1, Vector e2, double Q, double dt)
        {
            E1 = e1;
            E2 = e2;
            Qp = Q;

            RungeKutta.RungeKutta.Solve(dt, 10, ref R0, ref V0, f, g);

            //R0 += (V0 * dt + a_cm * dt * dt / 2) / 1E-9; // R0 - нм
            //V0 += a_cm * dt;

            RungeKutta.RungeKutta.Solve(dt, 10, ref phi, ref omega, f_rot, g_rot);

            //omega += alpha * dt;
            //phi += (omega ^ phi) * dt;
            phi.R = 1;

            if (double.NaN.Equals(R0.X) || double.NaN.Equals(R0.Y) || double.NaN.Equals(R0.Z))
            {
                int i = 0;
            }
        }

    }

    /*
    public class Pendulum
    {
        const double aem = 6.022E23;
        // const double dCO = 0.114687;
        double Qp;
        double E;
        const double q = 1.6022E-19;

        const double m = 0.001 * 16 / aem; // масса О в кг
        //const double m2 = 0.001 * 12 / aem; // масса С в кг



        public double dx = 0;
        double V0 = 200;

        double Omega
        {
            get
            {
                return 6.1868E13 - E / 5E9 * (6.1868E13 - 6.1036E13);
            }

        }

        double F
        {
            get
            {
                return -m * Omega * Omega * dx * 1E-9 + Qp * q * E; // сила в Н 
            }
        }
        double A_cm
        {
            get
            {
                return F / m; // м / с^2
            }
        }

        public void DoStep(double dt, double e, double q)
        {
            Qp = q;
            E = e;

            dx += (V0 * dt + A_cm * dt * dt / 2) / 1E-9; // R0 - нм
            V0 += A_cm * dt;
        }
    }
    */
    public class LinePendulum
    {
        const double aem = 6.022E23;
        const double k_b = 1.380662E-23;
        const double q_e = 1.6022E-19;

        const double T = 300;
        const double m1 = 0.001 * 16 / aem; // масса О в кг
        const double m2 = 0.001 * 12 / aem; // масса С в кг
        const double M = m1 + m2;


        const double nu0 = 6.1868E13;
        const double nu5 = 6.1036E13;
        const double t0 = 1 / nu0;
        const double E0 = 5E9;


        double V0;
        double l0;
        double Q0;

        double Q;

        double epsilon;
        double lambda;
        double V;

        public double Vn
        {
            get
            {
                return V;
            }
        }

        public double En
        {
            get
            {
                return M * V * V * V0 * V0 / q_e / 2;
            }
        }

        public LinePendulum()
        {
            V0 = Math.Sqrt(k_b * T / M);
            V = 1;

            l0 = V0 * t0;
            Q0 = q_e * E0 * t0 / M / V0;
        }

        public double E
        {
            get
            {
                return epsilon * E0;
            }
            set
            {
                epsilon = value / E0;
            }
        }

        public double q
        {
            get
            {
                return Q / Q0;
            }
            set
            {
                Q = value * Q0;
            }
        }

        double Omega
        {
            get
            {
                return 2 * Math.PI * (1 - epsilon * (1 - nu5 / nu0));
                //return 2 * Math.PI;
            }
        }

        
        public double dx
        {
            get
            {
                return lambda * l0;
            }
            set
            {
                lambda = value / l0;
            }
        }
        
        public void DoStepRK(double dt, double e, double q)
        {
            this.q = q;
            E = e;
            dt = dt / t0;
            RungeKutta.RungeKutta.Solve(dt, 10, ref lambda, ref V, f, g);
        }

        double g(double L, double U)
        {
            return -Omega * Omega * L + Q * epsilon;
        }
        double f(double L, double U)
        {
            return U;
        }
    }
}
