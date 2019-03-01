using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;

using Yale.Engine;
using Yale.Expression;

namespace Yale.Tests.ExpressionTests
{
    [TestClass]
    public class ExpressionBuilderOptionsTests
    {
        private ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void DefaultValues()
        {
            _instance.AddExpression("a", int.MaxValue.ToString());

            //OverflowCheck
            Assert.ThrowsException<OverflowException>(() => _instance.AddExpression("b", "a + 1"));
            _instance.AddExpression("b", "a - 1");

            //IntegerAsDouble (Default == false)
            Assert.AreEqual(typeof(int), _instance.GetResult("a").GetType());

            #region CaseSensitive

            //Variables
            Assert.ThrowsException<ExpressionCompileException>(() => _instance.AddExpression("c", "A"));
            _instance.AddExpression("c", "a");

            //Expression
            Assert.ThrowsException<ExpressionCompileException>(() => _instance.AddExpression("d", "C"));
            _instance.AddExpression("d", "c");

            //Members
            _instance.Variables.Add("rand", new Random());
            Assert.ThrowsException<ExpressionCompileException>(() => _instance.AddExpression("e", "rand.nextDouble() + 100"));
            _instance.AddExpression("e", "rand.NextDouble() + 100");

            #endregion CaseSensitive

            //StringComparison
            _instance.AddExpression("f", "\"hello\" = \"Hello\"");
            Assert.IsFalse(_instance.GetResult<bool>("f"));

            _instance.AddExpression("g", "\"hello\" = \"hello\"");
            Assert.IsTrue(_instance.GetResult<bool>("g"));

            //ReadLiteral
            _instance.AddExpression("h", "1.0");
            Assert.AreEqual(typeof(double), _instance.GetResult("h").GetType());
        }

        [TestMethod]
        public void RealLiteralDataTypeTest()
        {
            _instance = new ComputeInstance(new ComputeInstanceOptions()
            {
                ExpressionOptions = new ExpressionBuilderOptions
                {
                    RealLiteralDataType = RealLiteralDataType.Decimal
                }
            });
            _instance.AddExpression("a", "4.0");
            Assert.AreEqual(typeof(decimal), _instance.GetResult("a").GetType());

            _instance = new ComputeInstance(new ComputeInstanceOptions()
            {
                ExpressionOptions = new ExpressionBuilderOptions
                {
                    RealLiteralDataType = RealLiteralDataType.Single
                }
            });
            _instance.AddExpression("a", "4.0");
            Assert.AreEqual(typeof(Single), _instance.GetResult("a").GetType());

        }
    }
}