using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Yale.Engine;
using Yale.Core;
namespace Yale.Tests.Core
{
    [TestClass]
    public class ValueCollection
    {
        private readonly ComputeInstance _instance = new ComputeInstance();
        private VariableCollection _variable => _instance.Variables;


        [TestMethod]
        public void AddValue_CanBe_Retrieved()
        {
            var value = "a string";
            _instance.Variables.Add("a", value);

            Assert.AreEqual(value, _instance.Variables["a"]);
        }

        [TestMethod]
        public void SetValue_WithoutKey_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _instance.Variables.Add(null, 2);
            });
        }

        [TestMethod]
        public void GetValue_WithoutKey_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _variable.Get(null);
            });

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _variable.Get<object>(null);
            });
        }

        [TestMethod]
        public void AddValue_ThatAreValid_ReturnsExpectedResult()
        {
            const int a = 1;
            _instance.Variables.Add("a", a);
            var aResult = _variable.Get<int>("a");
            Assert.AreEqual(a, aResult);

            const double b = 1.0;
            _instance.Variables.Add("b", b);
            var bResult = _variable.Get<double>("b");
            Assert.AreEqual(b, bResult);

            const string c = "stringValue";
            _instance.Variables.Add("c", c);
            var cResult = _variable.Get<string>("c");
            Assert.AreEqual(c, cResult);

            const string d = "a > b";
            _instance.Variables.Add("d", d);
            var dResult = _variable.Get<string>("d");
            Assert.AreEqual(d, dResult);
            Assert.AreNotEqual(false, dResult);
        }
    }
}