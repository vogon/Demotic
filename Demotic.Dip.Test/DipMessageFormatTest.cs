using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Demotic.Core.ObjectSystem;

namespace Demotic.Network.Test
{
    [TestClass]
    public class DipMessageFormatTest
    {
        // TODO: make a unit-testier test for these things.

        [TestMethod]
        public void TestSerializeDNumber()
        {
            DipMessageFormat_Accessor fmt = new DipMessageFormat_Accessor();
            Bencoder_Accessor e = new Bencoder_Accessor();
            DNumber n = new DNumber(123.45m);

            fmt.SerializeDNumber(e, n);
            Utils.AssertBytewiseIdentical(Utils.Bs("Nn123.45ee"), e.Encoded);
        }

        [TestMethod]
        public void TestSerializeDString()
        {
            DipMessageFormat_Accessor fmt = new DipMessageFormat_Accessor();
            Bencoder_Accessor e = new Bencoder_Accessor();
            DString s = "blah";

            fmt.SerializeDString(e, s);
            Utils.AssertBytewiseIdentical(Utils.Bs("S4:blahe"), e.Encoded);
        }

        [TestMethod]
        public void TestSerializeDRecord()
        {
            DipMessageFormat_Accessor fmt = new DipMessageFormat_Accessor();
            Bencoder_Accessor e = new Bencoder_Accessor();

            DRecord rec = new DRecord();
            rec["foo"] = (DString)"bar";

            fmt.SerializeDRecord(e, rec);
            Utils.AssertBytewiseIdentical(Utils.Bs("R3:fooS3:baree"), e.Encoded);
        }
    }
}
