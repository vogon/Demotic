using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using Demotic.Core;
using Demotic.Core.ObjectSystem;
using Demotic.Network;

namespace Demotic.Client
{
    internal class ResponseReceivedEventArgs : EventArgs
    {
        public Message Request { get; set; }
        public Message Response { get; set; }

        public object Context { get; set; }
    }

    internal delegate void ResponseReceivedHandler(object sender, ResponseReceivedEventArgs e);

    internal class Client
    {
        public Client(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;

            _unacked = new Dictionary<int, AckContext>();
        }

        public void Start()
        {
            TcpClient client = new TcpClient(_hostname, _port);

            _xport = new TcpMessageTransport(client);
            _format = new DipMessageFormat();

            _channel = new MessageChannel(_xport, _format);
            _channel.MessageReceived += OnMessageReceived;
        }

        public void Stop()
        {
            // should really dispose stuff here
        }

        public void SendMessage(Message msg, object context)
        {
            _unacked.Add(msg.SequenceNumber,
                new AckContext { Request = msg, Context = context });
            _channel.SendMessage(msg);
        }

        private void OnMessageReceived(Message msg)
        {
            if (_unacked.ContainsKey(msg.AckNumber))
            {
                AckContext req = _unacked[msg.AckNumber];

                if (ResponseReceived != null)
                {
                    ResponseReceived(this, new ResponseReceivedEventArgs
                        {
                            Request = req.Request,
                            Response = msg,
                            Context = req.Context
                        }
                    );
                }

                if (msg.OpCode == MessageOpCode.OK || msg.OpCode == MessageOpCode.NG)
                {
                    _unacked.Remove(msg.AckNumber);
                }
            }
            else
            {
                if (msg.AckNumber != 0)
                {
                    Debug.Print("received unsolicited ack for seq = {0}", msg.AckNumber);
                }
            }
        }

        public event ResponseReceivedHandler ResponseReceived;

        private struct AckContext
        {
            public Message Request { get; set; }
            public object Context { get; set; }
        }

        private Dictionary<int, AckContext> _unacked;

        private string _hostname;
        private int _port;

        private TcpMessageTransport _xport;
        private DipMessageFormat _format;

        private MessageChannel _channel;
    }
}
