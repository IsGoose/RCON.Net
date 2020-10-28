using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCON.Net
{
    public class MultiPacketBuffer
    {
        private List<Packet> _packets = new List<Packet>();


        public MultiPacketBuffer()
        {

        }

        public Packet CombinePackets()
        {
            var newBuffer = new List<byte>() { 0x42, 0x45, 0x00, 0x00, 0x00, 0x00, 0xFF,(byte)_packets[0].PacketType,0};
            foreach (var packet in _packets)
                newBuffer.AddRange(Helpers.String2Bytes(packet.PayloadAsString));
            return new Packet(null, newBuffer.ToArray());
        }

        public void Add(Packet p) => _packets.Add(p);
    }
}
