using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RCON.Net
{
    public class RCONClient
    {
        private IPEndPoint _endpoint;
        private Socket _socket;

        private List<Packet> ReceivedPackets = new List<Packet>();
        private List<Packet> SentPackets = new List<Packet>();

        public RCONClient(string host = "127.0.0.1",int port = 2303,string password = "")
            : this(new IPEndPoint(IPAddress.Parse(host),port),password)
        {

        }

        public RCONClient(IPEndPoint endpoint,string password) 
        {
            _endpoint = endpoint;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Connect(endpoint);
            var loginPacket = new Packet(0, PacketType.Login, 0, BattlEyeCommand.None, "x");
            _socket.Send(loginPacket.Assemble());
        }
    }
}
