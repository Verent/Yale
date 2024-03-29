﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.ExpressionTests;

[TestClass]
public class Conditional
{
    private readonly ComputeInstance _instance = new();

    [TestMethod]
    public void IfTestTrue()
    {
        _instance.Variables.Add("a", 1);
        _instance.AddExpression("b", "If(a < 100; true; false)");

        Assert.IsTrue((bool)_instance.GetResult("b"));
    }

    [TestMethod]
    public void IfTestFalse()
    {
        _instance.Variables.Add("a", 1);
        _instance.AddExpression("b", "If(a > 100; true; false)");

        Assert.IsFalse((bool)_instance.GetResult("b"));
    }
}
