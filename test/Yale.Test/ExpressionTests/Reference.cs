using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Test.ExpressionTests
{
    [TestClass]
    public class Reference
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void AddIntExpressionToIntExpressionGeneric_ReturnSum()
        {
            _instance.AddExpression<int>("a", "1");
            var result = _instance.GetResult<int>("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(1, result);

            _instance.AddExpression<int>("b", "2");
            var result2 = _instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(2, result2);

            _instance.AddExpression<int>("c", "a + b");
            var result3 = _instance.GetResult<int>("c");
            Assert.AreEqual(typeof(int), result3.GetType());
            Assert.AreEqual(3, result3);
        }

        [TestMethod]
        public void AddIntExpressionToIntExpression_ReturnSum()
        {
            _instance.AddExpression("a", "1");
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(1, result);

            _instance.AddExpression("b", "2");
            var result2 = _instance.GetResult("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(2, result2);

            _instance.AddExpression("c", "a + b");
            var result3 = _instance.GetResult("c");
            Assert.AreEqual(typeof(int), result3.GetType());
            Assert.AreEqual(3, result3);
        }
    }
}
