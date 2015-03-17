
using System;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.Remoting.Messaging; //For Asynch Callbacks.


class CommunicationNode
{
    //IPaddresses are byte arrays of len 4. This constant refers to
    //the index that contains the host number on this subnet.
    const int HOSTNUMBERINDEX = 3;

	class UDPState
	{
		IPEndPoint ep;
		UdpClient client;
        public UDPState(IPEndPoint _ep, ref UdpClient _client)
        {
            client = _client;
            ep = _ep;
        }
	}
	class TCPState
	{
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

    private TCPState tcp_data;
	private Socket listen_socket;
	private Socket remote_socket; 
	//Primarily for broadcasts on open LAN.
	private UdpClient broadcaster;
	
	//What port is TCP socket currently associated with?
	private int tcp_port;
	//What port will UDP broadcast client handle on?
	private int broadcast_port;
	//ip address of computer this is currently running on.
	private IPAddress my_ip;
	private IPAddress broadcast_ip;
	
	//No critical errors have made this node inoperative.
	private bool node_ok;

    private string err;
	
	static void tcp_accept_callback(IAsyncResult result)
	{
        TCPState state = (TCPState)result;
        Socket listener = state.listening_socket;
        state.work_socket = listener.EndAccept(result);
	}
	
	//Iniitialize both sockets.
	public CommunicationNode()
	{
		//Ipv4
		AddressFamily addr_fam = AddressFamily.InterNetwork;
		SocketType sock_type = SocketType.Stream;
		//Tcp
		ProtocolType proto_type = ProtocolType.Tcp;
		listen_socket = new Socket(addr_fam, sock_type, proto_type);
		//Get local ip address of computer this code is running on.
		IPAddress[] _my_ip = Dns.GetHostAddresses(string.Empty);
		//Length varies. We only care about ipv4 in this program.
		if(_my_ip.Length == 0)
		{
            node_ok = false;
			return;
		}
        //Pick the last possible IPv4 address available to this machine.
        foreach (IPAddress possible_ip in _my_ip)
        {
            if (possible_ip.AddressFamily == AddressFamily.InterNetwork)
            {
                my_ip = possible_ip;
            }
        }
		//my_ip = _my_ip[1];
		//User must specify ports for two sockets by assign_ports.
		tcp_port = 0;
		broadcast_port = 0;
		
		/*Section not in use- subnet mask not needed. Instead we broad cast to xxx.xxx.x.255*/
		
		//Look for my_ip in the computer's network configuration to obtain subnet mask.
		/*NetworkInterface [] all_adapters = NetworkInterface.GetAllNetworkInterfaces();
		for(uint i = 0; i < Length(all_adapters); ++i)
		{
			NetworkInterface adapter_particular = all_adapters[i];
			IPInterface ip_information = adapter_particular.GetIPProperties();
		}*/
		
		/*End section not in use*/
		
		//Broadcasts will be to all others on this subnet
		byte [] ip_bytes = my_ip.GetAddressBytes();
        ip_bytes[HOSTNUMBERINDEX] = 255;
        foreach (byte b in ip_bytes)
        {
            Console.Out.Write(b);
        }
		broadcast_ip = new IPAddress(ip_bytes);
		node_ok = true;
	}
	
	public CommunicationNode(int _tcp_port, int _broadcast_port) : this()
	{
		assign_ports(_tcp_port, _broadcast_port);
	}
	
	public void assign_ports(int _tcp_port, int _broadcast_port)
	{
		tcp_port = _tcp_port;
		broadcast_port = _broadcast_port;
		broadcaster = new UdpClient(broadcast_port, AddressFamily.InterNetwork);
	}
	
	//User can determine if errors have broken node.
	public bool node_is_ok()
	{
		return node_ok;
	}
	
	//
	public bool listen()
	{
		//Prepare both the UDP broadcast port and the TCP ports for messages.
		IPEndPoint my_location = new IPEndPoint(my_ip, tcp_port);
		listen_socket.Bind(my_location);
		listen_socket.Listen(10);
		//tcp_thread = new Thread(tcp_thread_protocol);
		//broadcast_thread = new Thread(broadcast_thread_protocol);
		//Two threads are sent off to block.
		//Thread.Start(tcp_thread);
		//Thread.Start(broadcast_thread);
        tcp_data = new TCPState(my_location, ref listen_socket);
		UDPState callback_state_udp = new UDPState(my_location, ref broadcaster);
        AsyncCallback accept = new AsyncCallback(tcp_accept_callback);
		listen_socket.BeginAccept(accept, tcp_data);
		//broadcaster.BeginReceive();
		return true;
	}
	
	/*public bool close_connection
	{
		
	}*/
	
	public bool connect(string target_address)
	{
		IPAddress remote_ip;
		try
		{
			remote_ip = IPAddress.Parse(target_address);
		}
		catch(System.FormatException e)
		{
            err = e.ToString();
			return false;
		}
		catch(System.ArgumentNullException e)
		{
            err = e.ToString();
			return false;
		}
		listen_socket.Connect(remote_ip, tcp_port);
		return true;
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
}