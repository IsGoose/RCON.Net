using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCON.Net
{
    public class PacketEventArgs : EventArgs
    {
        public Packet PacketReceived { get; set; }
        public DateTime TimeReceived { get; set; }
    }
}
