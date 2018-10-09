using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;
using Yale.Tests.Helper;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class ObjectReference
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void CallInstanceMethod()
        {
            var testObject = new TestClass(nameof(CallInstanceMethod));

            _instance.SetValue("testObject", testObject);
            _instance.AddExpression<string>("e", "testObject.GetCaller()");

            Assert.AreEqual(nameof(CallInstanceMethod), _instance.GetResult<string>("e"));
        }
    }
}