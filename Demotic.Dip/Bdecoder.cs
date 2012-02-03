using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Demotic.Network
{
    internal class Bdecoder
    {
        public Bdecoder(byte[] data)
        {
            _data = data;
            _offset = 0;
            _nestingStack = new Stack<BencodingComplexType>();
        }

        public enum NodeType
        {
            Eof = 0,
            Integer,
            ByteString,
            StartDictionary,
            EndDictionary,
            StartList,
            EndList,
            StartDNumber,
            EndDNumber,
            Number,
            StartDString,
            EndDString,
            StartDRecord,
            EndDRecord,
            StartMetadata,
            EndMetadata,
        }

        public NodeType Next()
        {
            if (_offset >= _data.Length) 
            { 
                return NodeType.Eof; 
            }

            byte ch = _data[_offset];

            if (IsAsciiDigit(ch))
            {
                // bytestring: [length]:[payload] pair
                int next;
                byte[] value = DecodeBytestring(_data, _offset, out next);

                if (value == null)
                {
                    _offset = _data.Length;
                    _nodeType = NodeType.Eof;
                }
                else
                {
                    _offset = next;
                    _bytestringValue = value;
                    _nodeType = NodeType.ByteString;
                }
            }
            else if (ch == (byte)'i')
            {
                // integer: i[value]e
                int next;
                long? value = DecodeInteger(_data, _offset, out next);

                if (value == null)
                {
                    _offset = _data.Length;
                    _nodeType = NodeType.Eof;
                }
                else
                {
                    _offset = next;
                    _integerValue = value.Value;
                    _nodeType = NodeType.Integer;
                }
            }
            else if (ch == (byte)'n')
            {
                // "little DNumber": arbitrary-precision number, no metadata; n[value]e
                int next;
                decimal? value = DecodeNumber(_data, _offset, out next);

                if (value == null)
                {
                    _offset = _data.Length;
                    _nodeType = NodeType.Eof;
                }
                else
                {
                    _offset = next;
                    _numberValue = value.Value;
                    _nodeType = NodeType.Number;
                }
            }
            else if (ch == (byte)'l')
            {
                // list: l[value]...e
                _nestingStack.Push(BencodingComplexType.List);
                _offset++;
                _nodeType = NodeType.StartList;
            }
            else if (ch == (byte)'d')
            {
                // dictionary: d[key][value]...e
                _nestingStack.Push(BencodingComplexType.Dictionary);
                _offset++;
                _nodeType = NodeType.StartDictionary;
            }
            else if (ch == (byte)'m')
            {
                // metadata dictionary: m[key][value]...e
                _nestingStack.Push(BencodingComplexType.Metadata);
                _offset++;
                _nodeType = NodeType.StartMetadata;
            }
            else if (ch == (byte)'N')
            {
                // "big DNumber": "little DNumber" with metadata; N[metadata][little DNumber]e
                _nestingStack.Push(BencodingComplexType.DNumber);
                _offset++;
                _nodeType = NodeType.StartDNumber;
            }
            else if (ch == (byte)'S')
            {
                // "big DString": bytestring with metadata; S[metadata][bytestring]e
                _nestingStack.Push(BencodingComplexType.DString);
                _offset++;
                _nodeType = NodeType.StartDString;
            }
            else if (ch == (byte)'R')
            {
                // "big DRecord": dictionary with metadata; R[metadata][key][value]...e
                _nestingStack.Push(BencodingComplexType.DRecord);
                _offset++;
                _nodeType = NodeType.StartDRecord;
            }
            else if (ch == (byte)'e')
            {
                // end of complex object marker
                BencodingComplexType t;

                if (_nestingStack.Count == 0)
                {
                    throw new BadBencodingException("end at nesting depth 0");
                }
                else
                {
                    t = _nestingStack.Pop();
                }

                _offset++;

                switch (t)
                {
                    case BencodingComplexType.Dictionary:
                        _nodeType = NodeType.EndDictionary;
                        break;
                    case BencodingComplexType.List:
                        _nodeType = NodeType.EndList;
                        break;
                    case BencodingComplexType.DNumber:
                        _nodeType = NodeType.EndDNumber;
                        break;
                    case BencodingComplexType.DString:
                        _nodeType = NodeType.EndDString;
                        break;
                    case BencodingComplexType.DRecord:
                        _nodeType = NodeType.EndDRecord;
                        break;
                    case BencodingComplexType.Metadata:
                        _nodeType = NodeType.EndMetadata;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                throw new BadBencodingException("didn't recognize node type");
            }

            return _nodeType;
        }

        public long IntegerValue
        {
            get
            {
                if (_nodeType != NodeType.Integer)
                {
                    throw new InvalidOperationException();
                }

                return _integerValue;
            }
        }

        public byte[] ByteStringValue
        {
            get
            {
                if (_nodeType != NodeType.ByteString)
                {
                    throw new InvalidOperationException();
                }

                return _bytestringValue;
            }
        }

        // TODO: replace with real type
        public decimal NumberValue
        {
            get
            {
                if (_nodeType != NodeType.Number)
                {
                    throw new InvalidOperationException();
                }

                return _numberValue;
            }
        }

        public int NextOffset
        {
            get
            {
                return _offset;
            }
        }

        private byte[] _data;
        private int _offset;

        private NodeType _nodeType;
        private long _integerValue;
        private byte[] _bytestringValue;
        // TODO: replace with real type.
        private decimal _numberValue;

        private Stack<BencodingComplexType> _nestingStack;

        private static bool IsAsciiDigit(byte ch)
        {
            return (ch >= '0' && ch <= '9');
        }

        private static byte[] DecodeBytestring(byte[] encoded, int start, out int nextPos)
        {
            int separatorPos = -1;

            // scan forward to find the size-payload separator.
            for (int pos = start; pos < encoded.Length; pos++)
            {
                if (!IsAsciiDigit(encoded[pos]))
                {
                    if (encoded[pos] == (byte)':')
                    {
                        // found separator!
                        if (pos == start) { throw new BadBencodingException("empty size in bytestring"); }

                        separatorPos = pos;
                        break;
                    }
                    else
                    {
                        throw new BadBencodingException("[^0-9:] in size of bytestring");
                    }
                }
            }

            if (separatorPos == -1)
            {
                // no separator found before eof
                nextPos = start;
                return null;
            }

            // convert size to a UTF-16 string.
            string sizeString = new string(Encoding.ASCII.GetChars(encoded, start, separatorPos - start));

            // parse size.
            int size;

            try
            {
                size = int.Parse(sizeString);
            }
            catch (OverflowException)
            {
                throw new BadBencodingException("size of bytestring too large");
            }

            // chop and return.
            nextPos = separatorPos + 1 /*character after separator*/ + size;

            if (nextPos > encoded.Length)
            {
                // string body ran off the end.
                nextPos = start;
                return null;
            }

            byte[] payload = new byte[size];

            Array.Copy(encoded, separatorPos + 1, payload, 0, size);
            return payload;
        }

        private static long? DecodeInteger(byte[] encoded, int start, out int nextPos)
        {
            Debug.Assert(encoded[start] == (byte)'i');

            int end = -1;

            for (int pos = start + 1; pos < encoded.Length; pos++)
            {
                if (encoded[pos] == (byte)'e')
                {
                    end = pos;
                    break;
                }
            }

            if (end == -1)
            {
                // no end found.  bomb out.
                nextPos = start;
                return null;
            }

            string s = new string(Encoding.ASCII.GetChars(encoded, start + 1, end - (start + 1)));

            try
            {
                long i = long.Parse(s);

                nextPos = end + 1;
                return i;
            }
            catch (FormatException)
            {
                throw new BadBencodingException("garbage in integer");
            }
            catch (OverflowException)
            {
                throw new BadBencodingException("integer too large");
            }

            throw new NotImplementedException();
        }

        private static decimal? DecodeNumber(byte[] encoded, int start, out int nextPos)
        {
            Debug.Assert(encoded[start] == (byte)'n');

            int end = -1;

            for (int pos = start + 1; pos < encoded.Length; pos++)
            {
                if (encoded[pos] == (byte)'e')
                {
                    end = pos;
                    break;
                }
            }

            if (end == -1)
            {
                // no end found.  bomb out.
                nextPos = start;
                return null;
            }

            string s = new string(Encoding.ASCII.GetChars(encoded, start + 1, end - (start + 1)));

            try
            {
                decimal d = decimal.Parse(s);

                nextPos = end + 1;
                return d;
            }
            catch (FormatException)
            {
                throw new BadBencodingException("garbage in integer");
            }
            catch (OverflowException)
            {
                throw new BadBencodingException("integer too large");
            }

            throw new NotImplementedException();
        }

    }
}
