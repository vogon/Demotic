using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using Demotic.Core.ObjectSystem;

namespace Demotic.Dip
{
    [Message("get")]
    public class GetMessage : Message
    {
        public GetMessage(dynamic request)
        {
            Path = request.path;
        }

        public override byte[] Bencode()
        {
            var dict = new Dictionary<object, object>();
            dict.Add("op", "get");
            dict.Add("path", Path);

            return Bencoding.Encode(dict);
        }

        public string Path { get; set; }
    }

    [Message("put")]
    public class PutMessage : Message
    {
        public PutMessage(dynamic request)
        {
            Path = request.path;
            Value = request.value;
        }

        public override byte[] Bencode()
        {
            var dict = new Dictionary<object, object>();
            dict.Add("op", "put");
            dict.Add("path", Path);
            dict.Add("value", DObjectAdapter.Encode(Value));

            return Bencoding.Encode(dict);
        }

        public string Path { get; set; }
        public DObject Value { get; set; }
    }
}
