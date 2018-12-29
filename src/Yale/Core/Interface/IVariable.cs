using System;

namespace Yale.Core.Interface
{
    /// <summary>
    /// Internal object for storing expression variables
    /// </summary>
    internal interface IVariable
    {
        Type Type { get; }

        object ValueAsObject { get; }
    }
}