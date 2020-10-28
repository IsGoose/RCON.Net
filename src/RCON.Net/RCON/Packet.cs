using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCON.Net
{
    public class Packet
    {
        /// <summary>
        /// Packet Identification Internal Reference Only - BattlEye RCon Protocol does not specifiy PacketID
        /// </summary>
        public int? PacketId { get; set; }
        public byte[] CalculatedChecksum { get; set; }
        public byte[] ReceivedChecksum { get; set; }
        public PacketType PacketType { get; set; }
        public int SequenceNumber { get; set; }
        public byte[] PayloadBytes { get; set; }
        public byte[] RelevantPayloadBytes => PayloadBytes.Range(IsPartialPacket ? 6: 3, PayloadBytes.Length - (IsPartialPacket ? 6 : 3)).ToArray();
        public string PayloadAsString => Helpers.Bytes2String(PayloadBytes).Remove(0, IsPartialPacket ? 6 : 3);
        public bool IsPartialPacket { get; set; }
        

        public Packet(int? packetId,byte[] rawPacket)
        {
            PacketId = packetId;

            var checksumBytes = rawPacket.Range(2, 4);

            PacketType = (PacketType)rawPacket[7];

            SequenceNumber = PacketType == PacketType.Login ? -1 : rawPacket[8];

            IsPartialPacket = SequenceNumber != -1 && rawPacket[9] == 0x00;

            PayloadBytes = rawPacket.Range(6, rawPacket.Length - 6).ToArray();

            ReceivedChecksum = checksumBytes.ToArray();
            CalculatedChecksum = new CRC32().ComputeHash(PayloadBytes.Range(0,PayloadBytes.Length).ToArray()).Reverse().ToArray();
        }

        public Packet(int? packetId,PacketType type,int sequenceNum = 0,BattlEyeCommand command = BattlEyeCommand.None,string parameter = "")
        {
            PacketId = packetId;
            SequenceNumber = sequenceNum;
            PacketType = type;
            var subsequents = new List<byte>
            {
                0xFF,
                (byte)type
            };
            if (PacketType != PacketType.Login)
                subsequents.Add((byte)SequenceNumber);

            subsequents.AddRange(Helpers.String2Bytes(command.GetEnumDescription()));
            subsequents.AddRange(Helpers.String2Bytes(parameter));
            var checksum = new CRC32().ComputeHash(subsequents.ToArray()).Reverse().ToList();
            CalculatedChecksum = ReceivedChecksum = checksum.ToArray();
            checksum.InsertRange(0,new byte[] { 0x42, 0x45 });
            subsequents.InsertRange(0, checksum);

            PayloadBytes = subsequents.Range(6,subsequents.Count - 6).ToArray();
        }

        public byte[] Assemble()
        {
            var packet = new List<byte>
            {
                0x42,
                0x45,
                CalculatedChecksum[0],
                CalculatedChecksum[1],
                CalculatedChecksum[2],
                CalculatedChecksum[3]
            };
            packet.AddRange(PayloadBytes);

            return packet.ToArray();
        }
        
        public bool CompareChecksums()
        {
            if (ReceivedChecksum.Length != 4 || CalculatedChecksum.Length != 4)
                return false;
            for (int i = 0; i < 4; i++)
                if (ReceivedChecksum[i] != CalculatedChecksum[i])
                    return false;
            return true;
        }


    }
}
