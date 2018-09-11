namespace Yale.Expression
{
    using Core;

    internal delegate T ExpressionEvaluator<T>(object owner, ExpressionContext context, ValueCollection values);
}