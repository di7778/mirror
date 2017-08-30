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
            plane.dom = Domains.Substrate;

            Ball leftBall = new Ball();
            leftBall.r = 1;
            leftBall.p = new Point(-1.1, 0, 1.1);
            leftBall.dom = Domains.LeftBall;

            Ball rightBall = new Ball();
            rightBall.r = 1;
            rightBall.p = new Point(1.1, 0, 1.1);
            rightBall.dom = Domains.RightBall;


            Point MCenter = new Point(0, 0, 2);

            Vector MAtomC = new Vector(0.11, new Direction(Math.PI / 2, 0)); // начальное направление
            

            Vector E = new Vector(0, 0, 0);
            Polarization pol = new Polarization(E, MAtomC);

            for (int i = 0; i < 10; i++)
            {
                Charge q1 = new Charge();
                q1.q = -pol.Q;
                q1.p = MCenter - pol.Pol;
                q1.dom = Domains.Undef;

                Charge q2 = new Charge();
                q2.q = +pol.Q;
                q2.p = MCenter + pol.Pol;
                q2.dom = Domains.Undef;

                Charge q3 = new Charge();
                q3.q = +1;
                q3.p = new Point(-1.1, 0, 1.1);
                q3.dom = Domains.LeftBall;

                Charge q4 = new Charge();
                q4.q = -1;
                q4.p = new Point(+1.1, 0, 1.1);
                q4.dom = Domains.RightBall;
                
                Queue<Charge> queue = new Queue<Charge>();
                Queue<Charge> mqueue = new Queue<Charge>();
                queue.Enqueue(q1);
                queue.Enqueue(q2);
                queue.Enqueue(q3);
                queue.Enqueue(q4);


                int maxcounter = 4;


                Point p = new Point(0, 0, 2);

                int curstage = 0;
                int maxstage = 11;

                while (curstage < maxstage)
                {
                    int gencounter = 0;
                    int curcounter = 0;
                    while (curcounter < maxcounter)
                    {
                        Charge q = queue.Dequeue();
                        if (q.dom != Domains.Substrate)
                        {
                            queue.Enqueue(plane.Mirror(q));
                            gencounter++;
                        }
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

                mqueue.Dequeue();
                mqueue.Dequeue();

                while (mqueue.Count > 0)
                {
                    Charge q = mqueue.Dequeue();
                    E += q.ElField(p);
                }

                pol = new Polarization(E, MAtomC);
            }
        }
    }
}
