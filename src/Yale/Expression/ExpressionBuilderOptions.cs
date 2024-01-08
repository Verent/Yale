using System;
using System.Reflection;
using Yale.Core.Interfaces;
using Yale.Resources;

namespace Yale.Expression;

public class ExpressionBuilderOptions : IExpressionOptions
{
    private const string Format = "dd/MM/yyyy";

    /// <summary>
    /// Checks if the result of an operation is within the range of the result type
    /// Default: true
    /// </summary>
    public bool OverflowChecked { get; set; } = true;

    /// <summary>
    /// If true, integer literals will be treated as double
    /// Default: false
    /// </summary>
    public bool IntegerAsDouble { get; set; }

    /// <summary>
    /// Support case sensitive expressions
    /// Default: true
    /// </summary>
    public bool CaseSensitive { get; set; } = true;

    /// <summary>
    /// Format of DateTime literals
    /// </summary>
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
        ArgumentNullException.ThrowIfNull(nameof(type));

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
    private static void AssertNestedTypeIsAccessible(Type type)
    {
        Type? typeInternal = type;
        do
        {
            AssertTypeIsAccessibleInternal(typeInternal);
            typeInternal = typeInternal.DeclaringType;
        } while (typeInternal is not null);
    }

    private static void AssertTypeIsAccessibleInternal(Type type)
    {
        bool isPublic = type.IsNested ? type.IsNestedPublic : type.IsPublic;

        if (isPublic is false)
        {
            var msg = string.Format(GeneralErrors.TypeNotAccessibleToExpression, type.Name);
            throw new ArgumentException(msg);
        }
    }
}
