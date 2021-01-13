using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        public event EventHandler OnPacketReceived;
        public event EventHandler OnServerMessageReceived;
        public event EventHandler OnCommandReceived;
        public event EventHandler OnAcknowledgeReceived;

        private IPEndPoint _endpoint;
        private Socket _socket;


        private MultiPacketBuffer MultiPacketBuffer = null;
        

        private const int MaxPacketSize = 2048;

        private int _packetId = 0;

        private DateTime _lastCommandSentTime = DateTime.MinValue;
        private Timer _timer;

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

        public async Task<CommandResult> AttemptLogin(string password = "",int loginTimeout = 10000)
        {
            try
            {
                await _socket.ConnectAsync(_endpoint);
                if (!_socket.Connected)
                    return CommandResult.NotConnected;

                var loginBuffer = new byte[9];
                await SendPacketAsync(new Packet(++_packetId, PacketType.Login, 0, BattlEyeCommand.None, password));
                var received = false;
                var timer = new Timer((obj) => { if (!received) _socket.Close(); }, null, loginTimeout, Timeout.Infinite);
                await _socket.ReceiveAsync(new ArraySegment<byte>(loginBuffer), SocketFlags.None);
                received = true;
                var loginResponse = new Packet(++_packetId, loginBuffer);
                if (loginResponse.PacketType == PacketType.Login && loginResponse.RawPayload[8] == 0x01)
                    return CommandResult.Success;
                else
                    return CommandResult.Failed;
            }
            catch
            {
                _socket.Dispose();
                return CommandResult.Error;
            }
        }

        public void Setup()
        {
            var seArgs = new SocketAsyncEventArgs();
            seArgs.Completed += onRCONPacketReceived;
            seArgs.SetBuffer(new byte[MaxPacketSize], 0, MaxPacketSize);
            _socket.ReceiveAsync(seArgs);

            _timer = new Timer(async x =>
            {
                var timeDiff = DateTime.Now - _lastCommandSentTime;
                if (timeDiff > TimeSpan.FromMilliseconds(HeartbeatInterval))
                    await SendPacketAsync(new Packet(null, PacketType.Command));
            
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

            if(OnPacketReceived != null)
            {
                onPacketReceived(packet);
                return;
            }
            


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

                ReceivedPackets.Add(packet);

                if (packet.PacketType == PacketType.ServerMessage)
                {
                        onServerMessageReceived(packet);
                }
                if (packet.PacketType == PacketType.Command)
                {
                        onCommandReceived(packet);
                }
                if (packet.PacketType == PacketType.Acknowledgement)
                {
                        onAcknowledgeReceived(packet);
                }

                //TODO?: Allow Users to assign custom event handlers for their needs

            }
            _socket.ReceiveAsync(e);
            

        }

        protected virtual void onPacketReceived(Packet p) => OnPacketReceived?.Invoke(this,new PacketEventArgs { PacketReceived = p, TimeReceived = DateTime.UtcNow });
        protected virtual void onServerMessageReceived(Packet p) => OnServerMessageReceived?.Invoke(this,new PacketEventArgs { PacketReceived = p, TimeReceived = DateTime.UtcNow });
        protected virtual void onCommandReceived(Packet p) => OnCommandReceived?.Invoke(this,new PacketEventArgs { PacketReceived = p, TimeReceived = DateTime.UtcNow });
        protected virtual void onAcknowledgeReceived(Packet p) => OnAcknowledgeReceived?.Invoke(this,new PacketEventArgs { PacketReceived = p, TimeReceived = DateTime.UtcNow });
    }
}
