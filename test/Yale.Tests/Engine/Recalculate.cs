﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;
using Yale.Tests.Helper;

namespace Yale.Tests.Engine;

[TestClass]
public class Recalculate
{
    private readonly ComputeInstance autoInstance =
        new(
            new ComputeInstanceOptions { Recalculate = ComputeInstanceOptions.RecalculateMode.Auto }
        );

    private readonly ComputeInstance lazyInstance =
        new(
            new ComputeInstanceOptions { Recalculate = ComputeInstanceOptions.RecalculateMode.Lazy }
        );

    private readonly ComputeInstance noRecalculateInstance =
        new(
            new ComputeInstanceOptions
            {
                Recalculate = ComputeInstanceOptions.RecalculateMode.Never
            }
        );

    [TestMethod]
    public void AutoRecalculate_ValueUpdated_ReturnsUpdatedValue()
    {
        autoInstance.Variables.Add("a", 10);
        autoInstance.AddExpression<int>("b", "a");

        autoInstance.Variables["a"] = 20;
        var result = autoInstance.GetResult("b");

        Assert.AreEqual(20, result);
    }

    [TestMethod]
    public void AutoRecalculate_ValueUpdatedDependentExpression_ReturnUpdatedValue()
    {
        autoInstance.Variables.Add("a", 10);
        autoInstance.AddExpression<int>("b", "a");
        autoInstance.AddExpression<int>("c", "b");

        autoInstance.Variables["a"] = 20;
        var result = autoInstance.GetResult("c");

        Assert.AreEqual(20, result);
    }

    [TestMethod]
    public void AutoRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue()
    {
        TestClass<string> testObject =
            new(nameof(AutoRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue));
        var expected = "a string";

        testObject.Value = expected;

        autoInstance.Variables["o"] = testObject;
        autoInstance.AddExpression<string>("e", "o.Value");

        Assert.AreEqual(expected, autoInstance.GetResult("e"));

        expected = "a new string";
        testObject.Value = expected;

        Assert.AreEqual(expected, autoInstance.GetResult("e"));
    }

    [TestMethod]
    public void AutoRecalculate_Int_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue()
    {
        TestClass<int> testObject =
            new(
                nameof(
                    AutoRecalculate_Int_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue
                )
            );
        var expected = 1;

        testObject.Value = expected;

        autoInstance.Variables["o"] = testObject;
        autoInstance.AddExpression<int>("e", "o.Value");

        Assert.AreEqual(expected, autoInstance.GetResult("e"));

        expected = 2;
        testObject.Value = expected;

        Assert.AreEqual(expected, autoInstance.GetResult("e"));
    }

    [TestMethod]
    public void LazyRecalculate_ValueUpdated_ReturnUpdatedValue()
    {
        lazyInstance.Variables.Add("a", 10);
        lazyInstance.AddExpression<int>("b", "a");

        lazyInstance.Variables["a"] = 20;
        var result = lazyInstance.GetResult("b");

        Assert.AreEqual(20, result);
    }

    [TestMethod]
    public void LazyRecalculate_ValueUpdatedDependentExpression_ReturnUpdatedValue()
    {
        lazyInstance.Variables.Add("a", 10);
        lazyInstance.AddExpression<int>("b", "a");
        lazyInstance.AddExpression<int>("c", "b");

        lazyInstance.Variables["a"] = 20;
        var result = lazyInstance.GetResult("c");

        Assert.AreEqual(20, result);
    }

    [TestMethod]
    public void LazyRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue()
    {
        TestClass<string> testObject =
            new(nameof(LazyRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue));
        var expected = "a string";

        testObject.Value = expected;

        lazyInstance.Variables["o"] = testObject;
        lazyInstance.AddExpression<string>("e", "o.Value");

        Assert.AreEqual(expected, lazyInstance.GetResult("e"));

        expected = "a new string";
        testObject.Value = expected;

        Assert.AreEqual(expected, lazyInstance.GetResult("e"));
    }

    [TestMethod]
    public void NoRecalculate_ValueUpdated_ReturnStartValue()
    {
        noRecalculateInstance.Variables.Add("a", 10);
        noRecalculateInstance.AddExpression<int>("b", "a");

        noRecalculateInstance.Variables["a"] = 20;
        var result = noRecalculateInstance.GetResult("b");

        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void NoRecalculate_ValueUpdatedDependentExpression_ReturnStartValue()
    {
        noRecalculateInstance.Variables.Add("a", 10);
        noRecalculateInstance.AddExpression<int>("b", "a");
        noRecalculateInstance.AddExpression<int>("c", "b");

        noRecalculateInstance.Variables["a"] = 20;
        var result = noRecalculateInstance.GetResult("c");

        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void NoRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue()
    {
        TestClass<string> testObject =
            new(nameof(NoRecalculate_InstanceValueUpdatedDependentExpression_ReturnUpdatedValue));
        var expected = "a string";

        testObject.Value = expected;

        noRecalculateInstance.Variables.Add("o", testObject);
        noRecalculateInstance.AddExpression<string>("e", "o.Value");

        Assert.AreEqual(expected, noRecalculateInstance.GetResult("e"));

        testObject.Value = "a new string";
        Assert.AreEqual(expected, noRecalculateInstance.GetResult("e"));
    }
}
