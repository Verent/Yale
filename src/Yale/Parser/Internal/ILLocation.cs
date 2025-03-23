namespace Yale.Parser.Internal;

/// <summary>
/// Represents a location in an IL stream
/// </summary>
internal sealed class ILLocation : IEquatable<ILLocation>, IComparable<ILLocation>
{
    private int position;

    /// <summary>
    /// Long branch is 5 bytes; short branch is 2; so we adjust by the difference
    /// </summary>
    private const int LongBranchAdjust = 3;

    /// <summary>
    /// Length of the Br_s opcode
    /// </summary>
    private const int BrSLength = 2;

    public ILLocation() { }

    public ILLocation(int position) => this.position = position;

    public void SetPosition(int position) => this.position = position;

    /// <summary>
    /// Adjust our position by a certain amount of long branches
    /// </summary>
    /// <param name="longBranchCount"></param>
    /// <remarks></remarks>
    public void AdjustForLongBranch(int longBranchCount) =>
        position += longBranchCount * LongBranchAdjust;

    /// <summary>
    /// Determine if this branch is long
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool IsLongBranch(ILLocation target) =>
        Utility.IsLongBranch(position + BrSLength, target.position);

    // The branch offset is relative to the instruction *after* the branch so we add 2 (length of a br_s) to our position

    public bool Equals1(ILLocation other) => position == other.position;

    bool IEquatable<ILLocation>.Equals(ILLocation? other) => other is not null && Equals1(other);

    public override string ToString() =>
        position.ToString("x", provider: CultureInfo.InvariantCulture);

    public int CompareTo(ILLocation? other) =>
        other is null ? 1 : position.CompareTo(other.position);
}
