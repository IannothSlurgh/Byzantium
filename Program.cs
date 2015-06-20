using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.IO;

namespace Byzantium
{
    class Program
    {
        static void Main(string[] args)
        {
            one_code();
            /*if (System.Environment.MachineName.Equals("Redacted1"))
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
            if (System.Environment.MachineName.Equals("Redacted2"))
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
            //Console.Out.WriteLine(IPAddress.Any);*/
            Thread.Sleep(4000);
        }
        static void first_time()
        {
            if (!File.Exists("ByzantiumSettings.dat"))
            {
                FileStream new_F = File.Create("ByzantiumSettings.dat");
                Console.Out.WriteLine("Byzantium running for the first time.");
                Console.Out.WriteLine("Please input the port number on which Byzantium should send broadcasts.");
                Console.Out.WriteLine("To use the default of 12000, type default.");
                bool awaiting_int = true;
                int udp_port = 12000;
                while (awaiting_int)
                {
                    string user_input = Console.In.ReadLine();
                    if (int.TryParse(user_input, out udp_port))
                    {
                        awaiting_int = false;
                    }
                    if (user_input.Contains("default"))
                    {
                        udp_port = 12000;
                        awaiting_int = false;
                    }
                }
                Console.Out.WriteLine("Please input the port number on which Byzantium should send TCP communications.");
                Console.Out.WriteLine("To use the default of 12000, type default.");
                awaiting_int = true;
                int tcp_port = 12000;
                while (awaiting_int)
                {
                    string user_input = Console.In.ReadLine();
                    if (int.TryParse(user_input, out tcp_port))
                    {
                        awaiting_int = false;
                    }
                    if (user_input.Contains("default"))
                    {
                        tcp_port = 12000;
                        awaiting_int = false;
                    }
                }
                CommunicationNode my_node = CommunicationNode.get_instance(tcp_port, udp_port);
                List<String> adaptors = my_node.getPossibleAdaptorDescriptions();
                Console.Out.WriteLine("Here are a list of adaptors on which Byzantium may operate. Choose one.");
                for (int i = 0; i < adaptors.Count; ++i)
                {
                    String s = adaptors[i];
                    Console.Out.WriteLine(i + ") " + s);
                }
                awaiting_int = true;
                int adaptor_choice = 0;
                while (awaiting_int)
                {
                    string user_input = Console.In.ReadLine();
                    if (int.TryParse(user_input, out adaptor_choice))
                    {
                        if (adaptor_choice < adaptors.Count)
                        {
                            awaiting_int = false;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < adaptors.Count; ++i)
                        {
                            String s = adaptors[i];
                            if (user_input.Contains(s))
                            {
                                adaptor_choice = i;
                                awaiting_int = false;
                            }
                        }
                    }
                }
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] line = encoding.GetBytes("UDP PORT\n");
                new_F.Write(line, 0, line.Length);

                line = encoding.GetBytes(udp_port + "\n");
                new_F.Write(line, 0, line.Length);

                line = encoding.GetBytes("TCP PORT\n");
                new_F.Write(line, 0, line.Length);

                line = encoding.GetBytes(tcp_port + "\n");
                new_F.Write(line, 0, line.Length);

                line = encoding.GetBytes("ADAPTOR\n");
                new_F.Write(line, 0, line.Length);

                line = encoding.GetBytes(adaptors[adaptor_choice] + "\n");
                new_F.Write(line, 0, line.Length);

                new_F.Close();
            }
        }

        static CommunicationNode second_time()
        {
            StreamReader setting_reader = new StreamReader("ByzantiumSettings.dat");
            string text = "";
            bool read_udp_port = false;
            bool read_tcp_port = false;
            bool read_adaptor = false;
            int udp_port = 12000;
            int tcp_port = 12000;
            string adaptor = "";
            bool bad_file = false;
            //ASCIIEncoding ascii = new ASCIIEncoding();
            while (text != null)
            {
                //Console.Out.WriteLine(text);
                text = setting_reader.ReadLine();
                if (text == null)
                {
                    break;
                }
                if (read_udp_port)
                {
                    if (!int.TryParse(text, out udp_port))
                    {
                        bad_file = true;
                    }
                    read_udp_port = false;
                }
                if (read_tcp_port)
                {
                    if (!int.TryParse(text, out tcp_port))
                    {
                        bad_file = true;
                    }
                    read_tcp_port = false;
                }
                if (read_adaptor)
                {
                    adaptor = text;
                    read_adaptor = false;
                }
                if (text.Contains("UDP_PORT"))
                {
                    read_udp_port = true;
                }
                if (text.Contains("TCP_PORT"))
                {
                    read_tcp_port = true;
                }
                if (text.Contains("ADAPTOR"))
                {
                    read_adaptor = true;
                }
            }
            setting_reader.Close();
            Console.Out.WriteLine(udp_port);
            Console.Out.WriteLine(tcp_port);
            Console.Out.WriteLine(adaptor);
            if(!bad_file)
            {
                CommunicationNode my_node = CommunicationNode.get_instance(udp_port, tcp_port);
                my_node.assignAdaptor(adaptor);
                if (!my_node.node_is_ok())
                {
                    my_node = null;
                }
                return my_node;
            }
            return null;
        }

        static void one_code()
        {
            first_time();
            CommunicationNode my_node = second_time();
            while (my_node == null)
            {
                Console.Out.WriteLine("Initialization of the communication node failed.");
                Console.Out.WriteLine("Would you like to try to update its settings?");
                Console.Out.WriteLine("1)Yes");
                Console.Out.WriteLine("2)No");
                int setting = 0;
                string user_input = Console.In.ReadLine();
                if (!int.TryParse(user_input, out setting))
                {
                    if (user_input.Contains("y") || user_input.Contains("Y"))
                    {
                        setting = 1;
                    }
                    else
                    {
                        if (user_input.Contains("n") || user_input.Contains("N"))
                        {
                            setting = 2;
                        }
                    }
                }
                if (setting == 1)
                {
                    if (File.Exists("ByzantiumSettings.dat"))
                    {
                        File.Delete("ByzantiumSettings.dat");
                    }
                    first_time();
                    my_node = second_time();
                }
                if (setting == 2)
                {
                    return;
                }
            }
            Console.Out.WriteLine(NodeErrors.getStringErr(my_node.getIntErr()));
            control(my_node);
        }

        static void control(CommunicationNode my_node)
        {
            String signal = "Byzantine General " + System.Environment.MachineName;
            my_node.listen_broadcast();
            Console.Out.WriteLine("Waiting for broadcast...");
            Thread.Sleep(10000);
            Message most_recent = my_node.nextMessage();
            if (!most_recent.is_bad)
            {
                my_node.connect(Encoding.ASCII.GetString(most_recent.addr));
                my_node.send(Encoding.ASCII.GetBytes(signal));
                Console.Out.WriteLine(most_recent.msg);
                Console.Out.WriteLine("Broadcast received, establishing TCP connection with broadcaster.");
            }
            else
            {
                my_node.listen_tcp();
                my_node.broadcast(Encoding.ASCII.GetBytes(signal));
                Console.Out.WriteLine("No broadcast, broadcasting and waiting for TCP connection.");
            }
            Thread.Sleep(10000);
            most_recent = my_node.nextMessage();
            if (!most_recent.is_bad)
            {
                Console.Out.WriteLine(most_recent.msg);
                Console.Out.WriteLine("TCP message received.");
            }
        }

    }
}
