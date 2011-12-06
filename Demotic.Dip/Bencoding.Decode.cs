using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Demotic.Dip
{
    public static partial class Bencoding
    {
        private static bool IsAsciiDigit(byte b)
        {
            return (b >= (byte)'0' && b <= (byte)'9');
        }

        public static object Decode(byte[] encoded, out int nextPos)
        {
            return Decode(encoded, 0, out nextPos);
        }

        public static object Decode(byte[] encoded, int start, out int nextPos)
        {
            if (IsAsciiDigit(encoded[start]))
            {
                // bytestrings start with numerals.
                return DecodeBytestring(encoded, start, out nextPos);
            }
            else if (encoded[start] == (byte)'i')
            {
                // integers start with 'i'.
                return DecodeInteger(encoded, start, out nextPos);
            }
            else if (encoded[start] == (byte)'l')
            {
                // lists start with 'l'.
                return DecodeList(encoded, start, out nextPos);
            }
            else if (encoded[start] == (byte)'d')
            {
                // dictionaries start with 'd'.
                return DecodeDictionary(encoded, start, out nextPos);
            }
            else
            {
                throw new BadBencodingException("couldn't recognize bencoding type");
            }
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
                throw new BadBencodingException("bytestring size off the end");
            }

            byte[] payload = new byte[size];

            Array.Copy(encoded, separatorPos + 1, payload, 0, size);
            return payload;
        }

        private static object DecodeInteger(byte[] encoded, int start, out int nextPos)
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
                throw new BadBencodingException("unclosed integer");
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

        private static object DecodeDictionary(byte[] encoded, int start, out int nextPos)
        {
            Debug.Assert(encoded[start] == 'd');

            Dictionary<object, object> result = new Dictionary<object,object>();
            int elementPos = start + 1;
            object key = null;

            while (true)
            {
                if (elementPos >= encoded.Length)
                {
                    // hit the end of the encoding without an end marker.
                    throw new BadBencodingException("unclosed dict");
                }

                if (encoded[elementPos] == 'e')
                {
                    // end of dictionary reached.
                    if (key == null)
                    {
                        // no leftover key; everything's good
                        break;
                    }
                    else
                    {
                        // last element, but there's an unused key!
                        throw new BadBencodingException("unpaired key in dict");
                    }
                }

                object o = Decode(encoded, elementPos, out elementPos);

                // if key is null, this is the next key, otherwise, this is the value
                // associated with key.
                if (key == null)
                {
                    key = o;
                }
                else
                {
                    result.Add(key, o);
                    key = null;
                }
            }

            // we got here when elementPos was an 'e' -- the next element starts
            // one character later.
            nextPos = elementPos + 1;
            return result;
        }

        private static object DecodeList(byte[] encoded, int start, out int nextPos)
        {
            Debug.Assert(encoded[start] == 'l');

            List<object> result = new List<object>();
            int elementPos = start + 1;

            while (true)
            {
                if (encoded[elementPos] == 'e')
                {
                    // end of list reached.
                    break;
                }

                object o = Decode(encoded, elementPos, out elementPos);

                result.Add(o);

                if (elementPos >= encoded.Length)
                {
                    // hit the end of the encoding without an end marker.
                    throw new BadBencodingException("unclosed list");
                }
            }
            
            // we got here when elementPos was an 'e' -- the next element starts
            // one character later.
            nextPos = elementPos + 1;
            return result;
        }
    }
}
