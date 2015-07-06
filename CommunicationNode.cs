
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading;
using System.Runtime.Remoting.Messaging; //For Asynch Callbacks.
using System.Text;
using System.Collections.Generic;
using Byzantium;

//A singleton class responsible for both tcp and udp broadcast communication
//In our distributed application.
class CommunicationNode
{
    //IPaddresses are byte arrays of len 4. This constant refers to
    //the index that contains the host number on this subnet.
    const int HOSTNUMBERINDEX = 3;

    private static CommunicationNode singleton_instance = null;

	class UDPState
	{
        public IPEndPoint ep;
		public UdpClient client;
        public UDPState(IPEndPoint _ep, ref UdpClient _client)
        {
            client = _client;
            ep = _ep;
        }
	}
	class TCPState
	{
        public byte[] buffer = new byte[1024];
        public StringBuilder message_concat = new StringBuilder();
        public IPEndPoint ep;
        //For most communications with other nodes.
        //Use of socket because it fulfills both TcpListener and TcpClient roles.
        public Socket listening_socket;
        //The socket returned by a successful TCP connection.
        public Socket work_socket;
        public TCPState(IPEndPoint _ep, ref Socket _listening_socket)
        {
            listening_socket = _listening_socket;
            ep = _ep;
            work_socket = null;
        }
	}

    private static MessageQueue broadcast_messages;
    private static MessageQueue tcp_messages;
    private static TCPState tcp_data;
    private static UDPState udp_data;

	private static Socket listen_socket;
	private static Socket connect_socket; 
	//Primarily for broadcasts on open LAN.
	private static UdpClient broadcaster = null;
	
	//What port is TCP socket currently associated with?
	private static int tcp_port;
	//What port will UDP broadcast client handle on?
	private static int broadcast_port;
	//ip address of computer this is currently running on.
	private static IPAddress my_ip;
	private static IPAddress broadcast_ip;

    //In the case there are multiple options for host addresses a person can use, we store the adaptor
    //descriptions and the associated ips. As a result, we can let the user ask for a list of choices
    //and tell the node which choice we want.
    private static List<String> possible_my_ip_descriptions;
    private static List<IPAddress> possible_my_ip;

	//No critical errors have made this node inoperative.
	private static bool node_ok;

    private static string str_err;
    private static int int_err;

    private static void tcp_receive_callback(IAsyncResult result)
    {
        TCPState state = (TCPState)result.AsyncState;
        //Console.Out.WriteLine(state.work_socket.RemoteEndPoint);
        int num_received = state.work_socket.EndReceive(result);
        //0 means the sending socket closed. The message was completed.
        if (num_received == 0)
        {
            Message m = new Message();
            string addr_with_port = state.work_socket.RemoteEndPoint.ToString();
            int colon_pos = addr_with_port.IndexOf(":");
            addr_with_port = addr_with_port.Substring(0, colon_pos);
            m.addr = Encoding.Default.GetBytes(addr_with_port);
            m.msg = state.message_concat.ToString();
            m.proto = "TCP";
            //mq prevents empty messages in the case a client connects
            //and subsequently closes without sending any data.
            tcp_messages.Enqueue(m);
        }
        else
        {
            //We are still eagerly awaiting what else the sender may have to say.
            state.message_concat.Append(Encoding.ASCII.GetString(state.buffer, 0, num_received));
            state.work_socket.BeginReceive(state.buffer, 0, 1024, 0, tcp_receive_callback, state);
        }
    }

	private static void tcp_accept_callback(IAsyncResult result)
	{
        TCPState state = (TCPState)result.AsyncState;
        Socket listener = state.listening_socket;
        state.work_socket = listener.EndAccept(result);
        state.work_socket.BeginReceive(state.buffer, 0, 1024, 0, tcp_receive_callback, state);
	}

    private static void udp_receive_callback(IAsyncResult result)
    {
        UDPState state = (UDPState)result.AsyncState;
        UdpClient listener = state.client;
        Byte[] broadcasted_msg = listener.EndReceive(result, ref state.ep);
        listener.BeginReceive(udp_receive_callback, state);
        Message received = new Message();
        string preprocessed_addr = state.ep.ToString();
        int colon_pos = preprocessed_addr.IndexOf(":");
        preprocessed_addr = preprocessed_addr.Substring(0, colon_pos);
        received.addr = System.Text.Encoding.ASCII.GetBytes(preprocessed_addr);
        received.msg = System.Text.Encoding.ASCII.GetString(broadcasted_msg);
        received.proto = "UDP Broadcast";
        //Console.Out.WriteLine(Encoding.ASCII.GetString(broadcasted_msg));
        //We don't want our message queue to store things we said.
        string my_ip_with_port = my_ip.ToString();
        if (!Encoding.Default.GetString(received.addr).Equals(my_ip_with_port))
        {
            broadcast_messages.Enqueue(received);
        }
    }

    //Make sure to pass this callback the connect_socket.
    private static void tcp_send_callback(IAsyncResult result)
    {
        Socket connect_socket = (Socket)result.AsyncState;
        int bytes_sent = connect_socket.EndSend(result);
        connect_socket.Shutdown(SocketShutdown.Both);
        connect_socket.Disconnect(false);
        connect_socket.Close();
    }

    public void shutdown()
    {
        if (udp_data != null)
        {
            if (udp_data.client != null)
            {
                udp_data.client.Close();
            }
        }
        if (tcp_data != null)
        {

            if (tcp_data.listening_socket != null)
            {
                if (tcp_data.listening_socket.Connected)
                {
                    tcp_data.listening_socket.Shutdown(SocketShutdown.Both);
                    tcp_data.listening_socket.Disconnect(false);
                }
                tcp_data.listening_socket.Close();
            }
            if (tcp_data.work_socket != null)
            {
                if (tcp_data.work_socket.Connected)
                {
                    tcp_data.work_socket.Shutdown(SocketShutdown.Both);
                    tcp_data.work_socket.Disconnect(false);
                }
                tcp_data.work_socket.Close();
            }
            if (connect_socket != null)
            {
                if (connect_socket.Connected)
                {
                    connect_socket.Shutdown(SocketShutdown.Both);
                    connect_socket.Disconnect(false);
                }
                connect_socket.Close();
            }
        }
    }

    ~CommunicationNode()
    {
        shutdown();
    }

	//Iniitialize both sockets.
	private CommunicationNode()
	{
		//Ipv4
		AddressFamily addr_fam = AddressFamily.InterNetwork;
		SocketType sock_type = SocketType.Stream;
		//Tcp
		ProtocolType proto_type = ProtocolType.Tcp;
		listen_socket = new Socket(addr_fam, sock_type, proto_type);
        connect_socket = new Socket(addr_fam, sock_type, proto_type);
		//Get local ip address of computer this code is running on.
        NetworkInterface[] all_adaptors = NetworkInterface.GetAllNetworkInterfaces();
		//Length varies. We only care about ipv4 in this program.
        possible_my_ip = new List<IPAddress>();
        possible_my_ip_descriptions = new List<String>();
        foreach (NetworkInterface adaptor in all_adaptors)
        {
            if (adaptor.NetworkInterfaceType == NetworkInterfaceType.Ethernet || adaptor.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            {
                foreach (UnicastIPAddressInformation uip in adaptor.GetIPProperties().UnicastAddresses)
                {
                    if (uip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        possible_my_ip.Add(uip.Address);
                        possible_my_ip_descriptions.Add(adaptor.Description);
                    }
                }
            }
        }
        if (possible_my_ip.Count == 0)
        {
            node_ok = false;
            str_err = "No Network Adapators";
            int_err |= NodeErrors.NO_NETWORK_ADAPTORS;
            return;
        }
        if (possible_my_ip.Count > 1)
        {
            node_ok = false;
            str_err = "Multiple Host Addresses- Choose One";
            int_err |= NodeErrors.NETWORK_ADAPTOR_UNCHOSEN;
        }
        my_ip = possible_my_ip[0];
		//User must specify ports for two sockets by assign_ports.
		tcp_port = 0;
		broadcast_port = 0;
        str_err = "Ports Unchosen- Choose Port Numbers";
        int_err |= NodeErrors.PORTS_UNCHOSEN;
		//Broadcasts will be to all others on this subnet
		byte [] ip_bytes = my_ip.GetAddressBytes();
        ip_bytes[HOSTNUMBERINDEX] = 255;
		broadcast_ip = new IPAddress(ip_bytes);
		node_ok = true;
        broadcast_messages = new MessageQueue(100);
        tcp_messages = new MessageQueue(100);
	}
	
    private void checkNode()
    {
        if(int_err == 0)
        {
            node_ok = true;
        }
    }

    public int getIntErr()
    {
        return int_err;
    }

	private CommunicationNode(int _tcp_port, int _broadcast_port) : this()
	{
		assign_ports(_tcp_port, _broadcast_port);
	}
	
	public void assign_ports(int _tcp_port, int _broadcast_port)
	{
        if (broadcaster != null)
        {
            broadcaster.Close();
        }
		tcp_port = _tcp_port;
		broadcast_port = _broadcast_port;
		broadcaster = new UdpClient(broadcast_port, AddressFamily.InterNetwork);
        if ( NodeErrors.hasErr(int_err, NodeErrors.PORTS_UNCHOSEN) )
        {
            int_err = NodeErrors.toggleErr(int_err, NodeErrors.PORTS_UNCHOSEN);
            str_err = "N/A";
            checkNode();
        }
	}

    public List<String> getPossibleAdaptorDescriptions()
    {
        return new List<String>(possible_my_ip_descriptions);
    }

    public List<IPAddress> getPossibleHostAddresses()
    {
        return new List<IPAddress>(possible_my_ip);
    }

    public bool assignAdaptor(String name)
    {
        for (int i = 0; i < possible_my_ip_descriptions.Count; ++i)
        {
            String s = possible_my_ip_descriptions[i];
            if (s.Equals(name))
            {
                my_ip = possible_my_ip[i];
                byte[] ip_bytes = my_ip.GetAddressBytes();
                ip_bytes[HOSTNUMBERINDEX] = 255;
                broadcast_ip = new IPAddress(ip_bytes);
                if (NodeErrors.hasErr(int_err, NodeErrors.NETWORK_ADAPTOR_UNCHOSEN))
                {
                    int_err = NodeErrors.toggleErr(int_err, NodeErrors.NETWORK_ADAPTOR_UNCHOSEN);
                    checkNode();
                }
                return true;
            }
        }
        return false;
    }

    public Message nextMessageBroadcast()
    {
        return broadcast_messages.Dequeue();
    }

    public Message nextMessageTCP()
    {
        return tcp_messages.Dequeue();
    }

    public Message searchBroadcast(String contents)
    {
        int size = broadcast_messages.Length();
        for (int i = 0; i < size; ++i)
        {
            Message current = broadcast_messages.Dequeue();
            if (current.msg.Contains(contents))
            {
                return current;
            }
            else
            {
                broadcast_messages.Enqueue(current);
            }
        }
        return broadcast_messages.CreateBadMessage();
    }

    public Message searchTCP(String contents)
    {
        int size = tcp_messages.Length();
        for (int i = 0; i < size; ++i)
        {
            Message current = tcp_messages.Dequeue();
            if (current.msg.Contains(contents))
            {
                return current;
            }
            else
            {
                tcp_messages.Enqueue(current);
            }
        }
        return tcp_messages.CreateBadMessage();
    }

    /*public Message nextMessage()
    {
        return message_queue.Dequeue();
    }*/

	//User can determine if errors have broken node.
	public bool node_is_ok()
	{
		return int_err == 0;
	}
	
	//
	public bool listen_broadcast()
	{
		//Prepare both the UDP broadcast port and the TCP ports for messages.
		IPEndPoint my_location = new IPEndPoint(my_ip, broadcast_port);
		udp_data = new UDPState(my_location, ref broadcaster);
        broadcaster.BeginReceive( new AsyncCallback(udp_receive_callback), udp_data );
		return true;
	}

    public bool listen_tcp()
    {
        IPEndPoint my_location = new IPEndPoint(my_ip, tcp_port);
        if (!listen_socket.IsBound)
        {
            listen_socket.Bind(my_location);
            listen_socket.Listen(10);
        }
        tcp_data = new TCPState(my_location, ref listen_socket);
        AsyncCallback tcp_accept = new AsyncCallback(tcp_accept_callback);
        listen_socket.BeginAccept(tcp_accept, tcp_data);
        return true;
    }
	
	/*public bool close_connection
	{
		
	}*/
	
	public bool connect(string target_address)
	{
        if (target_address.Contains(":"))
        {
            int colon_pos = target_address.IndexOf(":");
            target_address = target_address.Substring(0, colon_pos);
        }
		IPAddress remote_ip;
		try
		{
			remote_ip = IPAddress.Parse(target_address);
		}
		catch(System.FormatException e)
		{
            str_err = e.ToString();
			return false;
		}
		catch(System.ArgumentNullException e)
		{
            str_err = e.ToString();
			return false;
		}
        if (!connect_socket.Connected)
        {
            AddressFamily addr_fam = AddressFamily.InterNetwork;
            SocketType sock_type = SocketType.Stream;
            ProtocolType proto_type = ProtocolType.Tcp;
            connect_socket = new Socket(addr_fam, sock_type, proto_type);
            connect_socket.Connect(remote_ip, tcp_port);
            return true;
        }
        return false;
	}
	
	//Send a broadcast on the UdpClient to all clients on same subnet.
	public bool broadcast(byte[] dgram)
	{
		//Ports have not been assigned, failure inevitable.
		if(broadcast_port == 0 || tcp_port == 0)
		{
			return false;
		}
		IPEndPoint broadcast_endpoint = new IPEndPoint(broadcast_ip, broadcast_port);
		broadcaster.Send(dgram, dgram.Length, broadcast_endpoint);
		return true;
	}

    //Talk to a specific ipaddress, and give it a message.
    public bool send(byte[] data, byte[] addr)
    {
        //Ports have not been assigned, failure inevitable.
        if (broadcast_port == 0 || tcp_port == 0)
        {
            return false;
        }

        bool connected = connect(Encoding.Default.GetString(addr));
        if (!connected)
        {
            return false;
        }
        return send(data);
    }

    public bool send(byte[] data)
    {
        if (connect_socket.Connected)
        {
            connect_socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(tcp_send_callback), connect_socket);
        }
        else
        {
            Console.Out.WriteLine("No Connection");
            return false;
        }
        return true;
    }

    public bool send(byte[] data, IPAddress addr)
    {
        return send(data, addr.GetAddressBytes());
    }

    public static CommunicationNode get_instance()
    {
        if (singleton_instance == null)
            singleton_instance = new CommunicationNode();
        return singleton_instance;
    }
    public static CommunicationNode get_instance(int _tcp_port, int _broadcast_port)
    {
        if (singleton_instance == null)
            singleton_instance = new CommunicationNode();
        singleton_instance.assign_ports(_tcp_port, _broadcast_port);
        return singleton_instance;
    }

    public IPAddress getMyIP()
    {
        return my_ip;
    }
}