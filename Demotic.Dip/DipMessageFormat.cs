using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Demotic.Core.ObjectSystem;

namespace Demotic.Network
{
    public partial class DipMessageFormat : IMessageFormat
    {
        private const string SequenceNumberFieldName = "seq";
        private const string AckNumberFieldName = "to";
        private const string OpCodeFieldName = "op";

        private static readonly Encoding TextEncoding = Encoding.UTF8;

        /// <see cref="IMessageFormat.Decode"/>
        public Message Decode(byte[] payload, out int bytesConsumed)
        {
            Bdecoder decoder = new Bdecoder(payload);
            Stack<DecodingFrame> frames = new Stack<DecodingFrame>();
            
            while (true)
            {
                var nodeType = decoder.Next();
                DecodingFrame top = frames.Count > 0 ? frames.Peek() : null;

                switch (nodeType)
                {
                    case Bdecoder.NodeType.ByteString:
                        top.InsertBytestring(decoder.ByteStringValue);
                        break;

                    case Bdecoder.NodeType.EndDictionary:
                    case Bdecoder.NodeType.EndDNumber:
                    case Bdecoder.NodeType.EndDRecord:
                    case Bdecoder.NodeType.EndDString:
                        DecodingFrame child = frames.Pop();
                        DecodingFrame parent = frames.Count > 0 ? frames.Peek() : null;

                        if (parent == null)
                        {
                            Debug.Assert(child is MessageToplevelFrame);

                            bytesConsumed = decoder.NextOffset;
                            return (Message)child.Realize();
                        }
                        else
                        {
                            parent.InsertObject((DObject)child.Realize());
                        }

                        break;

                    case Bdecoder.NodeType.EndList:
                        Debug.Fail("whaaaaaaaaat");
                        break;

                    case Bdecoder.NodeType.Integer:
                        top.InsertInteger(decoder.IntegerValue);
                        break;

                    case Bdecoder.NodeType.Number:
                        top.InsertNumber(decoder.NumberValue);
                        break;

                    case Bdecoder.NodeType.StartDictionary:
                        if (frames.Count == 0)
                        {
                            frames.Push(new MessageToplevelFrame());
                        }
                        else
                        {
                            Debug.Assert(false);
                            //frames.Push(new DRecordFrame());
                        }
                                                
                        break;

                    case Bdecoder.NodeType.StartList:
                        Debug.Fail("oh nooooo");
                        break;

                    case Bdecoder.NodeType.StartDNumber:
                        frames.Push(new DNumberFrame());
                        break;

                    case Bdecoder.NodeType.StartDRecord:
                        frames.Push(new DRecordFrame());
                        break;

                    case Bdecoder.NodeType.StartDString:
                        frames.Push(new DStringFrame());
                        break;

                    case Bdecoder.NodeType.Eof:
                        // expected a node here, but didn't get one. return "premature eof".
                        bytesConsumed = 0;
                        return null;
                }
            }
        }

        private abstract class DecodingFrame
        {
            public DecodingFrame() { }

            public virtual void InsertInteger(long i)
            {
                Debug.Assert(false, "expectation failed");
            }

            public virtual void InsertBytestring(byte[] str)
            {
                Debug.Assert(false, "expectation failed");
            }

            public virtual void InsertObject(DObject obj)
            {
                Debug.Assert(false, "expectation failed");
            }

            public virtual void InsertNumber(decimal dec)
            {
                Debug.Assert(false, "expectation failed");
            }

            public abstract object Realize();
        }

        private class MessageToplevelFrame : DecodingFrame
        {
            public MessageToplevelFrame()
            {
                _msg = new Message();
            }

            public override void InsertInteger(long i)
            {
                if (_key == null)
                {
                    _key = i;
                }
                else
                {
                    if (_key.Equals(SequenceNumberFieldName))
                    {
                        _msg.SequenceNumber = (int)i;
                    }
                    else if (_key.Equals(AckNumberFieldName))
                    {
                        _msg.AckNumber = (int)i;
                    }
                    else if (_key.Equals(OpCodeFieldName))
                    {
                        _msg.OpCode = (MessageOpCode)i;
                    }
                    else
                    {
                        Debug.Assert(_key is string);
                        _msg.Attributes[(string)_key] = (long)i;
                    }

                    _key = null;
                }
            }

            public override void InsertBytestring(byte[] str)
            {
                string s = new string(Encoding.UTF8.GetChars(str));

                if (_key == null)
                {
                    _key = s;
                }
                else
                {
                    Debug.Assert(_key is string);
                    _msg.Attributes[(string)_key] = new DString(s);

                    _key = null;
                }
            }

            public override void InsertObject(DObject obj)
            {
                if (_key == null)
                {
                    _key = obj;
                }
                else
                {
                    Debug.Assert(_key is string);
                    _msg.Attributes[(string)_key] = obj;

                    _key = null;
                }
            }

            public override object Realize()
            {
                return _msg;
            }

            private object _key;
            private Message _msg;
        }

        private class DNumberFrame : DecodingFrame
        {
            public override void InsertNumber(decimal dec)
            {
                _n = dec;
            }

            public override object Realize()
            {
                return _n;
            }

            private DNumber _n;
        }

        private class DStringFrame : DecodingFrame
        {
            public override void InsertBytestring(byte[] str)
            {
                _str = Encoding.UTF8.GetString(str);
            }

            public override object Realize()
            {
                return _str;
            }

            private DString _str;
        }

        private class DRecordFrame : DecodingFrame
        {
            public DRecordFrame()
            {
                _rec = new DRecord();
            }

            public override void InsertBytestring(byte[] str)
            {
                string s = new string(Encoding.UTF8.GetChars(str));

                Debug.Assert(_key == null);

                _key = s;
            }

            public override void InsertObject(DObject obj)
            {
                Debug.Assert(_key != null);
                Debug.Assert(_key is string);

                _rec[(string)_key] = obj;
                _key = null;
            }

            public override void InsertNumber(decimal dec)
            {
                Debug.Assert(_key != null);
                Debug.Assert(_key is string);

                _rec[(string)_key] = new DNumber(dec);
                _key = null;
            }

            public override object Realize()
            {
                return _rec;
            }

            private object _key;
            private DRecord _rec;
        }
        
        public byte[] Encode(Message msg)
        {
            Bencoder encoder = new Bencoder();

            encoder.StartDictionary();

            encoder.ByteString(TextEncoding.GetBytes(SequenceNumberFieldName))
                   .Integer(msg.SequenceNumber)
                   .ByteString(TextEncoding.GetBytes(AckNumberFieldName))
                   .Integer(msg.AckNumber)
                   .ByteString(TextEncoding.GetBytes(OpCodeFieldName))
                   .Integer((long)msg.OpCode);

            foreach (string k in msg.Attributes.Keys)
            {
                encoder.ByteString(TextEncoding.GetBytes(k));

                object value = msg.Attributes[k];

                if (typeof(long).IsAssignableFrom(value.GetType()))
                {
                    encoder.Integer((long)value);
                }
                else if (value is DNumber)
                {
                    SerializeDNumber(encoder, (DNumber)value);
                }
                else if (value is string)
                {
                    encoder.ByteString(TextEncoding.GetBytes((string)value));
                }
                else if (value is DString)
                {
                    SerializeDString(encoder, (DString)value);
                }
                else if (value is DRecord)
                {
                    SerializeDRecord(encoder, (DRecord)value);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            encoder.FinishDictionary();

            return encoder.Encoded;
        }
    }
}
