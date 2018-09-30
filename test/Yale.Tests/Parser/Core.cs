using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;
using Yale.Expression;

namespace Yale.Tests.Parser
{
    [TestClass]
    public class Core
    {
        private readonly ComputeInstance _instance = new ComputeInstance();

        [TestMethod]
        public void Parse_InvalidToken_ThrowsException()
        {
            Assert.ThrowsException<ExpressionCompileException>(() => _instance.AddExpression<int>("a", "b"));
        }
    }
}