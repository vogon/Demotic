using System;
using System.IO;

namespace Demotic.Dip
{
    public class MessageChannel
    {
        public delegate void MessageReceivedDelegate(Message msg);

        public MessageChannel(Stream input, Stream output)
        {
            if (!input.CanRead) throw new ArgumentException("input is not readable");
            if (!output.CanWrite) throw new ArgumentException("output is not writable");

            _input = input;
            _output = output;
        }

        private Stream _input;
        private Stream _output;

        public event MessageReceivedDelegate MessageReceived;

        public void SendMessage(Message msg)
        {
            byte[] buf = msg.Bencode();

            _output.Write(buf, 0, buf.Length);
        }

        public void Close()
        {
            _input.Close();
            _output.Close();
        }
    }
}