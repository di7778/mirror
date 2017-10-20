using System;
using Utilities;

namespace RungeKutta
{
    class RungeKutta
    {
        public static void Solve(double t0, double t2, int n, ref double L, ref double U, Func<double, double, double, double> f, Func<double, double, double, double> g)
        {
            double h = (t2 - t0) / n; // шаг
            double t1 = t0;

            for (int i = 0; i < n; i++)
            {
                double k1 = f(t1, L, U) * h;
                double m1 = g(t1, L, U) * h;
                double k2 = f(t1 + h / 2, L + k1 / 2, U + m1 / 2) * h;
                double m2 = g(t1 + h / 2, L + k1 / 2, U + m1 / 2) * h;
                double k3 = f(t1 + h / 2, L + k2 / 2, U + m2 / 2) * h;
                double m3 = g(t1 + h / 2, L + k2 / 2, U + m2 / 2) * h;
                double k4 = f(t1 + h, L + k3, U + m3) * h;
                double m4 = g(t1 + h, L + k3, U + m3) * h;
                L += (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                U += (m1 + 2 * m2 + 2 * m3 + m4) / 6;
                t1 += h;
            }
            return;
        }

        public static void Solve(double t0, double t2, int n, ref Vector L, ref Vector U, Func<double, Vector, Vector, Vector> f, Func<double, Vector, Vector, Vector> g)
        {
            double h = (t2 - t0) / n; // шаг
            double t1 = t0;

            for (int i = 0; i < n; i++)
            {
                Vector k1 = f(t1, L, U) * h;
                Vector m1 = g(t1, L, U) * h;
                Vector k2 = f(t1 + h / 2, L + k1 / 2, U + m1 / 2) * h;
                Vector m2 = g(t1 + h / 2, L + k1 / 2, U + m1 / 2) * h;
                Vector k3 = f(t1 + h / 2, L + k2 / 2, U + m2 / 2) * h;
                Vector m3 = g(t1 + h / 2, L + k2 / 2, U + m2 / 2) * h;
                Vector k4 = f(t1 + h, L + k3, U + m3) * h;
                Vector m4 = g(t1 + h, L + k3, U + m3) * h;
                L += (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                U += (m1 + 2 * m2 + 2 * m3 + m4) / 6;
                t1 += h;
            }
            return;
        }

        public static void Solve(double dt, int n, ref double L, ref double U, Func<double, double, double> f, Func<double, double, double> g)
        {
            double h = dt / n; // шаг
            for (int i = 0; i < n; i++)
            {
                double k1 = f(L, U) * h;
                double m1 = g(L, U) * h;
                double k2 = f(L + k1 / 2, U + m1 / 2) * h;
                double m2 = g(L + k1 / 2, U + m1 / 2) * h;
                double k3 = f(L + k2 / 2, U + m2 / 2) * h;
                double m3 = g(L + k2 / 2, U + m2 / 2) * h;
                double k4 = f(L + k3, U + m3) * h;
                double m4 = g(L + k3, U + m3) * h;
                L += (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                U += (m1 + 2 * m2 + 2 * m3 + m4) / 6;
            }
            return;
        }

        public static void Solve(double dt, int n, ref Vector L, ref Vector U, Func<Vector, Vector, Vector> f, Func<Vector, Vector, Vector> g)
        {
            double h = dt / n; // шаг
            for (int i = 0; i < n; i++)
            {
                Vector k1 = f(L, U) * h;
                Vector m1 = g(L, U) * h;
                Vector k2 = f(L + k1 / 2, U + m1 / 2) * h;
                Vector m2 = g(L + k1 / 2, U + m1 / 2) * h;
                Vector k3 = f(L + k2 / 2, U + m2 / 2) * h;
                Vector m3 = g(L + k2 / 2, U + m2 / 2) * h;
                Vector k4 = f(L + k3, U + m3) * h;
                Vector m4 = g(L + k3, U + m3) * h;
                L += (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                U += (m1 + 2 * m2 + 2 * m3 + m4) / 6;
            }
            return;
        }

        public static void Solve(double t0, double t2, int n, ref double L, Func<double, double, double> f)
        {
            double h = (t2 - t0) / n; // шаг
            double t1 = t0;

            for (int i = 0; i < n; i++)
            {
                double k1 = f(t1, L) * h;
                double k2 = f(t1 + h / 2, L + k1 / 2) * h;
                double k3 = f(t1 + h / 2, L + k2 / 2) * h;
                double k4 = f(t1 + h, L + k3) * h;
                L += (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                t1 += h;
            }
            return;
        }

        public static void Solve(double dt, int n, ref double L, Func<double, double> f)
        {
            double h = dt / n; // шаг
            for (int i = 0; i < n; i++)
            {
                double k1 = f(L) * h;
                double k2 = f(L + k1 / 2) * h;
                double k3 = f(L + k2 / 2) * h;
                double k4 = f(L + k3) * h;
                L += (k1 + 2 * k2 + 2 * k3 + k4) / 6;
            }
            return;
        }
    }
}
