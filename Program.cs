using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace Byzantium
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
            SortedAddressList sal = new SortedAddressList(10);
            byte[] ab = new byte[4];
            ab[3] = 92;
            byte[] bb = new byte[4];
            bb[3] = 153;
            byte[] cb = new byte[4];
            cb[3] = 120;
            IPAddress a = new IPAddress(ab);
            IPAddress b = new IPAddress(bb);
            IPAddress d = new IPAddress(cb);
            sal.add(a);
            sal.add(b);
            sal.add(d);
            Console.Out.WriteLine(sal[0].ToString());
            Console.Out.WriteLine(sal[1].ToString());
            Console.Out.WriteLine(sal[2].ToString());
            StringBuilder sb = new StringBuilder();
            Console.Out.WriteLine(sb.ToString());
            Console.Out.WriteLine("See");
            car c = test;
            texter(c);
            CommunicationNode ss = CommunicationNode.get_instance(12000, 12000);
            byte[] msg = Encoding.ASCII.GetBytes("Byzantine Signature");
            byte[] msg2 = Encoding.ASCII.GetBytes("Byzantine Signature");
            Console.Out.WriteLine(msg==msg2);
            for (int i = 0; i < 60; ++i)
            {
                ss.broadcast(msg);
            }
            Console.Out.WriteLine(IPAddress.Any);
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
