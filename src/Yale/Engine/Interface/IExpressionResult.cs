using System;

namespace Yale.Engine.Interface
{
    internal interface IExpressionResult
    {
        string Name { get; }

        object ResultAsObject { get; }

        Type ResultType { get; }

        void Recalculate();
    }
}