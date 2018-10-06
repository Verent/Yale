using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class Cast
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void CastToInt()
        {
            _instance.AddExpression("cast", "cast(100.25; int)");

            Assert.AreEqual(100, _instance.GetResult("cast"));
        }


        [TestMethod]
        public void PowerFloatVariable()
        {
            _instance.SetValue("a", 4.0);
            _instance.AddExpression("b", "a^2");

            Assert.AreEqual(16.0, (double)_instance.GetResult("b"));
        }
    }
}