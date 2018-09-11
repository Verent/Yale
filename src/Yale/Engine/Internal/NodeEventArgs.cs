using System;

namespace Yale.Engine.Internal
{
    /// <summary>
    /// Provides the data for the NodeRecalculated event.
    /// </summary>
    public sealed class NodeEventArgs : EventArgs
    {
        internal NodeEventArgs()
        { }

        internal void SetData(string name, object result)
        {
            Name = name;
            Result = result;
        }

        /// <summary>
        /// Gets the name of the recalculated node.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the recalculated result of the node.
        /// </summary>
        public object Result { get; private set; }
    }
}