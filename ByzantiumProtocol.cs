﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Byzantium
{
    public class ByzantiumProtocol
    {
        SortedAddressList friendly;
        SortedAddressList pending;
        CommunicationNode my_node = null;
        Thread control_thread;
        public ByzantiumProtocol()
        {
            friendly = new SortedAddressList(100);
            pending = new SortedAddressList(100);
            control_thread = new Thread(control1);
        }

        ~ByzantiumProtocol()
        {
            if (control_thread.IsAlive)
            {
                control_thread.Abort();
            }
        }

        public void init()
        {
            WriteSettings();
            my_node = ReadSettings();
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
                    WriteSettings();
                    my_node = ReadSettings();
                }
                if (setting == 2)
                {
                    return;
                }
            }
            Console.Out.WriteLine(NodeErrors.getStringErr(my_node.getIntErr()));
            control_thread.Start();
            //Thread.Sleep(4000);
        }

        void WriteSettings()
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

        CommunicationNode ReadSettings()
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

        //Get all IPAddresses in a string starting from a colon, each delimited by a space.
        SortedAddressList GetQueryList(String s)
        {
            SortedAddressList data = new SortedAddressList(20);
            //Find index of colon. All data after should be IPAddresses.
            int colon_index = 0;
            for (int i = 0; i < s.Length; ++i)
            {
                if (s.ElementAt(i).Equals(':'))
                {
                    colon_index = i;
                    break;
                }
            }
            //Look for the index of the first address
            int address_index = 0;
            for (int i = colon_index+1; i < s.Length; ++i)
            {
                if (!s.ElementAt(i).Equals(" "))
                {
                    address_index = i;
                    break;
                }
            }
            //Go address by address adding to sorted list
            bool incomplete = true;
            while (incomplete)
            {
                //Find the space after the address.
                int delimiter_index = 0;
                for (int i = address_index; i < s.Length; ++i)
                {
                    if (s.ElementAt(i).Equals(" "))
                    {
                        delimiter_index = i;
                        break;
                    }
                }
                int num_char = delimiter_index - address_index -1;
                string addr = s.Substring(address_index, num_char);
                data.add(addr);
                //Find the next address
                for (int i = delimiter_index; i < s.Length; ++i)
                {
                    if (!s.ElementAt(i).Equals(" "))
                    {
                        address_index = i;
                        break;
                    }
                    //If we don't find another address, we are done.
                    if (i == s.Length - 1)
                    {
                        incomplete = false;
                    }
                }
            }
            return data;
        }

        //
        Message TCPStep(String send, String receive, IPAddress target)
        {
            my_node.send(Encoding.Default.GetBytes(send), target.GetAddressBytes());
            Thread.Sleep(3000);
            return my_node.searchTCP(receive);
        }

        //Look for a byzantium network to join unless danger is involved.
        void JoinNetwork()
        {
            if (pending.length() > 0)
            {
                //Go through each pending address, ask for a list of references.
                for (int i = 0; i < pending.length(); ++i )
                {
                    IPAddress current = pending[i];
                    Message ask_for_refs = TCPStep(ByzantiumStrings.who_are_your_friends, ByzantiumStrings.these_are_my_friends, current);
                    //If an address responds with a list of references, go ask each reference if it knows the pending address.
                    if (!ask_for_refs.is_bad)
                    {
                        SortedAddressList query_list = GetQueryList(ask_for_refs.msg);
                        StringBuilder question = new StringBuilder();
                        question.Append(ByzantiumStrings.do_you_know);
                        question.Append(" " + current);
                        bool refs_good = true;
                        foreach (IPAddress ip in query_list)
                        {
                            Message ask_acquaintance = TCPStep(question.ToString(), ByzantiumStrings.i_know, ip);
                            if (ask_acquaintance.is_bad)
                            {
                                Console.WriteLine(ip.ToString()+" doesn't know "+current.ToString());
                                refs_good = false;
                            }
                        }
                        if (refs_good)
                        {
                            friendly.add(current);
                        }
                    }
                    pending.remove(current);
                }
            }
        }

        void control1()
        {
            //Spend a time gathering data on alleged available clients
            for(int i = 0; i<10; ++i)
            {
                Console.Out.WriteLine(ByzantiumStrings.searching);
                my_node.broadcast(Encoding.ASCII.GetBytes(ByzantiumStrings.wanderer_without_home));
                my_node.listen_tcp();
                Console.Out.WriteLine(ByzantiumStrings.waiting_response);
                Thread.Sleep(1000);
                Message m = my_node.nextMessageTCP();
                if (m.is_bad)
                {
                    Console.Out.WriteLine("No Response.");
                }
                else
                {
                    Console.Out.WriteLine(m.msg);
                    if (m.msg.Contains(ByzantiumStrings.join_us))
                    {
                        pending.add(m.addr);
                    }
                }
            }

            JoinNetwork();

            while (true)
            {
                Console.Out.WriteLine(ByzantiumStrings.awaiting_new_clients);
                my_node.listen_broadcast();
                my_node.listen_tcp();
                Thread.Sleep(1000);
                Message m = my_node.nextMessageBroadcast();
                if (m.is_bad)
                {
                    Console.Out.WriteLine("No broadcasts...");
                }
                else
                {
                    Console.Out.WriteLine(m.msg);
                    if (m.msg.Contains(ByzantiumStrings.wanderer_without_home))
                    {
                        pending.add(m.addr);
                        my_node.send(Encoding.Default.GetBytes(ByzantiumStrings.join_us), m.addr);
                        Console.Out.WriteLine("Inviting "+Encoding.Default.GetString(m.addr)+"...");
                    }
                }
                m = my_node.nextMessageTCP();
                if (m.is_bad)
                {
                    Console.Out.WriteLine("No TCP messages...");
                }
                else
                {
                    if (m.msg.Contains(ByzantiumStrings.who_are_your_friends))
                    {
                        StringBuilder response = new StringBuilder();
                        response.Append(ByzantiumStrings.these_are_my_friends + " ");
                        foreach (IPAddress ip in friendly)
                        {
                            response.Append(ip);
                            response.Append(" ");
                        }
                        byte[] response_bytes = Encoding.Default.GetBytes(response.ToString());
                        my_node.send(response_bytes, m.addr);
                    }
                    if (m.msg.Contains(ByzantiumStrings.do_you_know))
                    {
                        SortedAddressList acquaintances = GetQueryList(m.msg);
                        StringBuilder response = new StringBuilder();
                        response.Append(ByzantiumStrings.i_know+" ");
                        foreach (IPAddress ip in acquaintances)
                        {
                            if (friendly.find(ip))
                            {
                                response.Append(ip + " ");
                            }
                        }
                        byte[] response_bytes = Encoding.Default.GetBytes(response.ToString());
                        my_node.send(response_bytes, m.addr);
                    }
                }
            }

        }

        void control()
        {
            while (true)
            {
                String signal = "Byzantine General " + System.Environment.MachineName;
                my_node.listen_broadcast();
                Console.Out.WriteLine("Waiting for broadcast...");
                Thread.Sleep(1000);
                Message most_recent = my_node.nextMessageBroadcast();
                if (!most_recent.is_bad)
                {
                    Console.Out.WriteLine("Broadcast received, establishing TCP connection with broadcaster.");
                    my_node.connect(Encoding.ASCII.GetString(most_recent.addr));
                    Console.Out.WriteLine("Connect");
                    my_node.send(Encoding.ASCII.GetBytes(signal));
                    //my_node.send(new byte[0]);
                    Console.Out.WriteLine(most_recent.msg);
                    Thread.Sleep(2000);
                }
                else
                {
                    my_node.listen_tcp();
                    my_node.broadcast(Encoding.ASCII.GetBytes(signal));
                    Console.Out.WriteLine("No broadcast, broadcasting and waiting for TCP connection.");
                    Thread.Sleep(3000);
                }
                most_recent = my_node.nextMessageTCP();
                if (!most_recent.is_bad)
                {
                    //Console.Out.WriteLine(most_recent.msg);
                    Console.Out.WriteLine("TCP message received from " + Encoding.Default.GetString(most_recent.addr) + ".");
                }
            }
        }

        public void terminate()
        {
            if (control_thread.IsAlive)
            {         
                control_thread.Abort();
            }
        }

    }
}
