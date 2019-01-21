using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Yale.Core;
using Yale.Engine;

namespace Yale.Tests.Core
{
    [TestClass]
    public class ValueCollection
    {
        private readonly ComputeInstance _instance = new ComputeInstance();
        private VariableCollection _variables => _instance.Variables;

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
                _variables.Get(null);
            });

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _variables.Get<object>(null);
            });
        }

        [TestMethod]
        public void AddValue_ThatAreValid_ReturnsExpectedResult()
        {
            const int a = 1;
            _instance.Variables.Add("a", a);
            var aResult = _variables.Get<int>("a");
            Assert.AreEqual(a, aResult);

            const double b = 1.0;
            _instance.Variables.Add("b", b);
            var bResult = _variables.Get<double>("b");
            Assert.AreEqual(b, bResult);

            const string c = "stringValue";
            _instance.Variables.Add("c", c);
            var cResult = _variables.Get<string>("c");
            Assert.AreEqual(c, cResult);

            const string d = "a > b";
            _instance.Variables.Add("d", d);
            var dResult = _variables.Get<string>("d");
            Assert.AreEqual(d, dResult);
            Assert.AreNotEqual(false, dResult);
        }

        [TestMethod]
        public void Enumerator_Works()
        {
            foreach (var variable in _variables)
            {
                Assert.Fail("No variables addded");
            }

            _variables.Add("a", 1);
            foreach (var variable in _variables)
            {
                Assert.AreEqual("a", variable.Key);
                Assert.AreEqual(1, variable.Value);
            }
        }
    }
}