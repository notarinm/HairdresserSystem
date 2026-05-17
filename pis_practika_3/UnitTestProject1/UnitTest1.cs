using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ConsoleApp1;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test1()
        {
            string text = "A123A";
            int expected = 0; 

            int result = TextFind.TxAnalyzer.FindLongest(text);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test2()
        {
            string text = "B12BC345C";
            int expected = 4;

            int result = TextFind.TxAnalyzer.FindLongest(text);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test3()
        {
            string text = "X11XY22Y";
            int expected = 0;

            int result = TextFind.TxAnalyzer.FindLongest(text);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test4()
        {
            string text = "ABC123XYZ";
            int expected = -1;

            int result = TextFind.TxAnalyzer.FindLongest(text);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Test5()
        {
            string text = "1234567890";
            int expected = -1;

            int result = TextFind.TxAnalyzer.FindLongest(text);

            Assert.AreEqual(expected, result);
        }
    }
}
