using Yale.Expression;

namespace Yale.Engine;

public class ComputeInstanceOptions
{
    /// <summary>
    /// Results are recalculated when a preceding value is updated. Default is true.
    /// </summary>
    public RecalculateMode Recalculate { get; set; } = RecalculateMode.Auto;

    public ExpressionBuilderOptions ExpressionOptions { get; set; } = new();

    public static ComputeInstanceOptions Default { get; } = new ComputeInstanceOptions();

    public enum RecalculateMode
    {
        /// <summary>
        ///
        /// </summary>
        Auto,

        /// <summary>
        ///
        /// </summary>
        Lazy,

        /// <summary>
        ///
        /// </summary>
        Never
    }
}
