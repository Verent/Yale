using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class Boolean
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void Boolean_TrueIs_True()
        {
            const string value = "true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void Boolean_FalseIs_False()
        {
            const string value = "false";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsFalse((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void Boolean_TrueIsTrue_IsTrue()
        {
            const string value = "true = true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void Boolean_FalseIsFalse_True()
        {
            const string value = "false = false";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void Boolean_FalseAndFalse_False()
        {
            const string value = "false AND false";

            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsFalse((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void Boolean_FalseAndTrue_False()
        {
            const string value = "false AND true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsFalse((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void Boolean_TrueAndTrue_True()
        {
            const string value = "true AND true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void Boolean_FalseOrFalse_False()
        {
            const string value = "false OR false";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsFalse((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void Boolean_FalseOrTrue_True()
        {
            const string value = "false OR true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void Boolean_TrueOrTrue_True()
        {
            const string value = "true OR true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void Boolean_FalseXorFalse_False()
        {
            const string value = "false XOR false";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsFalse((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void Boolean_FalseXorTrue_True()
        {
            const string value = "false XOR true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void Boolean_TrueXorTrue_False()
        {
            const string value = "true XOR true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.AreEqual(false, result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.AreEqual(false, result2);
        }

        [TestMethod]
        public void Boolean_FalseNotEqualFalse_False()
        {
            const string value = "false <> false";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsFalse((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void Boolean_FalseNotEqualTrue_True()
        {
            const string value = "false <> true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsTrue((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsTrue(result2);
        }

        [TestMethod]
        public void Boolean_TrueNotEqualTrue_False()
        {
            const string value = "true <> true";
            _instance.AddExpression("a", value);
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(bool), result.GetType());
            Assert.IsFalse((bool)result);

            _instance.AddExpression<bool>("b", value);
            var result2 = _instance.GetResult<bool>("b");
            Assert.AreEqual(typeof(bool), result2.GetType());
            Assert.IsFalse(result2);
        }
    }
}