using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Demotic.Dip.Test
{
    [TestClass]
    public class BencodingTest
    {
        private void AssertBytewiseIdentical(byte[] a, byte[] b)
        {
            Assert.AreEqual(a.Length, b.Length, 
                "encodings have different lengths ({0} != {1})", a.Length, b.Length);

            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i], b[i],
                    "miscompare at byte {0} ({1} != {2})", i, a[i], b[i]);
            }
        }

        private byte[] Bs(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }

        [TestMethod]
        public void TestEncodeInteger()
        {
            byte[] expected = Encoding.ASCII.GetBytes("i42e");
            byte[] actual = Bencoding.Encode(42);

            AssertBytewiseIdentical(expected, actual);
        }

        [TestMethod]
        public void TestEncodeString()
        {
            byte[] expected = Encoding.ASCII.GetBytes("4:test");
            byte[] actual = Bencoding.Encode(Bs("test"));

            AssertBytewiseIdentical(expected, actual);
        }

        [TestMethod]
        public void TestEncodeList()
        {
            List<object> l = new List<object>(new object[] { Bs("spam"), 12, Bs("eggs") });
            byte[] expected = Encoding.ASCII.GetBytes("l4:spami12e4:eggse");
            byte[] actual = Bencoding.Encode(l);

            AssertBytewiseIdentical(expected, actual);
        }

        [TestMethod]
        public void TestEncodeDictionary()
        {
            Dictionary<byte[], object> d = new Dictionary<byte[], object>();
            d.Add(Bs("what"), Bs("delicious"));
            d.Add(Bs("answer"), 42);
            d.Add(Bs("time"), Bs("now"));

            byte[] expected = Encoding.ASCII.GetBytes("d4:what9:delicious6:answeri42e4:time3:nowe");
            byte[] actual = Bencoding.Encode(d);

            AssertBytewiseIdentical(expected, actual);
        }
    }
}
