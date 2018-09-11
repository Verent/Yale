using System;
using Yale.Core;

namespace Yale.Engine.Exceptions
{
    /// <summary>
    /// Represents the exception thrown when a circular reference is detected in the calculation engine.
    /// </summary>
    public class CircularReferenceException : YaleException
    {
        internal CircularReferenceException()
        {
        }

        internal CircularReferenceException(string circularReferenceSource) : base(GetMessage(circularReferenceSource))
        {
        }

        internal CircularReferenceException(string circularReferenceSource, Exception exception) : base(GetMessage(circularReferenceSource), exception)
        {
        }

        private static string GetMessage(string referenceSource)
        {
            return string.IsNullOrWhiteSpace(referenceSource) ?
            "Circular reference detected in calculation engine" :
            $"Circular reference detected in calculation engine at '{referenceSource}'";
        }
    }
}