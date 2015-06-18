using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Byzantium;

namespace ByzantiumUnitTests
{
    [TestClass]
    public class TestCommunicationNode
    {
        [TestMethod]
        public void TestMethod1()
        {
            CommunicationNode test_node = CommunicationNode.get_instance(12000, 12000);
            test_node.node_is_ok();
        }
        [TestMethod]
        public void TestMethod2()
        {
            int var = 0x7;
            int remove = 0x2;
            Assert.AreEqual(NodeErrors.toggleErr(var, remove), 0x5);
        }
    }
}
