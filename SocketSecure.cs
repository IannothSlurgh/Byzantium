
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
		IPEndPoint ep;
		Socket client;
        public TCPState(IPEndPoint _ep, ref Socket _client)
        {
            client = _client;
            ep = _ep;
        }
	}

	//For most communications with other nodes.
	//Use of socket because it fulfills both TcpListener and TcpClient roles.
	private Socket listen_socket;
	//The socket returned by a successful TCP connection.
	private Socket remote_socket; 
	//Primarily for broadcasts on open LAN.
	private UdpClient broadcaster;
	//Thread that blocks to handle synchronous listen_socket.
	private Thread tcp_thread;
	//Thread that blocks to handle synchronous udp.
	private Thread broadcast_thread;
	
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

	//this function is dedicated to synchronous tcp accepts.
	private void tcp_thread_protocol()
	{
		remote_socket = listen_socket.Accept();
		
	}
	
	private void broadcast_thread_protocol()
	{
	}
	
	static void accept_callback(IAsyncResult result)
	{
        Socket listener = (Socket)result.AsyncState;
        Socket work_socket = listener.EndAccept(result);
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
		//Length should be either 1 or 2 depending on OS. index 0 should be IPv4
		if(_my_ip.Length == 0)
		{
            node_ok = false;
			return;
		}
		my_ip = _my_ip[0];
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
		IPAddress broadcast_ip = new IPAddress(ip_bytes);
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
		TCPState callback_state_tcp = new TCPState(my_location, ref listen_socket);
		UDPState callback_state_udp = new UDPState(my_location, ref broadcaster);
        AsyncCallback accept = new AsyncCallback(accept_callback);
		listen_socket.BeginAccept(accept, callback_state_tcp);
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