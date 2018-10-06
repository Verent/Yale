using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.Engine
{
    [TestClass]
    public class Recalculate
    {
        private readonly ComputeInstance _autoInstance = new ComputeInstance(new ComputeInstanceOptions
        {
            AutoRecalculate = true
        });

        private readonly ComputeInstance _lazyInstance = new ComputeInstance(new ComputeInstanceOptions
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
            _autoInstance.SetValue("a", 10);
            _autoInstance.AddExpression<int>("b", "a");
            
            _autoInstance.SetValue("a", 20);
            var result = _autoInstance.GetResult("b");

            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void AutoRecalculate_ValueUpdatedDependentExpression_ReturnsUpdatedValue()
        {
            _autoInstance.SetValue("a", 10);
            _autoInstance.AddExpression<int>("b", "a");
            _autoInstance.AddExpression<int>("c", "b");

            _autoInstance.SetValue("a", 20);
            var result = _autoInstance.GetResult("c");

            Assert.AreEqual(20, result);
        }


        [TestMethod]
        public void LazyRecalculate_ValueUpdated_ReturnsUpdatedValue()
        {
            _lazyInstance.SetValue("a", 10);
            _lazyInstance.AddExpression<int>("b", "a");

            _lazyInstance.SetValue("a", 20);
            var result = _lazyInstance.GetResult("b");

            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void LazyRecalculate_ValueUpdatedDependentExpression_ReturnsUpdatedValue()
        {
            _lazyInstance.SetValue("a", 10);
            _lazyInstance.AddExpression<int>("b", "a");
            _lazyInstance.AddExpression<int>("c", "b");

            _lazyInstance.SetValue("a", 20);
            var result = _lazyInstance.GetResult("c");

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