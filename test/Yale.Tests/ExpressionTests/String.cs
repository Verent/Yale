using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class String
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void Basic()
        {
            _instance.AddExpression("a", "\"hi\"");
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(string), result.GetType());
            Assert.AreEqual("hi", result);

            _instance.AddExpression<string>("b", "\"hi\"");
            
            var result2 = _instance.GetResult<string>("b");
            Assert.AreEqual(typeof(string), result2.GetType());
            Assert.AreEqual("hi", result2);
        }


        [TestMethod]
        public void Concatenation()
        {
            _instance.AddExpression("a", "\"abc\" + \"def\"");
            var result = _instance.GetResult("a");
            Assert.AreEqual(typeof(string), result.GetType());
            const string expected = "abcdef";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Contains()
        {
            _instance.AddExpression("a", "\"def\".Contains(\"ef\")");
            var result = _instance.GetResult<bool>("a");
            Assert.IsTrue(result);
        }
    }
}
