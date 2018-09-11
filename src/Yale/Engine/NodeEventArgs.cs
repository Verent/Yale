using System;

namespace Yale.Engines
{

    /// <summary>
    /// Emits the result when an expression is recalculated.
    /// </summary>
    public sealed class NodeEventArgs : EventArgs
    {
        internal NodeEventArgs()
        {}

        internal void SetData(string name, object result, object priorResult)
        {
            Name = name;
            Result = result;
        }

        /// <summary>
        /// Name of the expression
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Current result
        /// </summary>
        public object Result { get; private set; }

        /// <summary>
        /// The value of the expression before it was recalculated
        /// </summary>
        public object PriorResult { get; private set; }

    }
}