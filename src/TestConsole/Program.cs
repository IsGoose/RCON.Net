﻿using RCON.Net;
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
            var client = new RCONClient("127.0.0.1", 2302, "ChangeMe");
        }
    }
}
