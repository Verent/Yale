using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class Integer
    {
        private readonly ComputeInstance _instance = new();

        [TestMethod]
        public void IntegerEqualsInteger_IsInteger()
        {
            _instance.AddExpression("a", "1");
            object result = _instance.GetResult("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(1, result);

            _instance.AddExpression<int>("b", "2");
            int result2 = _instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(2, result2);
        }

        [TestMethod]
        [DataTestMethod]
        [DataRow("1", "+", "1", 2)]
        [DataRow("1", "-", "1", 0)]
        [DataRow("2", "*", "2", 4)]
        [DataRow("2", "/", "2", 1)]
        [DataRow("10", "/", "3", 3)]
        [DataRow("10", "%", "3", 1)]
        [DataRow("10", "/", "3.0", 10 / 3.0)]
        [DataRow("10", "*", "3.0", 30.0)]
        [DataRow("10", "+", "3.1", 13.1)]
        [DataRow("10", "-", "3.1", 6.9)]
        [DataRow("10", "/", "0.0", double.PositiveInfinity)]
        [DataRow("-10", "/", "0.0", double.NegativeInfinity)]
        public void AddExpression_ReturnCorrectValue(
            string a,
            string symbol,
            string b,
            object expectedResult
        )
        {
            _instance.AddExpression("a", $"{a}{symbol}{b}");
            object result = _instance.GetResult("a");
            Assert.AreEqual(expectedResult.GetType(), result.GetType());
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void IntegerPowerInteger_IsInteger()
        {
            _instance.AddExpression("a", "2^2");
            object result = _instance.GetResult("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(4, result);

            _instance.AddExpression<int>("b", "3^2");
            int result2 = _instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(9, result2);
        }

        //[TestMethod]
        //public void IntegerPowerIntegerOverflow_Exception()
        //{
        //    Assert.ThrowsException<OverflowException>(() => _instance.AddExpression("a", $"{int.MaxValue}2^2"));
        //    Assert.ThrowsException<OverflowException>(() => _instance.AddExpression<int>("a", "2147483600^2"));
        //    //Int64
        //    Assert.ThrowsException<OverflowException>(() => _instance.AddExpression("a", "21474836001^234123"));
        //}

        [TestMethod]
        public void IntegerAdditionInteger_IsInteger()
        {
            _instance.AddExpression("a", "2+2");
            object result = _instance.GetResult("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(4, result);

            _instance.AddExpression<int>("b", "3+3");
            int result2 = _instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(6, result2);
        }

        [TestMethod]
        public void IntegerAdditionIntegerOverFlow_Exception()
        {
            Assert.ThrowsException<OverflowException>(
                () => _instance.AddExpression("a", $"{long.MaxValue} + 1")
            );
            Assert.ThrowsException<OverflowException>(
                () => _instance.AddExpression<long>("b", $"{long.MaxValue} + 1")
            );
        }

        [TestMethod]
        public void IntegerSubtractionInteger_IsInteger()
        {
            _instance.AddExpression("a", "2-2");
            object result = _instance.GetResult("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(0, result);

            _instance.AddExpression<int>("b", "3-3");
            int result2 = _instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(0, result2);
        }

        [TestMethod]
        public void IntegerSubtractionIntegerOverFlow_Exception()
        {
            Assert.ThrowsException<OverflowException>(
                () => _instance.AddExpression("a", $"{long.MinValue} - 1")
            );
            Assert.ThrowsException<OverflowException>(
                () => _instance.AddExpression<long>("b", $"{long.MinValue} - 1")
            );
        }

        [TestMethod]
        public void IntegerMultiplicationInteger_IsInteger()
        {
            _instance.AddExpression("a", "2*2");
            object result = _instance.GetResult("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(4, result);

            _instance.AddExpression<int>("b", "3*3");
            int result2 = _instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(9, result2);
        }

        [TestMethod]
        public void IntegerDivisionInteger_IsInteger()
        {
            _instance.AddExpression("a", "4/2");
            object result = _instance.GetResult("a");
            Assert.AreEqual(typeof(int), result.GetType());
            Assert.AreEqual(2, result);

            _instance.AddExpression<int>("b", "7/3");
            int result2 = _instance.GetResult<int>("b");
            Assert.AreEqual(typeof(int), result2.GetType());
            Assert.AreEqual(2, result2);
        }

        [TestMethod]
        public void IntegerNegation()
        {
            _instance.AddExpression("a", "-6 + 10");
            object result = _instance.GetResult("a");

            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void IntegerLiterals()
        {
            //Unsigned 32/64 bit
            _instance.AddExpression("e1", "100U + 100LU");
            object result = _instance.GetResult("e1");
            Assert.AreEqual(typeof(UInt64), result.GetType());
            Assert.AreEqual((ulong)200, (UInt64)result);
            //Signed 32/64 bit
            _instance.AddExpression("e2", "100 + 100L");
            result = _instance.GetResult("e2");
            Assert.AreEqual(typeof(Int64), result.GetType());
            Assert.AreEqual((long)200, (Int64)result);
        }
    }
}
