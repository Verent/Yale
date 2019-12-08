using System;
using System.Reflection;
using Yale.Expression;

namespace Yale.Core.Interfaces
{
    public interface IExpressionOptions
    {
        bool OverflowChecked { get; }
        bool IntegerAsDouble { get; }
        bool CaseSensitive { get; }
        string DateTimeFormat { get; }

        StringComparison StringComparison { get; }
        RealLiteralDataType RealLiteralDataType { get; }

        StringComparison MemberStringComparison { get; }
        MemberFilter MemberFilter { get; }

        void AssertTypeIsAccessible(Type type);
    }
}