using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using Demotic.Core.ObjectSystem;

namespace Demotic.Dip
{
    [Message("say")]
    public class SayMessage : Message
    {
        public SayMessage() { }
        public SayMessage(dynamic request)
        {
            Message = request.message;
        }

        public override byte[] Bencode()
        {
            var dict = new Dictionary<object, object>();
            dict.Add("op", "say");
            dict.Add("message", DObjectAdapter.Encode(Message));

            return Bencoding.Encode(dict);
        }

        public DString Message { get; set; }
    }

    [Message("ack")]
    public class AckMessage : Message
    {
        public AckMessage(dynamic request)
        {
        }

        public override byte[] Bencode()
        {
            var dict = new Dictionary<object, object>();
            dict.Add("op", "ack");

            return Bencoding.Encode(dict);
        }
    }

    [Message("nak")]
    public class NakMessage : Message
    {
        public NakMessage(dynamic request)
        {
        }

        public override byte[] Bencode()
        {
            var dict = new Dictionary<object, object>();
            dict.Add("op", "nak");

            return Bencoding.Encode(dict);
        }
    }

    [Message("obj")]
    public class ObjectMessage : Message
    {
        public ObjectMessage() { }
        public ObjectMessage(dynamic request)
        {
            Value = request.value;
        }

        public override byte[] Bencode()
        {
            var dict = new Dictionary<object, object>();
            dict.Add("op", "obj");
            dict.Add("value", DObjectAdapter.Encode(Value));

            return Bencoding.Encode(dict);
        }

        public DObject Value { get; set; }
    }
}
