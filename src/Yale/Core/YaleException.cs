using System;

namespace Yale.Core
{
    public class YaleException : Exception
    {
        internal YaleException()
        { }

        internal YaleException(string message) : base(message)
        { }

        internal YaleException(string message, Exception e) : base(message, e)
        { }
    }
}