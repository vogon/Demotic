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
    internal class MessageAttribute : Attribute
    {
        public MessageAttribute(string opcode)
        {
            Opcode = opcode;
        }

        public string Opcode { get; set; }
    }

    public abstract class Message
    {
        public static Message Decode(byte[] encoded)
        {
            dynamic msgPayload = DObjectAdapter.Decode(encoded);

            if (msgPayload == null)
            {
                return null;
            }

            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (object attr in t.GetCustomAttributes(false))
                {
                    MessageAttribute req = attr as MessageAttribute;
                    if (req == null) continue;

                    if (msgPayload.op == req.Opcode)
                    {
                        return (Message)t.GetConstructor(new[] { typeof(DRecord) }).Invoke(new[] { msgPayload });
                    }
                }
            }

            return null;
        }

        protected Message() { }

        public abstract byte[] Bencode();
    }
}
