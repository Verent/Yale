using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Tests.Core
{
    [TestClass]
    public class ValueCollection
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void AddValue_CanBe_Retrieved()
        {
            var value = "a string";
            _instance.SetValue("a", value);

            Assert.AreEqual(value, _instance.GetValue("a"));
        }

        [TestMethod]
        public void SetValue_WithoutKey_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _instance.SetValue(null, 2);
            });
        }

        [TestMethod]
        public void GetValue_WithoutKey_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _instance.GetValue(null);
            });

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _instance.GetValue<object>(null);
            });
        }

        [TestMethod]
        public void AddValue_ThatAreValid_ReturnsExpectedResult()
        {
            const int a = 1;
            _instance.SetValue("a", a);
            var aResult = _instance.GetValue<int>("a");
            Assert.AreEqual(a, aResult);

            const double b = 1.0;
            _instance.SetValue("b", b);
            var bResult = _instance.GetValue<double>("b");
            Assert.AreEqual(b, bResult);

            const string c = "stringValue";
            _instance.SetValue("c", c);
            var cResult = _instance.GetValue<string>("c");
            Assert.AreEqual(c, cResult);

            const string d = "a > b";
            _instance.SetValue("d", d);
            var dResult = _instance.GetValue<string>("d");
            Assert.AreEqual(d, dResult);
            Assert.AreNotEqual(false, dResult);
        }
    }
}