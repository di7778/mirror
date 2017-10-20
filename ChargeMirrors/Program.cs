using Spline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Utilities;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace ChargeMirrors
{
    class Program
    {
        static void Main(string[] args)
        {
            const double t0 = 1E-14;
            const double dThick = 0.1;
            const double dAtom = 0.1;

            //const double tm = 1E-16;

            //            Plane plane = new Plane();
            //            plane.d = new Direction();
            //            plane.p = new Point();
            //            plane.dom = Domains.Substrate;


            Ball leftBall = new Ball
            {
                r = 1,
                p = new Utilities.Point(-1.1, 0, 1.1),
                dom = Domains.LeftBall
            };


            Ball rightBall = new Ball
            {
                r = 1,
                p = new Utilities.Point(1.1, 0, 1.1),
                dom = Domains.RightBall
            };

            Process currentProcess = Process.GetCurrentProcess();
            int id = currentProcess.Id;
            do
            {
                double tc = t0;
                double time = 0;

                List<double> times = new List<double>();
                List<double> fields = new List<double>();
                List<double> charges = new List<double>();

                times.Add(0.0);
                fields.Add(0.0);
                charges.Add(0.0);

                //Point MCenter = new Point(5, 2.4 * HCs.NextDouble() - 1.2, 0.1 + 2.3 * HCs.NextDouble());
                Utilities.Point MCenter = new Utilities.Point(0, 0, 3.1);
                //Vector mol1 = new Vector(1.1 )
                //Utilities.Point MCenter = new Utilities.Point(leftBall.p, 0, 3.1);

                //Vector Vel0 = new Vector(-400, 0, 0);
                Vector Vel0 = new Vector(0, 0, -400);
                //Vector MAtomC = new Vector(0.11, new Direction(Math.PI / 2, 0)); // начальное направление
                Vector MAtomC = new Vector(0.11, new Direction(Math.PI / 2, 0)); // начальное направление

                Molecule mol = new Molecule(MCenter, Vel0, MAtomC, 0.025/2);
                Vector E0 = new Vector(0, 0, 0);
                Vector EO = new Vector(0, 0, 0);
                Vector EC = new Vector(0, 0, 0);
                Polarization pol = new Polarization(E0, MAtomC);
                Polarization polPrev = pol;
                double Eprev = 2.18E8;

                do
                {
                    int iterLimit = 1000;
                    int curIter = 0;
                    int precLimit = 8;
                    double curPrec;

                    do
                    {
                        // O
                        Charge q1 = new Charge
                        {
                            q = -pol.Q,
                            p = MCenter + pol.pO,
                            dom = Domains.Undef
                        };

                        // C
                        Charge q2 = new Charge
                        {
                            q = +pol.Q,
                            p = MCenter + pol.pC,
                            dom = Domains.Undef
                        };

                        Charge q3 = new Charge
                        {
                            q = +1,
                            p = new Utilities.Point(-1.1, 0, 1.1),
                            dom = Domains.LeftBall
                        };

                        Charge q4 = new Charge
                        {
                            q = -1,
                            p = new Utilities.Point(+1.1, 0, 1.1),
                            dom = Domains.RightBall
                        };

                        Queue<Charge> queue = new Queue<Charge>();
                        Queue<Charge> mqueue = new Queue<Charge>();
                        queue.Enqueue(q1);
                        queue.Enqueue(q2);
                        queue.Enqueue(q3);
                        queue.Enqueue(q4);

                        int maxcounter = 4;

                        int curstage = 0;
                        int maxstage = 8;

                        while (curstage < maxstage)
                        {
                            int gencounter = 0;
                            int curcounter = 0;
                            while (curcounter < maxcounter)
                            {
                                Charge q = queue.Dequeue();
                                
                                if (q.dom != Domains.LeftBall)
                                {
                                    queue.Enqueue(leftBall.Mirror(q));
                                    gencounter++;
                                    queue.Enqueue(leftBall.MirrorC(q));
                                    gencounter++;
                                }
                                
                                if (q.dom != Domains.RightBall)
                                {
                                    queue.Enqueue(rightBall.Mirror(q));
                                    gencounter++;
                                    queue.Enqueue(rightBall.MirrorC(q));
                                    gencounter++;
                                }
                                /*
                                if (q.dom != Domains.Substrate)
                                {
                                    queue.Enqueue(plane.Mirror(q));
                                    gencounter++;
                                }
                                */
                                mqueue.Enqueue(q);
                                curcounter++;
                            }
                            maxcounter = gencounter;
                            curstage++;
                        }

                        while (queue.Count > 0)
                        {
                            mqueue.Enqueue(queue.Dequeue());
                        }

                        E0 = Charge.GetElField(mqueue, MCenter);
                        polPrev = pol;
                        pol = new Polarization(E0, MAtomC);
                        EO = Charge.GetElField(mqueue, MCenter + pol.pO); // O
                        EC = Charge.GetElField(mqueue, MCenter + pol.pC); // C

                        curPrec = -Math.Log10(Math.Abs((pol.Q - polPrev.Q) / polPrev.Q));
                        curIter++;
                    } while (curIter < iterLimit && curPrec < precLimit);
                    tc = GetTimeStep((EO.R + EC.R) / 2, mol.Etr);

                    mol.doStepRK(EO, EC, pol.Q, tc);
                    time += tc;
                    MCenter = mol.R0.ToPoint();
                    MAtomC = mol.phi;
                    double distO = Math.Min((MCenter + pol.pO - leftBall.p).R, (MCenter + pol.pO - rightBall.p).R);
                    double distC = Math.Min((MCenter + pol.pC - leftBall.p).R, (MCenter + pol.pC - rightBall.p).R);
                    double dist = Math.Min(distO, distC);

                    /*
                    if (dist <= 1.0015)
                    {
                        HCs.SW = new StreamWriter("matrix" + id + ".txt", true);
                        HCs.SW.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8}", dist, MCenter.X, MCenter.Y, MCenter.Z,
                        E0.R, pol.Q, Math.Acos(MAtomC.GetDirection() * E0.GetDirection()), mol.Etr, mol.Erot);
                        HCs.SW.Close();
                        break;
                    }
                    */

                    //if (MCenter.Z < 0.10 || MCenter.X < 0.15 || double.NaN.Equals(MCenter.X) || double.NaN.Equals(MCenter.Y) || double.NaN.Equals(MCenter.Z))
                    if (dist < leftBall.r + dThick + dAtom  || MCenter.Z < 0.10 || double.NaN.Equals(MCenter.X) || double.NaN.Equals(MCenter.Y) || double.NaN.Equals(MCenter.Z))
                    {
                        /*
                        HCs.SW = new System.IO.StreamWriter("matrix" + id + ".txt", true);
                        HCs.SW.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8}", dist, 0, MCenter.Y, MCenter.Z,
                        0, pol.Q, Math.Acos(MAtomC.GetDirection() * E0.GetDirection()), mol.Etr, mol.Erot);
                        HCs.SW.Close();
                        */
                        break;
                    }
                    
                    if (MCenter.Z <= 20)
                    {
                        // parameters dump
                        HCs.SW = new StreamWriter("track0.txt", true);
                        //HCs.SW = new System.IO.StreamWriter("results.txt", true);
                        HCs.SW.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11}", time, tc, dist, MCenter.X, MCenter.Y, MCenter.Z, E0 * MAtomC.GetDirection(), E0.R, pol.Q, mol.Etr, mol.Erot,1/(((EC.R+EO.R)/2- Eprev)/Eprev/tc) );
                        times.Add(time);
                        fields.Add(E0 * MAtomC.GetDirection());
                        charges.Add(pol.Q);
                        HCs.SW.Close();
                    }
                    Eprev = (EO.R + EC.R) / 2;
                    
                } while (true);
                //----Spline
                int xFactor = 1000;
                // Create the data to be fitted
                double[] xs = new double[times.Count * xFactor];

                double[] x = new double[times.Count];
                double[] y = new double[times.Count];
                double[] c = new double[times.Count];

                times.CopyTo(x);
                fields.CopyTo(y);
                charges.CopyTo(c);

                y[0] = y[1];
                c[0] = c[1];

                // Compute the x values at which we will evaluate the spline.
                // Upsample the original data by a const factor.

                for (int i = 0; i < times.Count * xFactor; i++)
                    xs[i] = i * times[times.Count - 1] / (times.Count * xFactor - 1.0);

                double[] ys = CubicSpline.Compute(x, y, xs, 0.0d, Double.NaN);
                double[] cs = CubicSpline.Compute(x, c, xs, 0.0d, Double.NaN);

                //HCs.SW = new StreamWriter("track1.txt", true);
                //for (int i = 0; i < xs.Length; i++)
                //    HCs.SW.WriteLine("{0};{1};{2}", xs[i], ys[i], cs[i]);
                //HCs.SW.Close();
                
                LinePendulum pend = new LinePendulum();
                int step = 0;
                do
                {
                    pend.DoStepRK(xs[step + 1] - xs[step], ys[step], cs[step]);
                    HCs.SW = new StreamWriter("track2.txt", true);
                    HCs.SW.WriteLine("{0};{1};{2};{3};{4}", xs[step], ys[step], cs[step], pend.dx, pend.En);
                    HCs.SW.Close();
                    step++;
                }
                while (step < xs.Length - 2);
                return;
            } while (true);
        }
       
        static double GetTimeStep(double field, double energy)
        {
            /*
            const double mc = 8;
            double m = mc * Math.Sqrt(0.025 / energy);
            double t0 = Math.Log10(m*1E-14);
            double tm = Math.Log10(m*1E-19);
            double E0 = Math.Log10(1 / 2E8);
            double Em = Math.Log10(1 / 2E13);
            double logfn = Math.Log10(1 / field);
            logfn = Math.Max(Math.Min(logfn, E0), Em);
            double test2 = (logfn - E0) / (Em - E0) * (tm - t0);
            double time = Math.Pow(10, t0 + test2);
            */
            const double T0 = 1E-13;
            const double E0 = 2.18E8; // начальное поле
            const double alpha = 1;
            double result = T0 * Math.Pow(field / E0, -alpha);
            return result;
        }

        #region PlotSplineSolution

        private static void PlotSplineSolution(string title, double[] x, double[] y, double[] xs, double[] ys, string path, double[] qPrime = null)
        {
            var chart = new Chart
            {
                Size = new Size(600, 400)
            };
            chart.Titles.Add(title);
            chart.Legends.Add(new Legend("Legend"));

            ChartArea ca = new ChartArea("DefaultChartArea");
            ca.AxisX.Title = "X";
            ca.AxisY.Title = "Y";
            chart.ChartAreas.Add(ca);

            Series s1 = CreateSeries(chart, "Spline", CreateDataPoints(xs, ys), Color.Blue, MarkerStyle.None);
            Series s2 = CreateSeries(chart, "Original", CreateDataPoints(x, y), Color.Green, MarkerStyle.Diamond);

            chart.Series.Add(s2);
            chart.Series.Add(s1);

            if (qPrime != null)
            {
                Series s3 = CreateSeries(chart, "Slope", CreateDataPoints(xs, qPrime), Color.Red, MarkerStyle.None);
                chart.Series.Add(s3);
            }

            ca.RecalculateAxesScale();
            ca.AxisX.Minimum = Math.Floor(ca.AxisX.Minimum);
            ca.AxisX.Maximum = Math.Ceiling(ca.AxisX.Maximum);
            int nIntervals = (x.Length - 1);
            nIntervals = Math.Max(4, nIntervals);
            ca.AxisX.Interval = (ca.AxisX.Maximum - ca.AxisX.Minimum) / nIntervals;

            // Save
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (FileStream fs = new FileStream(path, FileMode.CreateNew))
            {
                chart.SaveImage(fs, ChartImageFormat.Png);
            }
        }

        private static List<DataPoint> CreateDataPoints(double[] x, double[] y)
        {
            Debug.Assert(x.Length == y.Length);
            List<DataPoint> points = new List<DataPoint>();

            for (int i = 0; i < x.Length; i++)
            {
                points.Add(new DataPoint(x[i], y[i]));
            }

            return points;
        }

        private static Series CreateSeries(Chart chart, string seriesName, IEnumerable<DataPoint> points, Color color, MarkerStyle markerStyle = MarkerStyle.None)
        {
            var s = new Series()
            {
                XValueType = ChartValueType.Double,
                YValueType = ChartValueType.Double,
                Legend = chart.Legends[0].Name,
                IsVisibleInLegend = true,
                ChartType = SeriesChartType.Line,
                Name = seriesName,
                ChartArea = chart.ChartAreas[0].Name,
                MarkerStyle = markerStyle,
                Color = color,
                MarkerSize = 8
            };

            foreach (var p in points)
            {
                s.Points.Add(p);
            }

            return s;
        }

        #endregion
    }
}
