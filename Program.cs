using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace Byzantium
{
    class Program
    {
        static void Main(string[] args)
        {
            if (System.Environment.MachineName.Equals("REDACTED1"))
            {
                CommunicationNode ss = CommunicationNode.get_instance(12000, 12000);
                List<String> adaptors = ss.getPossibleAdaptorDescriptions();
                foreach (String s in adaptors)
                {
                    Console.Out.WriteLine(s);
                }
                ss.assignAdaptor("NVIDIA nForce 10/100 Mbps Ethernet");
                byte[] msg = Encoding.ASCII.GetBytes("Byzantine Signature");
                Console.Out.WriteLine(ss.node_is_ok());
                for (int i = 0; i < 60; ++i)
                {
                    ss.broadcast(msg);
                }
            }
            if (System.Environment.MachineName.Equals("REDACTED2"))
            {
                CommunicationNode ss = CommunicationNode.get_instance(12000, 12000);
                List<String> adaptors = ss.getPossibleAdaptorDescriptions();
                String chosen = "";
                foreach (String s in adaptors)
                {
                    Console.Out.WriteLine(s);
                    if (s.Contains("Broadcom"))
                    {
                        chosen = s;
                    }
                }
                ss.assignAdaptor(chosen);
                Console.Out.WriteLine(ss.node_is_ok());
                ss.listen();
            }
            //Console.Out.WriteLine(IPAddress.Any);
            Thread.Sleep(2000);
        }
    }
}
