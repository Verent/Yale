using System;
using System.Reflection;
using Yale.Core.Interfaces;
using Yale.Resources;

namespace Yale.Expression;

public class ExpressionBuilderOptions : IExpressionOptions
{
    private const string Format = "dd/MM/yyyy";

    public bool OverflowChecked { get; set; } = true;
    public bool IntegerAsDouble { get; set; } = false;

    public bool CaseSensitive { get; set; } = true;
    public string DateTimeFormat { get; set; } = Format;

    public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
    public RealLiteralDataType RealLiteralDataType { get; set; } = RealLiteralDataType.Double;

    public StringComparer StringComparer =>
        CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
    public StringComparison MemberStringComparison =>
        CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    public MemberFilter MemberFilter => CaseSensitive ? Type.FilterName : Type.FilterNameIgnoreCase;

    public void AssertTypeIsAccessible(Type type)
    {
        if (type.IsNested)
        {
            AssertNestedTypeIsAccessible(type);
        }
        else
        {
            AssertTypeIsAccessibleInternal(type);
        }
    }

    //Todo:Verify
    private void AssertNestedTypeIsAccessible(Type type)
    {
        while (type != null)
        {
            AssertTypeIsAccessibleInternal(type);
            type = type.DeclaringType;
        }
    }

    private static void AssertTypeIsAccessibleInternal(Type t)
    {
        bool isPublic = t.IsNested ? t.IsNestedPublic : t.IsPublic;

        if (isPublic == false)
        {
            string msg = string.Format(GeneralErrors.TypeNotAccessibleToExpression, t.Name);
            throw new ArgumentException(msg);
        }
    }
}
