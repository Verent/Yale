using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.Engine
{
    [TestClass]
    public class Recalculate
    {
        private readonly ComputeInstance _autoRecalculateInstance = new ComputeInstance(new ComputeInstanceOptions
        {
            AutoRecalculate = true
        });

        private readonly ComputeInstance _lazyRecalculateInstance = new ComputeInstance(new ComputeInstanceOptions
        {
            AutoRecalculate = true,
            LazyRecalculate = true
        });

        private readonly ComputeInstance _noRecalculateInstance = new ComputeInstance(new ComputeInstanceOptions
        {
            AutoRecalculate = false,
            LazyRecalculate = false
        });

        [TestMethod]
        public void AutoRecalculate_ValueUpdated_ReturnsUpdatedValue()
        {
            _autoRecalculateInstance.SetValue("a", 10);
            _autoRecalculateInstance.AddExpression<int>("b", "a");
            
            _autoRecalculateInstance.SetValue("a", 20);
            var result = _autoRecalculateInstance.GetResult("b");

            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void AutoRecalculate_ValueUpdatedDependentExpression_ReturnsUpdatedValue()
        {
            _autoRecalculateInstance.SetValue("a", 10);
            _autoRecalculateInstance.AddExpression<int>("b", "a");
            _autoRecalculateInstance.AddExpression<int>("c", "b");

            _autoRecalculateInstance.SetValue("a", 20);
            var result = _autoRecalculateInstance.GetResult("c");

            Assert.AreEqual(20, result);
        }


        [TestMethod]
        public void LazyRecalculate_ValueUpdated_ReturnsUpdatedValue()
        {
            _lazyRecalculateInstance.SetValue("a", 10);
            _lazyRecalculateInstance.AddExpression<int>("b", "a");

            _lazyRecalculateInstance.SetValue("a", 20);
            var result = _lazyRecalculateInstance.GetResult("b");

            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void LazyRecalculate_ValueUpdatedDependentExpression_ReturnsUpdatedValue()
        {
            _lazyRecalculateInstance.SetValue("a", 10);
            _lazyRecalculateInstance.AddExpression<int>("b", "a");
            _lazyRecalculateInstance.AddExpression<int>("c", "b");

            _lazyRecalculateInstance.SetValue("a", 20);
            var result = _lazyRecalculateInstance.GetResult("c");

            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void NoRecalculate_ValueUpdated_ReturnsStartValue()
        {
            _noRecalculateInstance.SetValue("a", 10);
            _noRecalculateInstance.AddExpression<int>("b", "a");

            _noRecalculateInstance.SetValue("a", 20);
            var result = _noRecalculateInstance.GetResult("b");

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void NoRecalculate_ValueUpdatedDependentExpression_ReturnsStartValue()
        {
            _noRecalculateInstance.SetValue("a", 10);
            _noRecalculateInstance.AddExpression<int>("b", "a");
            _noRecalculateInstance.AddExpression<int>("c", "b");

            _noRecalculateInstance.SetValue("a", 20);
            var result = _noRecalculateInstance.GetResult("c");

            Assert.AreEqual(10, result);
        }
    }
}