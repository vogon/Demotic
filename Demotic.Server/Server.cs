using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

using Demotic.Core;
using Demotic.Core.ObjectSystem;
using Demotic.Network;
using System.Diagnostics;

namespace Demotic.Server
{
    internal sealed class Server
    {
        public Server()
        {
            _personalities = new List<IPresentation>();
            _channels = new List<MessageChannel>();
            _worker = new Worker();

            // TODO: hardcoded for now; add configuration
            IPEndPoint telnet = new IPEndPoint(IPAddress.Any, 17717);
            _personalities.Add(new TelnetDipPresentation(telnet));
        }

        private void AcceptLoop(IPresentation personality)
        {
            Console.WriteLine("in accept loop");

            while (true)
            {
                MessageChannel newChannel = personality.AwaitNextConnection();
                Console.WriteLine("accepted");
                _channels.Add(newChannel);

                // TODO: we're going to need to maintain a list of "canonical" clients for each personality at some point
                newChannel.MessageReceived += ((msg) => MakeActionAndDispatch(msg, newChannel));
                newChannel.ChannelClosed += (() => OnChannelClosed(newChannel));
            }
        }

        public void Start()
        {
            foreach (IPresentation p in _personalities)
            {
                p.Start();

                Task.Factory.StartNew(() => AcceptLoop(p), TaskCreationOptions.LongRunning);
            }

            _worker.Start();
        }

        public void Stop()
        {
            foreach (IPresentation p in _personalities)
            {
                p.Stop();
            }

            _worker.Stop();
        }

        private static string GetString(Message msg, string attrName)
        {
            if (!msg.Attributes.ContainsKey(attrName)) return null;

            object value = msg.Attributes[attrName];

            if (value is string)
            {
                return (string)value;
            }
            else if (value is DString)
            {
                return (string)((DString)value);
            }
            else
            {
                return null;
            }
        }

        private static DObject GetObject(Message msg, string attrName)
        {
            if (!msg.Attributes.ContainsKey(attrName)) return null;

            object value = msg.Attributes[attrName];

            if (value is DObject)
            {
                return (DObject)value;
            }
            else
            {
                return null;
            }
        }

        private void MakeActionAndDispatch(Message msg, MessageChannel channel)
        {
            UserAction request = null;
            RequestContext context = new RequestContext(channel, msg.SequenceNumber);

            if (msg.OpCode == MessageOpCode.GetObject)
            {
                string path = GetString(msg, "path");

                if (path == null) context.NegativeAcknowledge();

                request = new GetObjectAction(context, path);
            }
            else if (msg.OpCode == MessageOpCode.PutObject)
            {
                string path = GetString(msg, "path");
                DObject value = GetObject(msg, "value");

                if (path == null) context.NegativeAcknowledge();
                if (value == null) context.NegativeAcknowledge();

                request = new PutObjectAction(context, path, value);
            }
            else if (msg.OpCode == MessageOpCode.NG ||
                     msg.OpCode == MessageOpCode.OK ||
                     msg.OpCode == MessageOpCode.Quote)
            {
                // drop it on the floor; not expecting these.
                return;
            }
            else
            {
                throw new NotImplementedException();
            }

            Debug.Assert(request != null);
            _worker.Dispatch(request, context);
        }

        private void OnChannelClosed(MessageChannel channel)
        {
            Console.WriteLine("channel closed.");
            _channels.Remove(channel);
        }

        private List<MessageChannel> _channels;

        private List<IPresentation> _personalities;

        private Worker _worker;
    }

}
