using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Yale.Engine;
using Yale.Tests.Helper;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class ObjectReference
    {
        private readonly ComputeInstance _instance = new();

        [TestMethod]
        public void CallInstanceMethod()
        {
            TestClass<string> testObject = new(nameof(CallInstanceMethod));

            _instance.Variables.Add("testObject", testObject);
            _instance.AddExpression<string>("e", "testObject.GetCaller()");

            Assert.AreEqual(nameof(CallInstanceMethod), _instance.GetResult<string>("e"));
        }

        [TestMethod]
        public void CallInstanceMethod_2()
        {
            _instance.Variables.Add("rand", new Random());
            _instance.AddExpression("e", "rand.NextDouble() + 100");

            Assert.IsInstanceOfType((double)_instance.GetResult("e"), typeof(double));
        }
    }
}