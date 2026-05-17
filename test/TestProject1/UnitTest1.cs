using test;
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
            int[] num = { 1, 2, 3, 4, 5 };
            double ex = 3;
            double act = Calc.Avg(num);

            Assert.AreEqual(ex, act);
        }
    }
}