using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
        private readonly ComputeInstance instance = new();

        [TestMethod]
        public void AddExpression_WithAValue_ReturnsValue()
        {
            const int a = 1;
            instance.Variables.Add("a", a);
            int aResult = instance.Variables.Get<int>("a");
            Assert.AreEqual(a, aResult);

            instance.AddExpression("ea", "a");
            object aeResult = instance.GetResult("ea");

            Assert.AreEqual(aResult, aeResult);
        }

        [TestMethod]
        public void ChangeInt_UsedInExpressions_UpdatesExpression()
        {
            const int a = 1;
            const int aUpdated = 2;
            instance.Variables.Add("a", a);
            instance.AddExpression("ra", "a*2");
            Assert.AreEqual(a * 2, instance.GetResult<int>("ra"));

            instance.Variables["a"] = aUpdated;
            Assert.AreEqual(aUpdated * 2, instance.GetResult<int>("ra"));
        }

        [TestMethod]
        public void AddExpression_ThatAreValid_ReturnsExpectedResult()
        {
            instance.AddExpression("a", "true");
            instance.AddExpression("b", "false");

            Assert.AreEqual(2, instance.ExpressionCount);
            Assert.IsTrue(instance.GetResult<bool>("a"));
            Assert.IsFalse(instance.GetResult<bool>("b"));
        }

        [TestMethod]
        public void AddExpressionT_ThatAreValid_Exists()
        {
            instance.AddExpression<bool>("a", "true");
            instance.AddExpression<bool>("b", "false");

            Assert.AreEqual(2, instance.ExpressionCount);
            Assert.IsTrue(instance.ContainsExpression("a"));
            Assert.IsTrue(instance.ContainsExpression("b"));
        }

        [TestMethod]
        public void AddExpression_ThatAreInvalid_ThrowsException()
        {
            Assert.ThrowsException<ExpressionCompileException>(() =>
                instance.AddExpression("a", "true > false"));

            Assert.ThrowsException<ExpressionCompileException>(() =>
                instance.AddExpression("a", "Hello there < 1"));

            Assert.ThrowsException<ExpressionCompileException>(() =>
                instance.AddExpression("a", "1 == true"));
        }

        [TestMethod]
        public void Generic_GetResult_Valid()
        {
            instance.Variables.Add("a", 10);
            instance.AddExpression<int>("b", "a");
            int result = instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void GetResult_Valid()
        {
            instance.Variables.Add("a", 10);
            instance.AddExpression("b", "a");
            object result = instance.GetResult("b");
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void Generic_GetResult_DoesNotExist()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => instance.GetResult<int>("a"));
        }

        [TestMethod]
        public void GetResult_DoesNotExist()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => instance.GetResult("a"));
        }

        [TestMethod]
        public void GetExpression_ContainsExpression()
        {
            Assert.IsFalse(instance.ContainsExpression("a"));
            instance.AddExpression("a", "true");
            Assert.IsTrue(instance.ContainsExpression("a"));
        }

        [TestMethod]
        public void Generic_GetExpression_ExpressionExists_ReturnsExpression()
        {
            instance.AddExpression<bool>("a", "true");
            string expression = instance.GetExpression<bool>("a");

            Assert.AreEqual("true", expression);
        }

        [TestMethod]
        public void GetExpression_ExpressionDoesNotExists_ThrowsException()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => instance.GetExpression("a"));
            Assert.ThrowsException<KeyNotFoundException>(() => instance.GetExpression<bool>("a"));
        }

        [TestMethod]
        public void ContainsExpression_False()
        {
            bool result = instance.ContainsExpression("a");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContainsExpression_True()
        {
            instance.AddExpression<int>("a", "10");
            bool result = instance.ContainsExpression("a");
            Assert.IsTrue(result);

            instance.AddExpression("b", "10");
            result = instance.ContainsExpression("b");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Clear()
        {
            instance.AddExpression("a", "2");
            instance.AddExpression("b", "2");
            instance.AddExpression("c", "2");
            Assert.AreEqual(3, instance.ExpressionCount);
            instance.Variables.Add("va", 22);
            instance.Variables.Add("vb", 22);
            Assert.AreEqual(2, instance.Variables.Count);
            instance.Clear();
            Assert.AreEqual(0, instance.Variables.Count);
            Assert.AreEqual(0, instance.ExpressionCount);
        }

        [TestMethod]
        public void ExpressionCount_IsCorrect()
        {
            Assert.AreEqual(0, instance.ExpressionCount);
            instance.AddExpression("a", "2");
            Assert.AreEqual(1, instance.ExpressionCount);
        }

        [TestMethod]
        public void ChangedExpressions_AreUpdated()
        {
            instance.AddExpression("a", "2");
            Assert.AreEqual(2, (int)instance.GetResult("a"));
            instance.SetExpression("a", "3");
            Assert.AreEqual(3, (int)instance.GetResult("a"));
        }

        [TestMethod]
        public void Generic_ChangedExpressions_AreUpdated()
        {
            instance.AddExpression<int>("a", "2");
            Assert.AreEqual(2, (int)instance.GetResult("a"));
            instance.SetExpression<int>("a", "3");
            Assert.AreEqual(3, (int)instance.GetResult("a"));
        }
    }
}