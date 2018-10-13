using System;

namespace Yale.Expression
{
    /// <summary>
    /// This class contains information about an expression and the delegate used to
    /// evaluate it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Expression<T>
    {
        /// <summary>
        /// The compiled delegate used to evaluate the expression
        /// </summary>
        private readonly ExpressionEvaluator<T> Evaluator;

        public string ExpressionText { get; }

        public Type ResultType { get; }

        private readonly ExpressionContext _context;

        internal Expression(string expression, ExpressionEvaluator<T> evaluator, ExpressionContext context)
        {
            _context = context;
            Evaluator = evaluator;
            ExpressionText = expression;
            ResultType = typeof(T);
        }

        internal T Evaluate()
        {
            return Evaluator(_context.Owner, _context, _context.Values);
        }
    }
}