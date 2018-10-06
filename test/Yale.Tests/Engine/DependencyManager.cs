using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.Engine
{
    [TestClass]
    public class DependencyManager
    {
        private readonly ComputeInstance[] _instances = 
        {
            new ComputeInstance(new ComputeInstanceOptions
            {
                AutoRecalculate = true,
                LazyRecalculate = false
            }),
            new ComputeInstance(new ComputeInstanceOptions
            {
                AutoRecalculate = true,
                LazyRecalculate = true
            })
        };

        [TestMethod]
        public void ComplexDependency()
        {
            var result = Parallel.ForEach(_instances, Test1);
            Assert.IsTrue(result.IsCompleted);

            result = Parallel.ForEach(_instances, Test2);
            Assert.IsTrue(result.IsCompleted);
        }

        public void Test1(ComputeInstance instance)
        {
            instance.Clear();

            instance.SetValue("a", 10);
            instance.SetValue("b", 5);

            instance.AddExpression<int>("c", "a");
            instance.AddExpression<int>("d", "a + b");
            instance.AddExpression<int>("e", "c + d");
            instance.AddExpression<int>("f", "e + 1");

            var result = instance.GetResult("f");
            Assert.AreEqual(26, result);

            instance.SetValue("b", 1);

            result = instance.GetResult("f");
            Assert.AreEqual(22, result);

            instance.SetValue("a", 5);

            result = instance.GetResult("f");
            Assert.AreEqual(12, result);
        }


        public void Test2(ComputeInstance instance)
        {
            instance.Clear();

            instance.SetValue("a", 2);
            instance.SetValue("b", 5);

            instance.AddExpression<int>("c", "a + b");      // 7    9   12
            instance.AddExpression<int>("d", "b");          // 5    7   7
            instance.AddExpression<int>("e", "c + b");      // 12   16  19
            instance.AddExpression<int>("f", "a + e");      // 14   18  24
            instance.AddExpression<int>("g", "c + e");      // 19   25 
            instance.AddExpression<int>("h", "e + d");      // 17   23
            instance.AddExpression<int>("i", "g + f + h");  // 50   66


            var result = instance.GetResult("i");
            Assert.AreEqual(50, result);

            instance.SetValue("b", 7);

            result = instance.GetResult("i");
            Assert.AreEqual(66, result);

            instance.SetValue("a", 5);

            result = instance.GetResult("f");
            Assert.AreEqual(24, result);
        }
    }
}