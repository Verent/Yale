namespace Yale.Expression;

/// <summary>
/// This class contains information about an expression and the delegate used to evaluate it.
/// </summary>
/// <typeparam name="T"></typeparam>
internal sealed class Expression<T>
{
    /// <summary>
    /// The compiled delegate used to evaluate the expression
    /// </summary>
    private readonly ExpressionEvaluator<T> evaluator;

    private readonly ExpressionContext context;

    internal Expression(
        string expression,
        ExpressionEvaluator<T> evaluator,
        ExpressionContext context
    )
    {
        this.context = context;
        this.evaluator = evaluator;
        ExpressionText = expression;
        ResultType = typeof(T);
    }

    public string ExpressionText { get; }

    public Type ResultType { get; }

    internal T Evaluate() => evaluator(context.Owner, context, context.Variables);
}
