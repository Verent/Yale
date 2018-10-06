using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;
using Yale.Expression;

// ReSharper disable MethodTooLong
// ReSharper disable once ClassTooBig
// ReSharper disable TooManyDeclarations

namespace Yale.Tests.Engine
{
    [TestClass]
    public class ComputeInstanceTests
    {
        private readonly ComputeInstance _instance;

        public ComputeInstanceTests()
        {
            _instance = new ComputeInstance();
        }

        [TestMethod]
        public void SetValue_WithoutKey_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
           {
               _instance.SetValue(null, 2);
           });
        }

        [TestMethod]
        public void GetValue_WithoutKey_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _instance.GetValue(null);
            });

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _instance.GetValue<object>(null);
            });
        }

        [TestMethod]
        public void AddValue_ThatAreValid_ReturnsExpectedResult()
        {
            const int a = 1;
            _instance.SetValue("a", a);
            var aResult = _instance.GetValue<int>("a");
            Assert.AreEqual(a, aResult);

            const double b = 1.0;
            _instance.SetValue("b", b);
            var bResult = _instance.GetValue<double>("b");
            Assert.AreEqual(b, bResult);

            const string c = "stringValue";
            _instance.SetValue("c", c);
            var cResult = _instance.GetValue<string>("c");
            Assert.AreEqual(c, cResult);

            const string d = "a > b";
            _instance.SetValue("d", d);
            var dResult = _instance.GetValue<string>("d");
            Assert.AreEqual(d, dResult);
            Assert.AreNotEqual(false, dResult);
        }

        [TestMethod]
        public void AddExpression_WithAValue_ReturnsValue()
        {
            const int a = 1;
            _instance.SetValue("a", a);
            var aResult = _instance.GetValue<int>("a");
            Assert.AreEqual(a, aResult);

            _instance.AddExpression("ea", "a");
            var aeResult = _instance.GetResult("ea");

            Assert.AreEqual(aResult, aeResult);
        }

        [TestMethod]
        public void AddExpression_ThatAreValid_ReturnsExpectedResult()
        {
            _instance.AddExpression("a", "true");
            _instance.AddExpression("b", "false");

            Assert.AreEqual(2, _instance.ExpressionCount);
            Assert.IsTrue(_instance.ContainsExpression("a"));
        }

        [TestMethod]
        public void AddExpressionT_ThatAreValid_IsReturned()
        {
            _instance.AddExpression<bool>("a", "true");
            _instance.AddExpression<bool>("b", "false");

            Assert.AreEqual(2, _instance.ExpressionCount);
            Assert.IsTrue(_instance.ContainsExpression("a"));
        }

        [TestMethod]
        public void AddExpression_ThatAreInvalid_ThrowsException()
        {
            Assert.ThrowsException<ExpressionCompileException>(() =>
                _instance.AddExpression("a", "true > false"));

            Assert.ThrowsException<ExpressionCompileException>(() =>
                _instance.AddExpression("a", "Hello there < 1"));

            Assert.ThrowsException<ExpressionCompileException>(() =>
                _instance.AddExpression("a", "1 == true"));
        }

        //[TestMethod]
        //public void AutoRecalculate_False()
        //{
        //    var instance = new ComputeInstance(new ComputeInstanceOptions
        //    {
        //        AutoRecalculate = false,
        //        LazyRecalculate = true
        //    });

        //    const string expression = "b";
        //    const string value = "a";

        //    instance.SetValue(value, true);
        //    instance.AddExpression<bool>(expression, value);

        //    Assert.AreEqual(true, _instance.GetResult(value));
        //    Assert.AreEqual(true, _instance.GetResult<bool>(value));

        //    Assert.AreEqual(true, _instance.GetResult(expression));
        //    Assert.AreEqual(true, _instance.GetResult<bool>(expression));

        //    instance.SetValue(value, false);

        //    Assert.AreEqual(false, _instance.GetValue(value));
        //    Assert.AreEqual(true, _instance.GetResult(expression));

        //    _instance.Recalculate(expression);

        //    Assert.AreEqual(instance.GetValue(value), instance.GetResult(expression));
        //}

        //[TestMethod]
        //public void AutoRecalculate_true()
        //{
        //    var instance = new ComputeInstance(new ComputeInstanceOptions
        //    {
        //        AutoRecalculate = false,
        //        LazyRecalculate = true
        //    });

        //    const string expression = "b";
        //    const string value = "a";

        //    instance.SetValue(value, true);
        //    instance.AddExpression<bool>(expression, value);

        //    Assert.AreEqual(true, _instance.GetResult(value));
        //    Assert.AreEqual(true, _instance.GetResult<bool>(value));

        //    Assert.AreEqual(true, _instance.GetResult(expression));
        //    Assert.AreEqual(true, _instance.GetResult<bool>(expression));

        //    instance.SetValue(value, false);

        //    Assert.AreEqual(false, _instance.GetValue(value));
        //    Assert.AreEqual(instance.GetValue(value), instance.GetResult(expression));
        //}

        //[TestMethod]
        //public void LazyRecalculate_False()
        //{
        //    var instance = new ComputeInstance(new ComputeInstanceOptions
        //    {
        //        AutoRecalculate = true,
        //        LazyRecalculate = false
        //    });
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void LazyRecalculate_true()
        //{
        //    var instance = new ComputeInstance(new ComputeInstanceOptions
        //    {
        //        AutoRecalculate = true,
        //        LazyRecalculate = true
        //    });
        //    Assert.IsFalse(true);
        //}

        [TestMethod]
        public void Generic_GetResult_Valid()
        {
            _instance.SetValue("a", 10);
            _instance.AddExpression<int>("b", "a");
            var result = _instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void GetResult_Valid()
        {
            _instance.SetValue("a", 10);
            _instance.AddExpression("b", "a");
            var result = _instance.GetResult("b");
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void Generic_GetResult_DoesNotExist()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => _instance.GetResult<int>("a"));
        }

        [TestMethod]
        public void GetResult_DoesNotExist()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => _instance.GetResult("a"));
        }

        //[TestMethod]
        //public void TryGetResult_DoesNotExist_ReturnFalse()
        //{
        //    var result = _instance.TryGetResult("a", out var value);

        //    Assert.IsFalse(result);
        //    Assert.IsNull(value);
        //}

        //[TestMethod]
        //public void Generic_TryGetResult_DoesNotExist_ReturnFalse()
        //{
        //    var result = _instance.TryGetResult<bool?>("a", out var value);

        //    Assert.IsFalse(result);
        //    Assert.IsNull(value);
        //}

        //[TestMethod]
        //public void Generic_TryGetResult_DoesExists_ReturnTrue()
        //{
        //    _instance.AddExpression<int>("a", "10");
        //    var result = _instance.TryGetResult<int>("a", out var value);

        //    Assert.IsTrue(result);
        //    Assert.IsNotNull(value);
        //}

        //[TestMethod]
        //public void TryGetResult_DoesExists_ReturnTrue()
        //{
        //    _instance.AddExpression("a", "10");
        //    var result = _instance.TryGetResult("a", out object value);

        //    Assert.IsTrue(result);
        //    Assert.IsNotNull(value);
        //}

        //[TestMethod]
        //public void GetExpression_ExpressionExists_ReturnsExpression()
        //{
        //    _instance.AddExpression("a", "true");
        //    var expression = _instance.GetExpression("a");

        //    Assert.AreEqual("a", expression.ExpressionText);
        //}

        [TestMethod]
        public void Generic_GetExpression_ExpressionExists_ReturnsExpression()
        {
            _instance.AddExpression<bool>("a", "true");
            var expression = _instance.GetExpression<bool>("a");

            Assert.AreEqual("true", expression.ExpressionText);
            Assert.AreEqual(typeof(bool), expression.ResultType);
        }

        //[TestMethod]
        //public void GetExpression_ExpressionExists_ThrowsException()
        //{
        //    Assert.ThrowsException<KeyNotFoundException>(() => _instance.GetExpression("a"));
        //}

        [TestMethod]
        public void Generic_GetExpression_ExpressionExists_ThrowsException()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => _instance.GetExpression<bool>("a"));
        }


        //[TestMethod]
        //public void GetDependents_Valid()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void GetDependents_Invalid()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void GetDependents_DoesNotExist()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void GetPrecedents_Valid()
        //{
        //    _instance.AddExpression("a", "10");
        //    _instance.AddExpression("b", "a");

        //    _instance.
        //}

        //[TestMethod]
        //public void GetPrecedents_Invalid()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void GetPrecedents_DoesNotExist()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void HasDependents_False()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void HasDependents_True()
        //{
        //    Assert.IsFalse(true);
        //}

        [TestMethod]
        public void ContainsExpression_False()
        {
            var result = _instance.ContainsExpression("a");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContainsExpression_True()
        {
            _instance.AddExpression<int>("a", "10");
            var result = _instance.ContainsExpression("a");
            Assert.IsTrue(result);

            _instance.AddExpression("b", "10");
            result = _instance.ContainsExpression("b");
            Assert.IsTrue(result);
        }

        //[TestMethod]
        //public void Remove_False()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void Remove_True()
        //{
        //    Assert.IsFalse(true);
        //}

        [TestMethod]
        public void Clear()
        {
            _instance.AddExpression("a", "2");
            _instance.AddExpression("b", "2");
            _instance.AddExpression("c", "2");
            Assert.AreEqual(3, _instance.ExpressionCount);
            _instance.SetValue("va", 22);
            _instance.SetValue("vb", 22);
            Assert.AreEqual(2, _instance.ValueCount);
            _instance.Clear();
            Assert.AreEqual(0, _instance.ValueCount);
            Assert.AreEqual(0, _instance.ExpressionCount);
        }

        [TestMethod]
        public void ExpressionCount_IsCorrect()
        {
            Assert.AreEqual(0, _instance.ExpressionCount);
            _instance.AddExpression("a", "2");
            Assert.AreEqual(1, _instance.ExpressionCount);

        }

        //[TestMethod]
        //public void DependencyGraph()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void Recalculate_Valid()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void Recalculate_Invalid()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void Recalculate_DoesNotExist()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void ExplainResult_Valid()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void ExplainResult_Invalid()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void ExplainResult_DoesNotExist()
        //{
        //    Assert.IsFalse(true);
        //}

        //[TestMethod]
        //public void Test_Owner_object()
        //{
        //    Assert.Fail();
        //}
    }
}