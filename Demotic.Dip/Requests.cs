using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using Demotic.Core.ObjectSystem;

namespace Demotic.Dip
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class RequestAttribute : Attribute
    {
        public RequestAttribute(string opcode)
        {
            Opcode = opcode;
        }

        public string Opcode { get; set; }
    }

    public abstract class Request
    {
        public static Request Decode(byte[] encoded)
        {
            DRecord msgPayload = DObjectAdapter.Decode(encoded) as DRecord;

            if (msgPayload == null)
            {
                return null;
            }

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (object attr in t.GetCustomAttributes(false))
                {
                    RequestAttribute req = attr as RequestAttribute;
                    if (req == null) continue;

                    if (string.Equals(((DString)msgPayload.Get("op")).Value, req.Opcode))
                    {
                        return (Request)t.GetConstructor(new[] { typeof(DRecord) }).Invoke(new[] { msgPayload });
                    }
                }
            }

            return null;
        }

        protected Request() { }

        public abstract byte[] Bencode();
    }

    [Request("get")]
    public class GetRequest : Request
    {
        public GetRequest(DRecord request)
        {
            Debug.Assert(((DString)request.Get("op")).Value.Equals("get"));

            Path = ((DString)request.Get("path")).Value;
        }

        public override byte[] Bencode()
        {
            var dict = new Dictionary<object, object>();
            dict.Add("op", "get");
            dict.Add("path", Path);

            return Bencoding.Encode(dict);
        }

        public string Path { get; private set; }
    }
}
