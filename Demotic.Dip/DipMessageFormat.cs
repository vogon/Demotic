using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Demotic.Core.ObjectSystem;

namespace Demotic.Network
{
    public class DipMessageFormat : IMessageFormat
    {
        private const string SequenceNumberFieldName = "seq";
        private const string AckNumberFieldName = "to";
        private const string OpCodeFieldName = "op";

        /// <see cref="IMessageFormat.Decode"/>
        public Message? Decode(byte[] payload, out int bytesConsumed)
        {
            Bdecode decoder = new Bdecode(payload);
            Stack<DecodingFrame> frames = new Stack<DecodingFrame>();
            
            while (true)
            {
                var nodeType = decoder.Next();
                DecodingFrame top = frames.Count > 0 ? frames.Peek() : null;

                switch (nodeType)
                {
                    case Bdecode.NodeType.Bytestring:
                        top.InsertBytestring(decoder.BytestringValue);
                        break;

                    case Bdecode.NodeType.EndDictionary:
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

                    case Bdecode.NodeType.EndList:
                        Debug.Fail("whaaaaaaaaat");
                        break;

                    case Bdecode.NodeType.Error:
                        throw new BadBencodingException("blah");

                    case Bdecode.NodeType.Integer:
                        top.InsertInteger(decoder.IntegerValue);
                        break;

                    case Bdecode.NodeType.StartDictionary:
                        if (frames.Count == 0)
                        {
                            frames.Push(new MessageToplevelFrame());
                        }
                        else
                        {
                            frames.Push(new DRecordFrame());
                        }
                                                
                        break;

                    case Bdecode.NodeType.StartList:
                        Debug.Fail("oh nooooo");
                        break;

                    case Bdecode.NodeType.Eof:
                        // expected a node here, but didn't get one. return "premature eof".
                        bytesConsumed = 0;
                        return null;
                }
            }
        }

        private abstract class DecodingFrame
        {
            public DecodingFrame() { }

            public abstract void InsertInteger(long i);
            public abstract void InsertBytestring(byte[] str);
            public abstract void InsertObject(DObject obj);

            public abstract object Realize();
        }

        private class MessageToplevelFrame : DecodingFrame
        {
            public MessageToplevelFrame()
            {
                _msg.Attributes = new Dictionary<string, object>();
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
                        _msg.Attributes[(string)_key] = (DNumber)i;
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

        private class DRecordFrame : DecodingFrame
        {
            public DRecordFrame()
            {
                _rec = new DRecord();
            }

            public override void InsertInteger(long i)
            {
                if (_key == null)
                {
                    _key = i;
                }
                else
                {
                    Debug.Assert(_key is string);
                    _rec[(string)_key] = (DNumber)i;

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
                    _rec[(string)_key] = new DString(s);
                    
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
                    _rec[(string)_key] = obj;
                    
                    _key = null;                    
                }
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
            Bencode encoder = new Bencode();
            Encoding textEncoding = Encoding.UTF8;

            encoder.StartDictionary();

            encoder.ByteString(textEncoding.GetBytes(SequenceNumberFieldName))
                   .Integer(msg.SequenceNumber)
                   .ByteString(textEncoding.GetBytes(AckNumberFieldName))
                   .Integer(msg.AckNumber)
                   .ByteString(textEncoding.GetBytes(OpCodeFieldName))
                   .Integer((long)msg.OpCode);

            foreach (string k in msg.Attributes.Keys)
            {
                encoder.ByteString(textEncoding.GetBytes(k));

                object value = msg.Attributes[k];

                if (typeof(long).IsAssignableFrom(value.GetType()))
                {
                    encoder.Integer((long)value);
                }
                else if (value is DNumber)
                {
                    encoder.Integer(((DNumber)value).IntValue);
                }
                else if (value is string)
                {
                    encoder.ByteString(textEncoding.GetBytes((string)value));
                }
                else if (value is DString)
                {
                    encoder.ByteString(textEncoding.GetBytes((string)((DString)value)));
                }
                else if (value is DObject)
                {
                    // TODO: unstub
                    encoder.ByteString(textEncoding.GetBytes("[oh god no]"));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            // TODO: traverse rest of message

            encoder.FinishDictionary();

            return encoder.Encoded;
        }
    }
}
