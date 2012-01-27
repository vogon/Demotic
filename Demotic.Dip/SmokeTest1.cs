using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Demotic.Network
{
    public static class SmokeTest1
    {
        public static void MsgPing(Message msg)
        {
            _chan.SendMessage(
                new Message
                {
                    OpCode = MessageOpCode.OK,
                    SequenceNumber = msg.SequenceNumber + 1,
                    AckNumber = msg.SequenceNumber,
                }
            );

            Console.WriteLine("ping (seq {0}, opc {1})", msg.SequenceNumber, msg.OpCode);
        }

        public static void Goodbye()
        {
            Console.WriteLine("bye!");
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 17717);

            server.Start();

            TcpClient client = server.AcceptTcpClient();
            _xport = new TcpMessageTransport(client);
            _fmt = new DipMessageFormat();

            /*string test = "d3:seqi1e2:opi2e2:toi0ee";
            byte[] payload = Encoding.UTF8.GetBytes(test);

            int ofs = 0;

            Message? msg = _fmt.Decode(payload, out ofs);*/

            _chan = new MessageChannel(_xport, _fmt);

            _chan.MessageReceived += MsgPing;

            while (true)
            {

            }
        }

        private static DipMessageFormat _fmt;
        private static TcpMessageTransport _xport;
        private static MessageChannel _chan;
    }
}
