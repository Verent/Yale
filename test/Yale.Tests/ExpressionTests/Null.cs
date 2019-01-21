using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class Null
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void NullCheck()
        {
            _instance.Variables.Add("a", "stringObject");
            _instance.AddExpression("b", "a = null");

            Assert.IsFalse((bool)_instance.GetResult("b"));
        }

        [TestMethod]
        public void NullIsNullCheck()
        {
            _instance.AddExpression("b", "null = null");

            Assert.IsTrue((bool)_instance.GetResult("b"));
        }
    }
}