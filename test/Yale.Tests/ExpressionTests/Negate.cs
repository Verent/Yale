using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class Negate
    {
        private readonly ComputeInstance instance = new ComputeInstance();

        [TestMethod]
        public void Negate_PositiveInteger_IsNegative()
        {
            instance.Variables.Add("a", 10);
            instance.AddExpression("ae", "-a");
            instance.AddExpression("be", "-5");

            Assert.AreEqual(-10, instance.GetResult<int>("ae"));
            Assert.AreEqual(-5, instance.GetResult<int>("be"));
        }

        [TestMethod]
        public void Negate_NegativeInteger_IsPositive()
        {
            instance.Variables.Add("a", -10);
            instance.AddExpression("ae", "-a");
            instance.AddExpression("be", "--5");
            instance.AddExpression("ce", "-(-5)");

            Assert.AreEqual(10, instance.GetResult<int>("ae"));
            Assert.AreEqual(5, instance.GetResult<int>("be"));
            Assert.AreEqual(5, instance.GetResult<int>("ce"));
        }
    }
}
