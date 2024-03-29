﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Demotic.Core.ObjectSystem;
using Demotic.Network;

using W3b.Sine;

namespace Demotic.Network.Test
{
    [TestClass]
    public class BencoderTest
    {
        [TestMethod]
        public void TestEncodeEmpty()
        {
            Bencoder e = new Bencoder();

            Assert.AreEqual(e.Encoded.Length, 0);
        }

        private void AssertEncodesTo(Action<Bencoder> encodingSteps, string expected)
        {
            byte[] expectedBytes = Encoding.ASCII.GetBytes(expected);

            Bencoder e = new Bencoder();
            encodingSteps.Invoke(e);

            byte[] actualBytes = e.Encoded;

            Utils.AssertBytewiseIdentical(expectedBytes, actualBytes);
        }

        [TestMethod]
        public void TestEncodeInteger()
        {
            AssertEncodesTo((e => e.Integer(42)), "i42e");
        }

        [TestMethod]
        public void TestEncodeIntegerNegative()
        {
            AssertEncodesTo((e => e.Integer(-5)), "i-5e");
        }

        [TestMethod]
        public void TestEncodeString()
        {
            AssertEncodesTo((e => e.ByteString(Utils.Bs("test"))), "4:test");
        }

        [TestMethod]
        public void TestEncodeStringEmpty()
        {
            AssertEncodesTo((e => e.ByteString(Utils.Bs(""))), "0:");
        }

        [TestMethod]
        public void TestEncodeList()
        {
            AssertEncodesTo(
                (e => e.StartList().
                            ByteString(Utils.Bs("spam")).
                            Integer(12).
                            ByteString(Utils.Bs("eggs")).
                        FinishList()),
                "l4:spami12e4:eggse"
                );
        }

        [TestMethod]
        public void TestEncodeListEmpty()
        {
            AssertEncodesTo((e => e.StartList().FinishList()), "le");
        }

        [TestMethod]
        public void TestEncodeDictionary()
        {
            AssertEncodesTo(
                (e => e.StartDictionary().
                            ByteString(Utils.Bs("what")).ByteString(Utils.Bs("delicious")).
                            ByteString(Utils.Bs("answer")).Integer(42).
                            ByteString(Utils.Bs("time")).ByteString(Utils.Bs("now")).
                        FinishDictionary()),
                "d4:what9:delicious6:answeri42e4:time3:nowe"
                );
        }

        [TestMethod]
        public void TestEncodeDictionaryEmpty()
        {
            AssertEncodesTo((e => e.StartDictionary().FinishDictionary()), "de");
        }

        [TestMethod]
        public void TestEncodeDNumber()
        {
            BigNum n = BigFloatFactory.Instance.Create(123.45);

            AssertEncodesTo(
                (e => e.StartDNumber().Number(n).FinishDNumber()), 
                "Nn123.45ee"
                );
        }

        [TestMethod]
        public void TestEncodeDString()
        {
            AssertEncodesTo(
                (e => e.StartDString().ByteString(Utils.Bs("blah")).FinishDString()),
                "S4:blahe"
                );
        }

        [TestMethod]
        public void TestEncodeDRecord()
        {
            AssertEncodesTo(
                (e => e.StartDRecord().
                            ByteString(Utils.Bs("a")).ByteString(Utils.Bs("b")).
                        FinishDRecord()),
                "R1:a1:be"
                );
        }
    }
}
