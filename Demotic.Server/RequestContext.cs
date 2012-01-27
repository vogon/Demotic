using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Demotic.Core.ObjectSystem;
using Demotic.Network;

namespace Demotic.Server
{
    class RequestContext
    {
        public RequestContext(MessageChannel channel, int ackNumber)
        {
            _channel = channel;
            _ackNumber = ackNumber;
        }

        public void QuoteObject(DObject obj)
        {
            Message msg = new Message
            {
                OpCode = MessageOpCode.Quote,
                AckNumber = this._ackNumber
            };

            msg.Attributes = new Dictionary<string, object>();
            msg.Attributes["object"] = obj;

            _channel.SendMessage(msg);
        }

        public void Acknowledge()
        {
            Message msg = new Message 
            {
                OpCode = MessageOpCode.OK,
                AckNumber = this._ackNumber
            };

            _channel.SendMessage(msg);
        }

        public void NegativeAcknowledge()
        {
            Message msg = new Message
            {
                OpCode = MessageOpCode.NG,
                AckNumber = this._ackNumber
            };

            _channel.SendMessage(msg);
        }

        private int _ackNumber;
        private MessageChannel _channel;
        
        // TODO: eventually, some additional crap goes here to bind this to a user
    }
}
