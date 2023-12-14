using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.ExpressionTests;

[TestClass]
public class String
{
    private readonly ComputeInstance _instance = new();

    [TestMethod]
    public void Basic()
    {
        _instance.AddExpression("a", "\"hi\"");
        object result = _instance.GetResult("a");
        Assert.AreEqual(typeof(string), result.GetType());
        Assert.AreEqual("hi", result);

        _instance.AddExpression<string>("b", "\"hi\"");

        string result2 = _instance.GetResult<string>("b");
        Assert.AreEqual(typeof(string), result2.GetType());
        Assert.AreEqual("hi", result2);
    }

    [TestMethod]
    public void Concatenation()
    {
        _instance.AddExpression("a", "\"abc\" + \"def\"");
        object result = _instance.GetResult("a");
        Assert.AreEqual(typeof(string), result.GetType());
        const string expected = "abcdef";
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Contains()
    {
        _instance.AddExpression("a", "\"def\".Contains(\"ef\")");
        bool result = _instance.GetResult<bool>("a");
        Assert.IsTrue(result);
    }
}
