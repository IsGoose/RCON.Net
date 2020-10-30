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

        public string Hostname { get; set; }
        public int Port { get; set; }
        public int HeartbeatInterval { get; set; }

        public List<Packet> ReceivedPackets = new List<Packet>();
        public List<Packet> SentPackets = new List<Packet>();

        private IPEndPoint _endpoint;
        private Socket _socket;


        private MultiPacketBuffer MultiPacketBuffer = null;
        

        private const int MaxPacketSize = 2048;

        private int _packetId = 0;

        private DateTime _lastCommandSentTime = DateTime.MinValue;

        public RCONClient(string host = "127.0.0.1",int port = 2303,int heartbeatInterval = 30000)
            : this(new IPEndPoint(IPAddress.Parse(host),port), heartbeatInterval)
        {

        }

        public RCONClient(IPEndPoint endpoint,int heartbeatInterval) 
        {
            Hostname = endpoint.Address.ToString();
            Port = endpoint.Port;
            HeartbeatInterval = heartbeatInterval;

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
                if (loginResponse.PacketType == PacketType.Login && loginResponse.RawPayload[8] == 0x01)
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
            await _socket.SendAsync(new ArraySegment<byte>(p.RawPayload), SocketFlags.None);
            SentPackets.Add(p);
            _lastCommandSentTime = DateTime.Now;
        }



        private void onRCONPacketReceived(object sender, SocketAsyncEventArgs e)
        {
            var bytesReceived = e.Buffer;
            Array.Resize(ref bytesReceived, e.BytesTransferred);
            var packet = new Packet(null, bytesReceived);

            if(packet.CompareChecksums())
            {
                Task.Run(async () => await SendPacketAsync(new Packet(null, PacketType.ServerMessage, packet.SequenceNumber)));
                if (packet.IsPartialPacket)
                {
                    if (MultiPacketBuffer == null)
                        MultiPacketBuffer = new MultiPacketBuffer();
                    MultiPacketBuffer.Add(packet);
                    if (packet.RawPayload[10] == MultiPacketBuffer.PacketCount)
                    {
                        var combinedPacket = MultiPacketBuffer.CombinePackets();
                        combinedPacket.PacketId = ++_packetId;
                        MultiPacketBuffer = null;
                        packet = combinedPacket;
                    } else
                    {
                        _socket.ReceiveAsync(e);
                        return;
                    }
                }
                else
                {
                    packet.PacketId = ++_packetId;
                }

                //TODO: Fire EventHandlers Dependent on PacketType
                //TODO: Allow Users to assign custom event handlers for their needs

            } else
                Console.WriteLine($"Checksum Missmatch");
            _socket.ReceiveAsync(e);
            

        }
    }
}
