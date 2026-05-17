using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using test0;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            int[] num = { 1, 2, 3 };
            double ex = 2;
            double actusl = Calc.numAvg(num);

            Assert.AreEqual(ex, actusl);
        }
    }
}
