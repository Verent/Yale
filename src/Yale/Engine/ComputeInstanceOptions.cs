namespace Yale.Engine
{
    public class ComputeInstanceOptions
    {
        /// <summary>
        /// Results that are retrieved are always using that latest values in
        /// precedents
        /// </summary>
        public bool AutoRecalculate { get; set; } = true;

        /// <summary>
        /// Results are only calculated when retrieving the result or
        /// the result of an expression that depend on it. Lazy recalculate will only work when AutoRecalculate is true
        /// </summary>
        public bool LazyRecalculate { get; set; } = false;

        public static ComputeInstanceOptions Default { get; } = new ComputeInstanceOptions();
    }
}