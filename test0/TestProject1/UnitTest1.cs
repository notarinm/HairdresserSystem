using System.Runtime.InteropServices.Marshalling;
using test0;

namespace TestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public void Test2()
        {
            int[] num = { 1, 2, 3 };
            double ans = 2;
            double exx = Calc.numAvg(num);

            Assert.AreEqual(exx, ans);

        }
    }
}