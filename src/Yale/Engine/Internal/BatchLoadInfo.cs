using Yale.Expression;

namespace Yale.Engine.Internal
{
    internal class BatchLoadInfo
    {
        public string Name;
        public string ExpressionText;

        public ExpressionContext Context;

        public BatchLoadInfo(string name, string text, ExpressionContext context)
        {
            Name = name;
            ExpressionText = text;
            Context = context;
        }
    }
}