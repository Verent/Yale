using Yale.Expression;

namespace Yale.Engine
{
    public class ComputeInstanceOptions
    {
        /// <summary>
        /// Results are recalculated when a preceding value is updated
        /// </summary>
        public bool AutoRecalculate { get; set; } = true;

        /// <summary>
        /// Results are only calculated when retrieving the result or
        /// the result of an expression that depend on it. Lazy recalculate will only work when AutoRecalculate is true
        /// </summary>
        public bool LazyRecalculate { get; set; } = false;

        public ExpressionBuilderOptions ExpressionOptions { get; set; } =
            new ExpressionBuilderOptions();

        public static ComputeInstanceOptions Default { get; } = new ComputeInstanceOptions();
    }
}
