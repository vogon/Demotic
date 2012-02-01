using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Demotic.Network.Test
{
    internal static class Utils
    {
        public static void AssertBytewiseIdentical(byte[] a, byte[] b)
        {
            Assert.AreEqual(a.Length, b.Length,
                "encodings have different lengths ({0} != {1})", a.Length, b.Length);

            for (int i = 0; i < a.Length; i++)
            {
                Assert.AreEqual(a[i], b[i],
                    "miscompare at byte {0} ({1} != {2})", i, a[i], b[i]);
            }
        }

        public static byte[] Bs(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }
    }
}
