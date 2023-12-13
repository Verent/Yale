using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class Operator
    {
        private readonly ComputeInstance _instance = new();

        [TestMethod]
        public void BitShift()
        {
            _instance.AddExpression("a", "0x80 >> 2");
            Assert.AreEqual(32, _instance.GetResult<int>("a"));
        }

        [TestMethod]
        public void ComparisonEq()
        {
            _instance.AddExpression("a", "2 = 2");
            Assert.IsTrue(_instance.GetResult<bool>("a"));

            _instance.AddExpression("b", "2 eq 2");
            Assert.IsTrue(_instance.GetResult<bool>("b"));
        }
    }
}
