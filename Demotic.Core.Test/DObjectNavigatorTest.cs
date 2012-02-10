using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Demotic.Core.ObjectSystem;

namespace Demotic.Core.Test
{
    [TestClass]
    public class DObjectNavigatorTest
    {
        private DRecord _root = new DRecord();
        private DObject _root_n = new DNumber(3);
        private DObject _root_s = new DString("hey");
        private DRecord _root_rec = new DRecord();
        private DObject _root_rec_x = new DNumber(5);

        [TestInitialize]
        public void Initialize()
        {
            _root["n"] = _root_n;
            _root["s"] = _root_s;
            _root["rec"] = _root_rec;
            _root_rec["x"] = _root_rec_x;
        }

        private void SimpleGetTest(string path, DObject expected)
        {
            DObjectNavigator nav = new DObjectNavigator(_root);
            DObjectNavigator selector = nav.Select(path);

            Assert.AreEqual(expected, selector.Get());
        }

        private void SimplePutTest(string path)
        {
        }

        [TestMethod]
        public void TestNoSelect()
        {
            DObjectNavigator nav = new DObjectNavigator(_root);

            Assert.AreEqual(_root, nav.Get());
        }

        [TestMethod]
        public void TestGetEmptyString()
        {
            SimpleGetTest("", _root);
        }

        [TestMethod]
        public void TestGetRoot()
        {
            SimpleGetTest("/", _root);
        }

        [TestMethod]
        public void TestGetChild()
        {
            SimpleGetTest("n", _root_n);
        }

        [TestMethod]
        public void TestGetGrandchild()
        {
            SimpleGetTest("rec/x", _root_rec_x);
        }
    }
}
