﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Yale.Engine;

namespace Yale.Test.ExpressionTests
{
    [TestClass]
    public class Double
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        [DataTestMethod]
        [DataRow("1.0", "+", "1.0", 2.0)]
        [DataRow("1.0", "-", "1.0", 0.0)]
        [DataRow("2.0", "*", "2.0", 4.0)]
        [DataRow("2.0", "/", "2.0", 1.0)]
        [DataRow("10.0", "/", "3", 10.0 / 3.0)]
        [DataRow("10.0", "/", "0.0", double.PositiveInfinity)]
        [DataRow("-10.0", "/", "0.0", double.NegativeInfinity)]
        public void Double_AddExpression_ReturnCorrectValue(string a, string symbol, string b, object expectedResult)
        {
            _instance.AddExpression("a", $"{a}{symbol}{b}");
            var result = _instance.GetResult("a");
            Assert.AreEqual(expectedResult.GetType(), result.GetType());
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void Double_DoublePowerDouble_IsDouble()
        {
            _instance.AddExpression("a", "2.0^2.1");
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(double), result.GetType());
            Assert.AreEqual(Math.Pow(2, 2.1), result);

            _instance.AddExpression<double>("b", "3.0^2.1");
            var result2 = _instance.GetResult<double>("b");
            Assert.AreEqual(typeof(double), result2.GetType());
            Assert.AreEqual(Math.Pow(3, 2.1), result2);
        }

    }
}