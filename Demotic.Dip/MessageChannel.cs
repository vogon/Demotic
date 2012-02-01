using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Network
{
    public delegate void ChannelClosedCallback();
    public delegate void MessageReceivedCallback(Message msg);

    public class MessageChannel
    {
        public MessageChannel(IMessageTransport xport, IMessageFormat format)
        {
            _xport = xport;
            _format = format;
            _buffer = new List<byte>();
            _nextSeq = 1;

            _xport.DataReady += OnDataReady;
            _xport.ConnectionClosed += OnConnectionClosed;
        }

        public event MessageReceivedCallback MessageReceived;
        public event ChannelClosedCallback ChannelClosed;

        public void SendMessage(Message msg)
        {
            if (msg.SequenceNumber < 0)
            {
                // get next sequence number and put it into msg.
                msg.SequenceNumber = _nextSeq;
                _nextSeq++;
            }
            else if (msg.SequenceNumber >= _nextSeq)
            {
                // if something above this in the stack passes a sequence number
                // in, we can't override its wishes because it might be using
                // the sequence number to correlate requests with responses.
                _nextSeq = msg.SequenceNumber + 1;
            }

            byte[] payload = _format.Encode(msg);

            _xport.SendData(payload);
        }

        private void OnDataReady(byte[] data)
        {
            _buffer.AddRange(data);

            int bytesConsumed;

            if (MessageReceived == null) return;

            try
            {
                Message msg = _format.Decode(_buffer.ToArray(), out bytesConsumed);

                if (msg != null)
                {
                    MessageReceived(msg);
                    _buffer.RemoveRange(0, bytesConsumed);
                }
                else
                {
                    return;
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("error in message format; flushing buffer");
                _buffer.Clear();
            }
        }

        private void OnConnectionClosed()
        {
            if (ChannelClosed != null)
            {
                ChannelClosed();
            }
        }

        private int _nextSeq;

        private IMessageTransport _xport;
        private IMessageFormat _format;

        private List<byte> _buffer;
    }
}
