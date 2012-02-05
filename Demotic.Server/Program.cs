﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Demotic.Core;

namespace Demotic.Server
{
    class Program
    {
        public static World World { get; private set; }

        static void Main(string[] args)
        {
            World = new World();
            // PROBABLY A BUG: if there's a compile error in this script, the script loop will crash
            // and never run scripts again.
            Script s = new Script("(Get(\"flag\") != null) && (((DNumber)Get(\"flag\")).Value >= 10)",
                                  "Console.WriteLine(\"hi!\"); Environment.Exit(0);");

            Server srv = new Server();

            srv.Start();

            World.RegisterScript(s);
            World.Start();

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
