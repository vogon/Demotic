using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Demotic.Core.ObjectSystem;
using Demotic.Network;

namespace Demotic.Server
{
    partial class TelnetDipPresentation : IPresentation
    {
        public TelnetDipPresentation(IPEndPoint listenEndpoint)
        {
            _server = new TcpListener(listenEndpoint);
        }

        public void Start()
        {
            _server.Start();
        }

        public MessageChannel AwaitNextConnection()
        {
            TcpClient tcpClient = _server.AcceptTcpClient();

            var xport = new TcpMessageTransport(tcpClient);
            var format = new DipMessageFormat();
            return new MessageChannel(xport, format);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        private TcpListener _server;
        private static Encoding _DefaultEncoding = Encoding.ASCII;

    }
}
