using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yale.Engine;
using Yale.Expression;

namespace Yale.Tests.Parser
{
    [TestClass]
    public class Core
    {
        private readonly ComputeInstance instance = new();

        [TestMethod]
        public void Parse_InvalidToken_ThrowsException()
        {
            Assert.ThrowsException<ExpressionCompileException>(
                () => instance.AddExpression<int>("a", "b")
            );
        }
    }
}
