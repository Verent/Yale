using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Yale.Engine;
using Yale.Expression;

namespace Yale.Tests.Core
{
    [TestClass]
    public class Import
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void Parse_ValidImportedTypeProperty_Executes()
        {
            _instance.Imports.AddType(typeof(Math));

            const string key = "math_constant";

            _instance.AddExpression(key, "E");
            var result = _instance.GetResult(key);

            Assert.AreEqual(Math.E, result);
        }

        [TestMethod]
        public void Parse_ValidImportedTypeMethod_Executes()
        {
            _instance.Imports.AddType(typeof(Math));

            const string key = "math_function";

            _instance.AddExpression(key, "Sqrt(16)");
            var result = _instance.GetResult(key);

            Assert.AreEqual(4.0, result);
        }

        [TestMethod]
        public void Parse_ValidImportedTypeWithNSMethod_Executes()
        {
            _instance.Imports.AddType(typeof(Math), "Test");

            const string key = "math_function";

            _instance.AddExpression(key, "Test.Sqrt(16)");
            var result = _instance.GetResult(key);

            Assert.AreEqual(4.0, result);
        }

        [TestMethod]
        public void Parse_ImportedMethod_Executes()
        {
            _instance.Imports.AddMethod("Sqrt", typeof(Math), "Test");

            const string key = "math_function";

            _instance.AddExpression(key, "Test.Sqrt(16)");
            var result = _instance.GetResult(key);

            Assert.AreEqual(4.0, result);
        }

        [TestMethod]
        public void Parse_MethodNotImported_ThrowsException()
        {
            _instance.Imports.AddMethod("Sqrt", typeof(Math), "Test");

            Assert.ThrowsException<ExpressionCompileException>(
                () => _instance.AddExpression("key", "Test.Pow(16)"));
        }
    }
}