namespace Yale.Expression;

/// <summary>
/// Defines values to indicate the data type to use for storing real literals.
/// </summary>

#pragma warning disable CA1720 // Identifier contains type name
public enum RealLiteralDataType
{
    /// <summary>
    /// Specifies that real literals will be stored using the <see cref="float"/> data type.
    /// </summary>
    Single,

    /// <summary>
    /// Specifies that real literals will be stored using the <see cref="double"/> data type.
    /// </summary>
    Double,

    /// <summary>
    /// Specifies that real literals will be stored using the <see cref="decimal"/> data type.
    /// </summary>
    Decimal
}
#pragma warning restore CA1720 // Identifier contains type name
