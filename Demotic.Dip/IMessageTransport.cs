using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Network
{
    public delegate void DataReadyCallback(byte[] data);
    public delegate void ConnectionClosedCallback();

    public interface IMessageTransport
    {
        event DataReadyCallback DataReady;
        event ConnectionClosedCallback ConnectionClosed;

        void SendData(byte[] data);
    }
}
