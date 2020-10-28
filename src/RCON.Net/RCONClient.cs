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

        private const int MaxPacketSize = 2048;

        private int _packetId = 0;

        public RCONClient(string host = "127.0.0.1",int port = 2303)
            : this(new IPEndPoint(IPAddress.Parse(host),port))
        {

        }

        public RCONClient(IPEndPoint endpoint) 
        {
            _endpoint = endpoint;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Connect(endpoint);
            var loginPacket = new Packet(0, PacketType.Login, 0, BattlEyeCommand.None, password);
            _socket.Send(loginPacket.Assemble());
        public async Task SendPacketAsync(Packet p)
        {
            if (p.PacketId == null)
                p.PacketId = ++_packetId;
            await _socket.SendAsync(new ArraySegment<byte>(p.Assemble()), SocketFlags.None);
        }
        }
    }
}
