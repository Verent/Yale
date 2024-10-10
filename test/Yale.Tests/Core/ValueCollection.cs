using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Core;
using Yale.Engine;

namespace Yale.Tests.Core;

[TestClass]
public class ValueCollection
{
    private readonly ComputeInstance instance = new();
    private VariableCollection Variables => instance.Variables;

    [TestMethod]
    public void AddValue_CanBe_Retrieved()
    {
        var value = "a string";
        Variables.Add("a", value);

        Assert.AreEqual(value, Variables["a"]);
    }

    [TestMethod]
    public void SetValue_WithoutKey_ThrowsException()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            Variables.Add(null, 2);
        });
    }

    [TestMethod]
    public void GetValue_WithoutKey_ThrowsException()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            Variables.Get(null);
        });

        Assert.ThrowsException<ArgumentNullException>(() =>
        {
            Variables.Get<object>(null);
        });
    }

    [TestMethod]
    public void AddValue_ThatAreValid_ReturnsExpectedResult()
    {
        const int a = 1;
        Variables.Add("a", a);
        var aResult = Variables.Get<int>("a");
        Assert.AreEqual(a, aResult);

        const double b = 1.0;
        Variables.Add("b", b);
        var bResult = Variables.Get<double>("b");
        Assert.AreEqual(b, bResult);

        const string c = "stringValue";
        Variables.Add("c", c);
        var cResult = Variables.Get<string>("c");
        Assert.AreEqual(c, cResult);

        const string d = "a > b";
        Variables.Add("d", d);
        var dResult = Variables.Get<string>("d");
        Assert.AreEqual(d, dResult);
        //Todo
        //Assert.AreNotEqual(false, dResult);
    }

    [TestMethod]
    public void Enumerator_Works()
    {
        foreach (var _ in Variables)
        {
            Assert.Fail("No variables added");
        }

        Variables.Add("a", 1);
        foreach (var variable in Variables)
        {
            Assert.AreEqual("a", variable.Key);
            Assert.AreEqual(1, variable.Value);
        }
    }
}
