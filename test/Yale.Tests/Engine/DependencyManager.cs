using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.Engine;

[TestClass]
public class DependencyManager
{
    private readonly ComputeInstance[] _instances =
    {
        new(new ComputeInstanceOptions { Recalculate = true, LazyRecalculate = false }),
        new(new ComputeInstanceOptions { Recalculate = true, LazyRecalculate = true })
    };

    [TestMethod]
    public void ModifyExpressions_Updates_Dependencies()
    {
        ComputeInstance instance = new();

        instance.Variables.Add("a", 10);
        instance.Variables.Add("b", 5);

        instance.AddExpression<int>("c", "a");
        instance.AddExpression<int>("d", "a + b");
        instance.AddExpression<int>("e", "c + d");
        instance.AddExpression<int>("f", "e + 1");

        object result = instance.GetResult("f");
        Assert.AreEqual(26, result);

        instance.SetExpression<int>("d", "a + a");

        result = instance.GetResult("d");
        Assert.AreEqual(20, result);

        result = instance.GetResult("f");
        Assert.AreEqual(31, result);
    }

    [TestMethod]
    public void ComplexDependency()
    {
        ParallelLoopResult result = Parallel.ForEach(_instances, Test1);
        Assert.IsTrue(result.IsCompleted);

        result = Parallel.ForEach(_instances, Test2);
        Assert.IsTrue(result.IsCompleted);

        Squares(_instances[0]);
        //result = Parallel.ForEach(_instances, Squares);
        //Assert.IsTrue(result.IsCompleted);
    }

    public void Test1(ComputeInstance instance)
    {
        instance.Clear();

        instance.Variables["a"] = 10;
        instance.Variables["b"] = 5;

        instance.AddExpression("c", "a");
        instance.AddExpression("d", "a + b");
        instance.AddExpression("e", "c + d");
        instance.AddExpression("f", "e + 1");

        object result = instance.GetResult("f");
        Assert.AreEqual(26, result);

        instance.Variables["b"] = 1;

        result = instance.GetResult("f");
        Assert.AreEqual(22, result);

        instance.Variables["a"] = 5;

        result = instance.GetResult("f");
        Assert.AreEqual(12, result);
    }

    public void Test2(ComputeInstance instance)
    {
        instance.Clear();

        instance.Variables.Add("a", 2);
        instance.Variables.Add("b", 5);

        instance.AddExpression<int>("c", "a + b"); // 7    9   12
        instance.AddExpression<int>("d", "b"); // 5    7   7
        instance.AddExpression<int>("e", "c + b"); // 12   16  19
        instance.AddExpression<int>("f", "a + e"); // 14   18  24
        instance.AddExpression<int>("g", "c + e"); // 19   25
        instance.AddExpression<int>("h", "e + d"); // 17   23
        instance.AddExpression<int>("i", "g + f + h"); // 50   66

        object result = instance.GetResult("i");
        Assert.AreEqual(50, result);

        instance.Variables["b"] = 7;

        result = instance.GetResult("i");
        Assert.AreEqual(66, result);

        instance.Variables["a"] = 5;

        result = instance.GetResult("f");
        Assert.AreEqual(24, result);
    }

    public void Squares(ComputeInstance instance)
    {
        instance.Clear();

        instance.Variables.Add("a", 3);
        instance.AddExpression("square", "a^2");
        object result = instance.GetResult("square");
        Assert.AreEqual(9, result);

        instance.Variables["a"] = 2;
        result = instance.GetResult("square");
        Assert.AreEqual(4, result);

        instance.AddExpression("ssquare", "square^2");
        result = instance.GetResult("ssquare");
        Assert.AreEqual(16, result);

        instance.Variables["a"] = 7;

        result = instance.GetResult("square");
        Assert.AreEqual(49, result);
        result = instance.GetResult("ssquare");
        Assert.AreEqual(2401, result);
    }
}
