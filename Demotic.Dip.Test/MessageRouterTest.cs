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

        }
    }
}
