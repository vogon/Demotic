using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Network
{
    class ConsoleMessageTransport : IMessageTransport
    {
        public event DataReadyCallback DataReady;

        public void SendData(byte[] data)
        {
            throw new NotImplementedException();
        }


        public event ConnectionClosedCallback ConnectionClosed;
    }
}
