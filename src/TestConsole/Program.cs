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
            var client = new RCONClient("127.0.0.1", 2302,5000);
            client.OnServerMessageReceived += (sender, e) => {
                Console.WriteLine("[Server Message]: " + (e as PacketEventArgs).PacketReceived.AsRelevantString);
            };   
            client.OnCommandReceived += (sender, e) => {
                Console.WriteLine("[Command]: " + (e as PacketEventArgs).PacketReceived.AsRelevantString);
            };    
            client.OnAcknowledgeReceived += (sender, e) => {
                Console.WriteLine("[Server Acknowledged]");
            };
            var result = await client.AttemptLogin("x");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            switch(result)
            {
                case CommandResult.NotConnected:
                    {
                        Console.WriteLine("[LOGIN] Socket Connection Unsucessful");
                        return;
                    }
                case CommandResult.Failed :
                    {
                        Console.WriteLine("[LOGIN] RCON Login Attempt Failed (Incorrect Password?)");
                        return;
                    }
                case CommandResult.Error:
                    {
                        Console.WriteLine("[LOGIN] An Unknown Error Occurred");
                        return;
                    }
            }

            Console.ResetColor();
            Console.WriteLine($"[LOGIN] Successfully Logged in to RCON Client ({client.Hostname}:{client.Port}). Initialising Client...");
            client.Setup();

            while (true)
                await client.SendPacketAsync(new Packet(null, PacketType.Command, 0, BattlEyeCommand.None, Console.ReadLine()));


        }
    }
}
