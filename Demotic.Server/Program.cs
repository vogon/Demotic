using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Demotic.Core;

namespace Demotic.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine e = new Engine();
            Script s = new Script("(root.Get(\"flag\") != null) && (((DNumber)root.Get(\"flag\")).IntValue >= 10)", 
                                  "Console.WriteLine(\"hi!\"); Environment.Exit(0);");

            Server srv = new Server();

            srv.Start();

            e.RegisterScript(s);
            e.Start();

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
