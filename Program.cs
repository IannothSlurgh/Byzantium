﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace safenetwork
{
    class car
    {
        public String p;
        //private get_p()
        public void set_p(string blah)
        {
            p = new String(blah.ToCharArray());
        }
        public car(ref String _p)
        {
            p = _p;
        }
    }

    class Program
    {
        public static String whatever = "XQMB";
        public static car test = new car(ref whatever);
        static void Main(string[] args)
        {
            car c = test;
            texter(c);
            CommunicationNode ss = new CommunicationNode();
            ss.assign_ports(12000, 12000);
            byte[] msg = Encoding.ASCII.GetBytes("Byzantine Signature");
            for (int i = 0; i < 60; ++i)
            {
                ss.broadcast(msg);
            }
            Thread.Sleep(2000);
        }
        static void texter(car c)
        {
            Console.Out.WriteLine(test.p);
            Console.Out.WriteLine(c.p);
            c.set_p("viral");
            Console.Out.WriteLine(c.p);
            Console.Out.WriteLine(test.p);
        }
    }
}