using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Demotic.Core.ObjectSystem;

namespace Demotic.Dip
{
    public static class DObjectBencoder : IDObjectVisitor<byte[]>
    {
        private static byte[] Visit(DNumber num)
        {
            return Bencoding.Encode(num.IntValue);
        }

        private static byte[] Visit(DString str)
        {
            return Bencoding.Encode(Encoding.UTF8.GetBytes(str.Value));
        }

        private static byte[] Visit(DRecord rec)
        {
            Dictionary<string, DObject> values = rec.Values;
            var contents = new Dictionary<object, object>();

            foreach (var kvp in values)
            {
                contents.Add(kvp.Key, kvp.Value);
            }

            return Bencoding.Encode(contents);
        }

        public static DObject Decode(byte[] encoded)
        {
            int next;
            object o = Bencoding.Decode(encoded, out next);

            return BuildDemoticType(o);
        }

        private static DObject BuildDemoticType(object o)
        {
            if (o is long)
            {
                return new DNumber((long)o);
            }
            else if (o is byte[])
            {
                return new DString(new string(Encoding.UTF8.GetChars((byte[])o)));
            }
            else if (o is List<object>)
            {
                Debug.Fail("lists are unimplemented");
                return null;
            }
            else if (o is Dictionary<object, object>)
            {
                DRecord rec = new DRecord();

                foreach (var kvp in (Dictionary<object, object>)o)
                {
                    Debug.Assert(kvp.Key is byte[]);

                    string k = new string(Encoding.UTF8.GetChars((byte[])kvp.Key));
                    DObject v = BuildDemoticType(kvp.Value);

                    rec.Set(k, v);
                }

                return rec;
            }
            else
            {
                Debug.Fail("unexpected encoding type");
                return null;
            }
        }
    }
}
