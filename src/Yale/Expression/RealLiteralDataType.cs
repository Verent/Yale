using System.Diagnostics.CodeAnalysis;

namespace Yale.Expression;

/// <summary>
/// Defines values to indicate the data type to use for storing real literals.
/// </summary>
[SuppressMessage(
    "Documentation",
    "CA1200:Avoid using cref tags with a prefix",
    Justification = "Because they represent types"
)]
public enum RealLiteralDataType
{
    /// <summary>
    /// Specifies that real literals will be stored using the <see cref="T:System.Single"/> data type.
    /// </summary>
    Single,

    /// <summary>
    /// Specifies that real literals will be stored using the <see cref="T:System.Double"/> data type.
    /// </summary>
    Double,

    /// <summary>
    /// Specifies that real literals will be stored using the <see cref="T:System.Decimal"/> data type.
    /// </summary>
    Decimal
}
