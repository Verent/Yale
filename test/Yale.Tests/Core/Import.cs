using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;

namespace Yale.Test.Core
{
    [TestClass]
    public class Import
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void Parse_ValidImportedType_Executes()
        {
            _instance.Builder.Imports.AddType(typeof(Math));

            const string key = "math_constant";

            _instance.AddExpression(key, "E");
            var result = _instance.GetResult(key);

            Assert.AreEqual(Math.E, result);
        }

        [TestMethod]
        public void Parse_ValidImportedMethod_Executes()
        {
            _instance.Builder.Imports.AddType(typeof(Math));

            const string key = "math_function";

            _instance.AddExpression(key, "Sqrt(16)");
            var result = _instance.GetResult(key);

            Assert.AreEqual(4.0, result);
        }
    }
}