using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCON.Net
{
    public enum PacketType
    {
        Login,
        Command,
        ServerMessage,
        Acknowledgement
    }
}
