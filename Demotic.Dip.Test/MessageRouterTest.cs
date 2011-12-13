using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Demotic.Core.ObjectSystem;

namespace Demotic.Dip.Test
{
    [TestClass]
    public class MessageRouterTest
    {
        [TestMethod]
        public void TestEmptyRouter()
        {
            MessageRouter<int> router = new MessageRouter<int>();
            DNumber x = 3;

            Assert.AreEqual(router.Lookup(x), 0);
        }

        [TestMethod]
        public void TestSimpleAccept()
        {
            MessageRouter<string> router = new MessageRouter<string>();
            string success = "woo";

            router.AddVerifier(success, x => (x.Test == "blah"));

            DRecord rec = new DRecord();
            rec.Set("Test", (DString)"blah");

            Assert.AreEqual(router.Lookup(rec), success);
        }

        [TestMethod]
        public void TestSimpleReject()
        {
            MessageRouter<string> router = new MessageRouter<string>();
            string success = "woo";

            router.AddVerifier(success, x => (x.Test == "blah"));

            DRecord rec = new DRecord();
            rec.Set("Test", (DString)"doh");

            Assert.AreEqual(router.Lookup(rec), null);
        }
    }
}
