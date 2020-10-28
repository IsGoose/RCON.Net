using RCON.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () => await MainAsync(args)).GetAwaiter().GetResult();
        }
        static async Task MainAsync(string[] args)
        {
            var client = new RCONClient("127.0.0.1", 2302);
            var result = await client.AttemptLogin("x");

            if (result == CommandResult.Success)
                 client.Setup();

            while (true)
                await client.SendPacketAsync(new Packet(null, PacketType.Command, 0, BattlEyeCommand.None, Console.ReadLine()));


        }
    }
}
