using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCON.Net
{
    public class Packet
    {
        public int? PacketId { get; set; }
        public PacketType PacketType { get; set; }
        public int SequenceNumber { get; set; }
        public string AsRelevantString => Helpers.Bytes2String(RawPayload.Range(IsPartialPacket ? 12 : 9,RawPayload.Length - (IsPartialPacket ? 12 : 9)));
        public byte[] RawPayload { get; set; }
        public bool IsPartialPacket { get; set; }
        public bool WasPartialPacket { get; set; } = false;

        private byte[] _calculatedChecksum { get; set; }
        private byte[] _receivedChecksum { get; set; }


        public Packet(int? packetId,byte[] rawPacket)
        {
            try
            {
                PacketId = packetId;

                RawPayload = rawPacket;

                var checksumBytes = rawPacket.Range(2, 4);

                PacketType = (PacketType)rawPacket[7];

                SequenceNumber = PacketType == PacketType.Login ? -1 : rawPacket[8];


                byte[] _payloadBytes = rawPacket.Range(6, rawPacket.Length - 6).ToArray();

                _receivedChecksum = checksumBytes.ToArray();
                _calculatedChecksum = new CRC32().ComputeHash(_payloadBytes.Range(0, _payloadBytes.Length).ToArray()).Reverse().ToArray();

                if(rawPacket.Length > 9)
                    IsPartialPacket = SequenceNumber != -1 && rawPacket[9] == 0x00;
                else
                {
                    IsPartialPacket = false;
                    PacketType = PacketType == PacketType.Login ? PacketType.Login : PacketType.Acknowledgement;
                }
            }
            catch
            {

            }
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
            _calculatedChecksum = _receivedChecksum = checksum.ToArray();
            checksum.InsertRange(0,new byte[] { 0x42, 0x45 });
            subsequents.InsertRange(0, checksum);

            RawPayload = subsequents.ToArray();
        }
        
        public bool CompareChecksums()
        {
            if (_receivedChecksum.Length != 4 || _calculatedChecksum.Length != 4)
                return false;
            for (int i = 0; i < 4; i++)
                if (_receivedChecksum[i] != _calculatedChecksum[i])
                    return false;
            return true;
        }


    }
}
