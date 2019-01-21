using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Yale.Engine;
using Yale.Expression;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class Identifier
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void AddIntExpressionToIntExpressionGeneric_ReturnSum()
        {
            _instance.AddExpression<int>("a", "1");
            var result = _instance.GetResult<int>("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(1, result);

            _instance.AddExpression<int>("b", "2");
            var result2 = _instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(2, result2);

            _instance.AddExpression<int>("c", "a + b");
            var result3 = _instance.GetResult<int>("c");
            Assert.AreEqual(typeof(int), result3.GetType());
            Assert.AreEqual(3, result3);
        }

        [TestMethod]
        public void AddIntExpressionToIntExpression_ReturnSum()
        {
            _instance.AddExpression("a", "1");
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(1, result);

            _instance.AddExpression("b", "2");
            var result2 = _instance.GetResult("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(2, result2);

            _instance.AddExpression("c", "a + b");
            var result3 = _instance.GetResult("c");
            Assert.AreEqual(typeof(int), result3.GetType());
            Assert.AreEqual(3, result3);
        }

        [TestMethod]
        public void Flee_Basic()
        {
            _instance.Variables.Add("x", 100);
            _instance.AddExpression("a", "x * 2");
            _instance.Variables.Add("y", 1);
            _instance.AddExpression("b", "a + y");
            _instance.AddExpression("c", "b * 2");

            var result = _instance.GetResult<int>("c");
            Assert.AreEqual(result, (100 * 2 + 1) * 2);

            _instance.Variables["x"] = 345;
            result = _instance.GetResult<int>("c");
            Assert.AreEqual((345 * 2 + 1) * 2, result);
        }

        [TestMethod]
        public void Flee_MultipleIdentical_References()
        {
            _instance.Variables.Add("x", 100);
            _instance.AddExpression("a", "x * 2");
            _instance.AddExpression("b", "a + a + a");
            var result = _instance.GetResult<int>("b");
            Assert.AreEqual(100 * 2 * 3, result);
        }

        [TestMethod]
        public void Flee_Complex()
        {
            _instance.Variables.Add("x", 100);
            _instance.AddExpression("a", "x * 2");
            _instance.Variables.Add("y", 24);
            _instance.AddExpression("b", "y * 2");
            _instance.AddExpression("c", "a + b");
            _instance.AddExpression("d", "80");
            _instance.AddExpression("e", "a + b + c + d");

            var result = _instance.GetResult<int>("e");
            Assert.AreEqual((100 * 2) + (24 * 2) + ((100 * 2) + (24 * 2)) + 80, result);
        }

        [TestMethod]
        public void Flee_Arithmetic()
        {
            _instance.Variables.Add("a", 10);
            _instance.Variables.Add("b", 20);
            _instance.AddExpression("x", "((a * 2) + (b ^ 2)) - (100 % 5)");

            var result = _instance.GetResult<int>("x");
            Assert.AreEqual(420, result);
        }

        [TestMethod]
        public void Flee_Comparison_Operators()
        {
            _instance.Variables.Add("a", 10);
            _instance.AddExpression("x", "a <> 100");
            var result = _instance.GetResult<bool>("x");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Flee_And_Or_Xor_Not_Operators()
        {
            _instance.Variables.Add("a", 10);
            _instance.AddExpression("x", "a > 100");

            var result = _instance.GetResult<bool>("x");
            Assert.IsFalse(result);

            _instance.Variables.Add("b", 100);
            _instance.SetExpression("x", "b = 100");

            result = _instance.GetResult<bool>("x");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Flee_Shift_Operators()
        {
            _instance.AddExpression("x", "100 >> 2");
            var result = _instance.GetResult<int>("x");
            Assert.AreEqual(25, result);
        }

        [TestMethod]
        public void Flee_Recalculate_NonSource()
        {
            _instance.Variables.Add("x", 10);
            _instance.AddExpression("a", "x * 2");
            _instance.Variables.Add("y", 1);
            _instance.AddExpression("b", "a + y");
            _instance.AddExpression("c", "b * 2");
            _instance.Variables["x"] = 100;

            var result = _instance.GetResult<int>("c");
            Assert.AreEqual((100 * 2 + 1) * 2, result);
        }

        [TestMethod]
        public void Flee_Partial_Recalculate()
        {
            _instance.Variables.Add("x", 100);
            _instance.AddExpression("a", "x * 2");
            _instance.Variables.Add("y", 1);
            _instance.AddExpression("b", "a + y");
            _instance.AddExpression("c", "b * 2");
            _instance.Variables["y"] = 222;

            var result = _instance.GetResult<int>("c");
            Assert.AreEqual((100 * 2 + 222) * 2, result);
        }

        [TestMethod]
        public void Self_Reference1()
        {
            _instance.Variables.Add("x", 100);
            _instance.AddExpression("a", "x * 2");
            _instance.Variables.Add("y", 1);

            Assert.ThrowsException<ExpressionCompileException>(() => { _instance.AddExpression("b", "a + y + b"); });
        }

        [TestMethod]
        public void Flee_Boolean_Expression()
        {
            const string expression = "a AND NOT b AND NOT c AND d";
            var expressionVariables = new Dictionary<string, object> { { "a", 1 }, { "b", 0 }, { "c", 0 }, { "d", 1 } };

            foreach (var expressionVariable in expressionVariables.Keys)
            {
                _instance.Variables.Add(expressionVariable, expressionVariables[expressionVariable]);
            }

            _instance.AddExpression("e", expression);
            var result = _instance.GetResult("e");

            Assert.AreEqual(1, result);

            _instance.Variables["a"] = 0;
            var result2 = _instance.GetResult("e");
            Assert.AreEqual(0, result2);
        }
    }
}