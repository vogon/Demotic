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
        public void TestEncodeIntegerNegative()
        {
            byte[] expected = Encoding.ASCII.GetBytes("i-5e");
            byte[] actual = Bencoding.Encode(-5);

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
        public void TestEncodeStringEmpty()
        {
            byte[] expected = Encoding.ASCII.GetBytes("0:");
            byte[] actual = Bencoding.Encode(Bs(""));

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
        public void TestEncodeListEmpty()
        {
            List<object> l = new List<object>();
            byte[] expected = Encoding.ASCII.GetBytes("le");
            byte[] actual = Bencoding.Encode(l);

            AssertBytewiseIdentical(expected, actual);
        }

        [TestMethod]
        public void TestEncodeDictionary()
        {
            Dictionary<object, object> d = new Dictionary<object, object>();
            d.Add(Bs("what"), Bs("delicious"));
            d.Add(Bs("answer"), 42);
            d.Add(Bs("time"), Bs("now"));

            byte[] expected = Encoding.ASCII.GetBytes("d4:what9:delicious6:answeri42e4:time3:nowe");
            byte[] actual = Bencoding.Encode(d);

            AssertBytewiseIdentical(expected, actual);
        }

        [TestMethod]
        public void TestEncodeDictionaryEmpty()
        {
            Dictionary<object, object> d = new Dictionary<object, object>();

            byte[] expected = Encoding.ASCII.GetBytes("de");
            byte[] actual = Bencoding.Encode(d);

            AssertBytewiseIdentical(expected, actual);
        }

        [TestMethod]
        public void TestDecodeBytestring()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("blah5:magicgarbage");
            int start = 4;
            int next;

            byte[] actual = (byte[])Bencoding.Decode(encoded, start, out next);
            byte[] expected = Encoding.ASCII.GetBytes("magic");

            AssertBytewiseIdentical(actual, expected);
            Assert.AreEqual(next, start + 7);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeBytestringPrematureEof()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("7:overr");
            int next;

            byte[] actual = (byte[])Bencoding.Decode(encoded, out next);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeBytestringSizeGarbage()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("1%ab#2:interference");
            int next;

            byte[] actual = (byte[])Bencoding.Decode(encoded, out next);
        }

        [TestMethod]
        public void TestDecodeInteger()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("numberzi427efffffff");
            int start = 7;
            int next;

            long expected = 427;
            long actual = (long)Bencoding.Decode(encoded, start, out next);

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(next, start + 5);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeIntegerGarbage()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("isawthesign");
            int next;

            long actual = (long)Bencoding.Decode(encoded, out next);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeIntegerHalfopen()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("i123");
            int next;

            long actual = (long)Bencoding.Decode(encoded, out next);
        }

        [TestMethod]
        public void TestDecodeDictionary()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("forewordd3:foo3:barepostscript");
            int start = 8;
            int next;

            var actual = (Dictionary<object, object>)Bencoding.Decode(encoded, start, out next);

            Assert.AreEqual(actual.Count, 1);

            foreach (var kvp in actual)
            {
                Assert.IsTrue(kvp.Key is byte[]);
                AssertBytewiseIdentical((byte[])kvp.Key, Bs("foo"));

                Assert.IsTrue(kvp.Value is byte[]);
                AssertBytewiseIdentical((byte[])kvp.Value, Bs("bar"));
            }

            Assert.AreEqual(next, start + 12);
        }

        [TestMethod]
        public void TestDecodeDictionaryEmpty()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("foreworddepostscript");
            int start = 8;
            int next;

            var actual = (Dictionary<object, object>)Bencoding.Decode(encoded, start, out next);

            Assert.AreEqual(actual.Count, 0);
            Assert.AreEqual(next, start + 2);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeDictionaryUnpairedKey()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("di99ee");
            int next;

            var actual = Bencoding.Decode(encoded, out next);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeDictionaryHalfOpen()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("d5:sushii85e");
            int next;

            var actual = Bencoding.Decode(encoded, out next);
        }

        [TestMethod]
        public void TestDecodeList()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("naughtyli1ei2e5:threeenice");
            int start = 7;
            int next;

            long expected1 = 1;
            long expected2 = 2;
            byte[] expected3 = Bs("three");
            var actual = (List<object>)Bencoding.Decode(encoded, start, out next);

            Assert.AreEqual(actual.Count, 3);

            Assert.IsTrue(actual[0] is long);
            Assert.AreEqual((long)actual[0], expected1);

            Assert.IsTrue(actual[1] is long);
            Assert.AreEqual((long)actual[1], expected2);

            Assert.IsTrue(actual[2] is byte[]);
            AssertBytewiseIdentical((byte[])actual[2], expected3);

            Assert.AreEqual(next, start + 15);
        }

        [TestMethod]
        public void TestDecodeListEmpty()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("naughtylenice");
            int start = 7;
            int next;

            var actual = (List<object>)Bencoding.Decode(encoded, start, out next);

            Assert.AreEqual(actual.Count, 0);
            Assert.AreEqual(next, start + 2);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeListHalfOpen()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("l4:lord4:high11:executioner");
            int next;

            var actual = Bencoding.Decode(encoded, out next);
        }
    }
}
