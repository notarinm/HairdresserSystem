using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ConsoleApp2;

namespace Test2
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test1()
        {
            string text = "A123A";
            int index = 0;

            string result = Class1.LenPos(text, index);

            Assert.AreEqual("A123A", result);
        }

        [TestMethod]
        public void Test2()
        {
            string text = "XYZB456BMNO";
            int index = 3; 

            string result = Class1.LenPos(text, index);

            Assert.AreEqual("B456B", result);
        }

        [TestMethod]
        public void Test3()
        {
            string text = "A123B"; 
            int index = 0;

            string result = Class1.LenPos(text, index);


            Assert.AreEqual(string.Empty, result);
        }

    }
}
