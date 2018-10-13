namespace Yale.Expression
{
    using Core;

    internal delegate T ExpressionEvaluator<out T>(object owner, ExpressionContext context, ValueCollection values);
}