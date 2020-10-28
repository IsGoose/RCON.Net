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
        }

        public async Task<CommandResult> AttemptLogin(string password = "")
        {
            try
            {
                await _socket.ConnectAsync(_endpoint);
                if (!_socket.Connected)
                    return CommandResult.NotConnected;

                var loginBuffer = new byte[9];

                await SendPacketAsync(new Packet(++_packetId, PacketType.Login, 0, BattlEyeCommand.None, password));
                await _socket.ReceiveAsync(new ArraySegment<byte>(loginBuffer), SocketFlags.None);
                var loginResponse = new Packet(++_packetId, loginBuffer);
                if (loginResponse.PacketType == PacketType.Login && loginResponse.PayloadBytes[2] == 0x01)
                    return CommandResult.Success;
                else
                    return CommandResult.Failed;
            }
            catch
            {
                return CommandResult.Error;
            }
        }

        public void Setup()
        {
            var seArgs = new SocketAsyncEventArgs();
            seArgs.Completed += onRCONPacketReceived;
            seArgs.SetBuffer(new byte[MaxPacketSize], 0, MaxPacketSize);
            _socket.ReceiveAsync(seArgs);

        }

        public async Task SendPacketAsync(Packet p)
        {
            if (p.PacketId == null)
                p.PacketId = ++_packetId;
            await _socket.SendAsync(new ArraySegment<byte>(p.Assemble()), SocketFlags.None);
        }
        }
    }
}
