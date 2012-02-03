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
        }

        [TestMethod]
        public void TestDecodeBytestringPrematureEof2()
        {
            byte[] encoded = Utils.Bs("12");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.Eof, d.Next());
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
            Assert.AreEqual(expected, d.IntegerValue);
            Assert.AreEqual(5, d.NextOffset);
        }

        [TestMethod, ExpectedException(typeof(BadBencodingException))]
        public void TestDecodeIntegerGarbage()
        {
            byte[] encoded = Utils.Bs("isawthesign");
            Bdecoder d = new Bdecoder(encoded);

            d.Next();
        }

        [TestMethod]
        public void TestDecodeIntegerHalfopen()
        {
            byte[] encoded = Utils.Bs("i123");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.Eof, d.Next());
        }

        [TestMethod]
        public void TestDecodeDictionary()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("d3:foo3:barepostscript");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartDictionary, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(Utils.Bs("foo"), d.ByteStringValue);

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(Utils.Bs("bar"), d.ByteStringValue);

            Assert.AreEqual(Bdecoder.NodeType.EndDictionary, d.Next());
            Assert.AreEqual(12, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeDictionaryEmpty()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("depostscript");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartDictionary, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.EndDictionary, d.Next());
            Assert.AreEqual(2, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeDictionaryHalfOpen()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("d5:sushii85e");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartDictionary, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(Utils.Bs("sushi"), d.ByteStringValue);

            Assert.AreEqual(Bdecoder.NodeType.Integer, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.Eof, d.Next());
        }

        [TestMethod]
        public void TestDecodeDNumber()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("Nn123.45eecomplex");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartDNumber, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.Number, d.Next());
            Assert.AreEqual(123.45m, d.NumberValue);

            Assert.AreEqual(Bdecoder.NodeType.EndDNumber, d.Next());
            Assert.AreEqual(10, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeDString()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("S4:testesisyphean");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartDString, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(Utils.Bs("test"), d.ByteStringValue);

            Assert.AreEqual(Bdecoder.NodeType.EndDString, d.Next());
            Assert.AreEqual(8, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeDStringEmpty()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("S0:e");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartDString, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(Utils.Bs(""), d.ByteStringValue);

            Assert.AreEqual(Bdecoder.NodeType.EndDString, d.Next());
            Assert.AreEqual(4, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeDRecord()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("R4:safe6:secureeoffthe");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartDRecord, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(Utils.Bs("safe"), d.ByteStringValue);

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(Utils.Bs("secure"), d.ByteStringValue);

            Assert.AreEqual(Bdecoder.NodeType.EndDRecord, d.Next());
            Assert.AreEqual(16, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeDRecordEmpty()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("Re");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartDRecord, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.EndDRecord, d.Next());
            Assert.AreEqual(2, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeMetadata()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("m4:data4:metae");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartMetadata, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(Utils.Bs("data"), d.ByteStringValue);

            Assert.AreEqual(Bdecoder.NodeType.ByteString, d.Next());
            Utils.AssertBytewiseIdentical(Utils.Bs("meta"), d.ByteStringValue);

            Assert.AreEqual(Bdecoder.NodeType.EndMetadata, d.Next());
            Assert.AreEqual(14, d.NextOffset);
        }

        [TestMethod]
        public void TestDecodeMetadataEmpty()
        {
            byte[] encoded = Encoding.ASCII.GetBytes("me");
            Bdecoder d = new Bdecoder(encoded);

            Assert.AreEqual(Bdecoder.NodeType.StartMetadata, d.Next());

            Assert.AreEqual(Bdecoder.NodeType.EndMetadata, d.Next());
            Assert.AreEqual(2, d.NextOffset);
        }

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
