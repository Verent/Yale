using System;
using System.Reflection;
using Yale.Expression;

namespace Yale.Core.Interface
{
    public interface IExpressionOptions
    {
        bool OverflowChecked { get; }
        bool IntegerAsDouble { get; }
        object DefaultOwner { get; }
        bool CaseSensitive { get; }
        string DateTimeFormat { get; }

        StringComparison StringComparison { get; }
        RealLiteralDataType RealLiteralDataType { get; }

        StringComparison MemberStringComparison { get; }
        MemberFilter MemberFilter { get; }

        void AssertTypeIsAccessible(Type type);
    }
}