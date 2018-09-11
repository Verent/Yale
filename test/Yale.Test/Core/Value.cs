using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Test.Core
{
    [TestClass]
    public class Recalculate
    {
        private readonly ComputeInstance _instance = new ComputeInstance(new ComputeInstanceOptions
        {
            AutoRecalculate = true
        });


        [TestMethod]
        public void Core_ValueUpdated_ReturnsUpdatedValue()
        {
            _instance.SetValue("a", 10);
            _instance.AddExpression<int>("b", "a");
            _instance.SetValue("a", 20);

            var result = _instance.GetResult("b");

            Assert.AreEqual(20, result);
        }
    }
}