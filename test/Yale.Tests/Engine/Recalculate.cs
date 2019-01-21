using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;
using Yale.Tests.Helper;

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
            _autoInstance.Variables.Add("a", 10);
            _autoInstance.AddExpression<int>("b", "a");

            _autoInstance.Variables["a"] = 20;
            var result = _autoInstance.GetResult("b");

            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void AutoRecalculate_ValueUpdatedDependentExpression_ReturnUpdatedValue()
        {
            _autoInstance.Variables.Add("a", 10);
            _autoInstance.AddExpression<int>("b", "a");
            _autoInstance.AddExpression<int>("c", "b");

            _autoInstance.Variables["a"] = 20;
            var result = _autoInstance.GetResult("c");

            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void AutoRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue()
        {
            var testObject = new TestClass(nameof(AutoRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue));
            var expected = "a string";

            testObject.Value = expected;

            _autoInstance.Variables["o"] = testObject;
            _autoInstance.AddExpression<string>("e", "o.Value");

            Assert.AreEqual(expected, _autoInstance.GetResult("e"));

            expected = "a new string";
            testObject.Value = expected;

            Assert.AreEqual(expected, _autoInstance.GetResult("e"));
        }

        [TestMethod]
        public void LazyRecalculate_ValueUpdated_ReturnUpdatedValue()
        {
            _lazyInstance.Variables.Add("a", 10);
            _lazyInstance.AddExpression<int>("b", "a");

            _lazyInstance.Variables["a"] = 20;
            var result = _lazyInstance.GetResult("b");

            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void LazyRecalculate_ValueUpdatedDependentExpression_ReturnUpdatedValue()
        {
            _lazyInstance.Variables.Add("a", 10);
            _lazyInstance.AddExpression<int>("b", "a");
            _lazyInstance.AddExpression<int>("c", "b");

            _lazyInstance.Variables["a"] = 20;
            var result = _lazyInstance.GetResult("c");

            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void LazyRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue()
        {
            var testObject = new TestClass(nameof(LazyRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue));
            var expected = "a string";

            testObject.Value = expected;

            _lazyInstance.Variables["o"] = testObject;
            _lazyInstance.AddExpression<string>("e", "o.Value");

            Assert.AreEqual(expected, _lazyInstance.GetResult("e"));

            expected = "a new string";
            testObject.Value = expected;

            Assert.AreEqual(expected, _lazyInstance.GetResult("e"));
        }

        [TestMethod]
        public void NoRecalculate_ValueUpdated_ReturnStartValue()
        {
            _noRecalculateInstance.Variables.Add("a", 10);
            _noRecalculateInstance.AddExpression<int>("b", "a");

            _noRecalculateInstance.Variables["a"] = 20;
            var result = _noRecalculateInstance.GetResult("b");

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void NoRecalculate_ValueUpdatedDependentExpression_ReturnStartValue()
        {
            _noRecalculateInstance.Variables.Add("a", 10);
            _noRecalculateInstance.AddExpression<int>("b", "a");
            _noRecalculateInstance.AddExpression<int>("c", "b");

            _noRecalculateInstance.Variables["a"] = 20;
            var result = _noRecalculateInstance.GetResult("c");

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void NoRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue()
        {
            var testObject = new TestClass(nameof(NoRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue));
            var expected = "a string";

            testObject.Value = expected;

            _noRecalculateInstance.Variables.Add("o", testObject);
            _noRecalculateInstance.AddExpression<string>("e", "o.Value");

            Assert.AreEqual(expected, _noRecalculateInstance.GetResult("e"));

            testObject.Value = "a new string";
            Assert.AreEqual(expected, _noRecalculateInstance.GetResult("e"));
        }
    }
}