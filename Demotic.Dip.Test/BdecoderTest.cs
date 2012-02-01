using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Demotic.Network;

namespace Demotic.Network.Test
{
    [TestClass]
    public class BdecoderTest
    {
        [TestMethod]
        public void TestDecodeBytestring()
        {
            byte[] encoded = Utils.Bs("5:magicgarbage");
            byte[] expected = Utils.Bs("magic");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(expected, d.ByteStringValue);
            Assert.AreEqual(7, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeBytestringPrematureEof()
        {
            byte[] encoded = Utils.Bs("7:overr");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.Eof, d.Next());
            Assert.AreEqual(7, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeBytestringPrematureEof2()
        {
            byte[] encoded = Utils.Bs("12");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.Eof, d.Next());
            Assert.AreEqual(2, d.NextOffset);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeBytestringSizeGarbage()
        {
            byte[] encoded = Utils.Bs("1%ab#2:interference");
            Bdecoder d = new Bdecoder(encoded);

            d.Next();
        }

        [TestMethod]
        public void TestDecodeInteger()
        {
            byte[] encoded = Utils.Bs("i427effffff");
            int expected = 427;
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.Integer, d.Next());
            Assert.AreEqual(427, d.IntegerValue);
            Assert.AreEqual(5, d.NextOffset);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeIntegerGarbage()
        {
            byte[] encoded = Utils.Bs("isawthesign");
            Bdecoder d = new Bdecoder(encoded);

            d.Next();
        }

        //[TestMethod, ExpectedException(typeof(BadBencodingException))]
        //public void TestDecodeIntegerHalfopen()
        //{
        //    byte[] encoded = Encoding.ASCII.GetBytes("i123");
        //    int next;

        //    long actual = (long)Bencoding.Decode(encoded, out next);
        //}

        //[TestMethod]
        //public void TestDecodeDictionary()
        //{
        //    byte[] encoded = Encoding.ASCII.GetBytes("forewordd3:foo3:barepostscript");
        //    int start = 8;
        //    int next;

        //    var actual = (Dictionary<object, object>)Bencoding.Decode(encoded, start, out next);

        //    Assert.AreEqual(actual.Count, 1);

        //    foreach (var kvp in actual)
        //    {
        //        Assert.IsTrue(kvp.Key is byte[]);
        //        BencodingUtils.AssertBytewiseIdentical((byte[])kvp.Key, Utils.Bs("foo"));

        //        Assert.IsTrue(kvp.Value is byte[]);
        //        BencodingUtils.AssertBytewiseIdentical((byte[])kvp.Value, Utils.Bs("bar"));
        //    }

        //    Assert.AreEqual(next, start + 12);
        //}

        //[TestMethod]
        //public void TestDecodeDictionaryEmpty()
        //{
        //    byte[] encoded = Encoding.ASCII.GetBytes("foreworddepostscript");
        //    int start = 8;
        //    int next;

        //    var actual = (Dictionary<object, object>)Bencoding.Decode(encoded, start, out next);

        //    Assert.AreEqual(actual.Count, 0);
        //    Assert.AreEqual(next, start + 2);
        //}

        //[TestMethod, ExpectedException(typeof(BadBencodingException))]
        //public void TestDecodeDictionaryUnpairedKey()
        //{
        //    byte[] encoded = Encoding.ASCII.GetBytes("di99ee");
        //    int next;

        //    var actual = Bencoding.Decode(encoded, out next);
        //}

        //[TestMethod, ExpectedException(typeof(BadBencodingException))]
        //public void TestDecodeDictionaryHalfOpen()
        //{
        //    byte[] encoded = Encoding.ASCII.GetBytes("d5:sushii85e");
        //    int next;

        //    var actual = Bencoding.Decode(encoded, out next);
        //}

        //[TestMethod]
        //public void TestDecodeList()
        //{
        //    byte[] encoded = Encoding.ASCII.GetBytes("naughtyli1ei2e5:threeenice");
        //    int start = 7;
        //    int next;

        //    long expected1 = 1;
        //    long expected2 = 2;
        //    byte[] expected3 = Utils.Bs("three");
        //    var actual = (List<object>)Bencoding.Decode(encoded, start, out next);

        //    Assert.AreEqual(actual.Count, 3);

        //    Assert.IsTrue(actual[0] is long);
        //    Assert.AreEqual((long)actual[0], expected1);

        //    Assert.IsTrue(actual[1] is long);
        //    Assert.AreEqual((long)actual[1], expected2);

        //    Assert.IsTrue(actual[2] is byte[]);
        //    BencodingUtils.AssertBytewiseIdentical((byte[])actual[2], expected3);

        //    Assert.AreEqual(next, start + 15);
        //}

        //[TestMethod]
        //public void TestDecodeListEmpty()
        //{
        //    byte[] encoded = Encoding.ASCII.GetBytes("naughtylenice");
        //    int start = 7;
        //    int next;

        //    var actual = (List<object>)Bencoding.Decode(encoded, start, out next);

        //    Assert.AreEqual(actual.Count, 0);
        //    Assert.AreEqual(next, start + 2);
        //}

        //[TestMethod, ExpectedException(typeof(BadBencodingException))]
        //public void TestDecodeListHalfOpen()
        //{
        //    byte[] encoded = Encoding.ASCII.GetBytes("l4:lord4:high11:executioner");
        //    int next;

        //    var actual = Bencoding.Decode(encoded, out next);
        //}

    }
}
