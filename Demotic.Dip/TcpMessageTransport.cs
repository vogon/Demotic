using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Demotic.Network
{
    public class TcpMessageTransport : IMessageTransport
    {
        public TcpMessageTransport(TcpClient client)
        {
            _client = client;

            AwaitReceive();
        }

        public event DataReadyCallback DataReady;
        public event ConnectionClosedCallback ConnectionClosed;

        public void SendData(byte[] data)
        {
            int written = 0;

            while (written < data.Length)
            {
                written += _client.Client.Send(data, 
                    written, data.Length - written, SocketFlags.None);
            }
        }

        private void AwaitReceive()
        {
            _pendingReceiveBuffer = new byte[1024];
            _pendingReceiveResult = _client.Client.BeginReceive(_pendingReceiveBuffer, 
                0, 1024, SocketFlags.None, OnReceive, null);
        }

        private void OnReceive(IAsyncResult ar)
        {
            int bytesReceived = _client.Client.EndReceive(ar);

            if (bytesReceived == 0)
            {
                // connection closed by remote host; signal that if we have
                // a registered callback.
                if (ConnectionClosed != null) ConnectionClosed();
                return;
            }

            if (DataReady != null)
            {
                byte[] data = new byte[bytesReceived];
                Array.Copy(_pendingReceiveBuffer, data, bytesReceived);

                DataReady(data);
            }

            // and start the cycle again.
            AwaitReceive();
        }

        private IAsyncResult _pendingReceiveResult;
        private byte[] _pendingReceiveBuffer;

        private TcpClient _client;
    }
}
