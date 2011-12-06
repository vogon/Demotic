using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Demotic.Dip
{
    public static partial class Bencoding
    {
        public static byte[] Encode(int n)
        {
            return Encoding.ASCII.GetBytes(string.Format("i{0}e", n));
        }

        public static byte[] Encode(byte[] s)
        {
            byte[] len = Encoding.ASCII.GetBytes(string.Format("{0}:", s.Length));
            byte[] all = new byte[len.Length + s.Length];

            Array.Copy(len, all, len.Length);
            Array.Copy(s, 0, all, len.Length, s.Length);

            return all;
        }

        public static byte[] Encode(Dictionary<object, object> d)
        {
            byte[] prologue = new[] { (byte)'d' };
            byte[] epilogue = new[] { (byte)'e' };
            List<byte[]> sections = new List<byte[]>();

            sections.Add(prologue);

            foreach (var kvp in d)
            {
                sections.Add(Encode(kvp.Key));
                sections.Add(Encode(kvp.Value));
            }

            sections.Add(epilogue);

            return JoinArrays(sections);
        }

        public static byte[] Encode(List<object> l)
        {
            byte[] prologue = new[] { (byte)'l' };
            byte[] epilogue = new[] { (byte)'e' };
            List<byte[]> sections = new List<byte[]>();

            sections.Add(prologue);

            foreach (object o in l)
            {
                sections.Add(Encode(o));
            }

            sections.Add(epilogue);

            return JoinArrays(sections);
        }

        private static byte[] Encode(object o)
        {
            Type t = typeof(Bencoding);
            MethodInfo mi = t.GetMethod(
                "Encode", 
                BindingFlags.Public | BindingFlags.Static, null, 
                new[] { o.GetType() }, null);

            if (mi != null && mi != MethodInfo.GetCurrentMethod())
            {
                return (byte[])mi.Invoke(null /*should be static, ignored*/, new[] { o });
            }

            throw new NotBencodableException("bloop");
        }
    }
}
