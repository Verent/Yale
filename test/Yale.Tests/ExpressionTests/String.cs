using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Test.ExpressionTests
{
    [TestClass]
    public class String
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void String_AddExpression_ReturnCorrect()
        {
            _instance.AddExpression("a", "\"hi\"");
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(string), result.GetType());
            Assert.AreEqual("hi", result);

            _instance.AddExpression<string>("b", "\"hi\"");
            
            var result2 = _instance.GetResult<string>("b");
            Assert.AreEqual(typeof(string), result2.GetType());
            Assert.AreEqual("hi", result2);
        }
    }
}
