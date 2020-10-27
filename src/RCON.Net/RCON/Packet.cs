﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCON.Net
{
    public class Packet
    {
        public int PacketId { get; set; }
        public byte[] CalculatedChecksum { get; set; }
        public byte[] ReceivedChecksum { get; set; }
        public PacketType PacketType { get; set; }
        public int SequenceNumber { get; set; }
        public byte[] PayloadBytes { get; set; }
        public bool IsPartialPacket { get; set; }
        

        public Packet(int packetId,byte[] rawPacket)
        {
            PacketId = packetId;

            var checksumBytes = rawPacket.Range(2, 4);

            PacketType = (PacketType)rawPacket[7];

            SequenceNumber = PacketType == PacketType.Login ? -1 : rawPacket[8];

            PayloadBytes = rawPacket.Range(6, rawPacket.Length - 6).ToArray();

            ReceivedChecksum = checksumBytes.ToArray();
            CalculatedChecksum = new CRC32().ComputeHash(PayloadBytes.Range(0,4).ToArray()).Reverse().ToArray();
        }

        public Packet(int packetId,PacketType type,int sequenceNum = 0,BattlEyeCommand command = BattlEyeCommand.None,string parameter = "")
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
        


    }
}