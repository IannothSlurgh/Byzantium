using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using Byzantium;

namespace ByzantiumUnitTests
{
    [TestClass]
    public class TestSortedAddressList
    {
        [TestMethod]
        public void SortingRandom1()
        {
            SortedAddressList sal = new SortedAddressList(10);
            byte[] ab = new byte[4];
            ab[3] = 92;
            byte[] bb = new byte[4];
            bb[3] = 153;
            byte[] cb = new byte[4];
            cb[3] = 120;
            IPAddress a = new IPAddress(ab);
            IPAddress b = new IPAddress(bb);
            IPAddress d = new IPAddress(cb);
            sal.add(a);
            sal.add(b);
            sal.add(d);
            Assert.AreEqual(sal[0].ToString(), "0.0.0.92");
            Assert.AreEqual(sal[1].ToString(), "0.0.0.120");
            Assert.AreEqual(sal[2].ToString(), "0.0.0.153");
        }

        [TestMethod]
        public void SortingRandom2()
        {
            SortedAddressList sal = new SortedAddressList(10);
            byte[] ab = new byte[4];
            ab[3] = 153;
            byte[] bb = new byte[4];
            bb[3] = 92;
            byte[] cb = new byte[4];
            cb[3] = 120;
            IPAddress a = new IPAddress(ab);
            IPAddress b = new IPAddress(bb);
            IPAddress d = new IPAddress(cb);
            sal.add(a);
            sal.add(b);
            sal.add(d);
            Assert.AreEqual(sal[0].ToString(), "0.0.0.92");
            Assert.AreEqual(sal[1].ToString(), "0.0.0.120");
            Assert.AreEqual(sal[2].ToString(), "0.0.0.153");
        }

        [TestMethod]
        public void SortingRandom3()
        {
            SortedAddressList sal = new SortedAddressList(10);
            byte[] ab = new byte[4];
            ab[3] = 120;
            byte[] bb = new byte[4];
            bb[3] = 153;
            byte[] cb = new byte[4];
            cb[3] = 92;
            IPAddress a = new IPAddress(ab);
            IPAddress b = new IPAddress(bb);
            IPAddress d = new IPAddress(cb);
            sal.add(a);
            sal.add(b);
            sal.add(d);
            Assert.AreEqual(sal[0].ToString(), "0.0.0.92");
            Assert.AreEqual(sal[1].ToString(), "0.0.0.120");
            Assert.AreEqual(sal[2].ToString(), "0.0.0.153");
        }

        [TestMethod]
        public void SortingRandom4()
        {
            SortedAddressList sal = new SortedAddressList(10);
            byte[] ab = new byte[4];
            ab[3] = 120;
            byte[] bb = new byte[4];
            bb[3] = 92;
            byte[] cb = new byte[4];
            cb[3] = 153;
            IPAddress a = new IPAddress(ab);
            IPAddress b = new IPAddress(bb);
            IPAddress d = new IPAddress(cb);
            sal.add(a);
            sal.add(b);
            sal.add(d);
            Assert.AreEqual(sal[0].ToString(), "0.0.0.92");
            Assert.AreEqual(sal[1].ToString(), "0.0.0.120");
            Assert.AreEqual(sal[2].ToString(), "0.0.0.153");
        }

        [TestMethod]
        public void SortingInOrder()
        {
            SortedAddressList sal = new SortedAddressList(10);
            byte[] ab = new byte[4];
            ab[3] = 92;
            byte[] bb = new byte[4];
            bb[3] = 120;
            byte[] cb = new byte[4];
            cb[3] = 153;
            IPAddress a = new IPAddress(ab);
            IPAddress b = new IPAddress(bb);
            IPAddress d = new IPAddress(cb);
            sal.add(a);
            sal.add(b);
            sal.add(d);
            Assert.AreEqual(sal[0].ToString(), "0.0.0.92");
            Assert.AreEqual(sal[1].ToString(), "0.0.0.120");
            Assert.AreEqual(sal[2].ToString(), "0.0.0.153");
        }

        [TestMethod]
        public void SortingReverse()
        {
            SortedAddressList sal = new SortedAddressList(10);
            byte[] ab = new byte[4];
            ab[3] = 153;
            byte[] bb = new byte[4];
            bb[3] = 120;
            byte[] cb = new byte[4];
            cb[3] = 92;
            IPAddress a = new IPAddress(ab);
            IPAddress b = new IPAddress(bb);
            IPAddress d = new IPAddress(cb);
            sal.add(a);
            sal.add(b);
            sal.add(d);
            Assert.AreEqual(sal[0].ToString(), "0.0.0.92");
            Assert.AreEqual(sal[1].ToString(), "0.0.0.120");
            Assert.AreEqual(sal[2].ToString(), "0.0.0.153");
        }

    }
}
