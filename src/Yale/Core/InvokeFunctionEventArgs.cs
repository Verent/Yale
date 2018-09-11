using System;

namespace Yale.Core
{
    /// <summary>
    /// Provides the data for the InvokeFunction event.
    /// </summary>
    [Obsolete("Only used in the Variable collection")]
    public class InvokeFunctionEventArgs : EventArgs
    {
        internal InvokeFunctionEventArgs(string name, object[] arguments)
        {
            FunctionName = name;
            Arguments = arguments;
        }

        /// <summary>
        /// Gets the name of the on-demand function being invoked.
        /// </summary>
        public string FunctionName { get; }

        /// <summary>
        /// Gets the values of the arguments to the on-demand function being invoked.
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// Gets or sets the result of the on-demand function being invoked.
        /// </summary>
        public object Result { get; set; }
    }
}