using System;

namespace Yale.Parser.Internal;

/// <summary>
/// "Represents a location in an IL stream"
/// </summary>
internal class ILLocation : IEquatable<ILLocation>, IComparable<ILLocation>
{
    private int _position;

    /// <summary>
    /// ' Long branch is 5 bytes; short branch is 2; so we adjust by the difference
    /// </summary>
    private const int LongBranchAdjust = 3;

    /// <summary>
    /// Length of the Br_s opcode
    /// </summary>
    private const int BrSLength = 2;

    public ILLocation() { }

    public ILLocation(int position) => _position = position;

    public void SetPosition(int position) => _position = position;

    /// <summary>
    /// Adjust our position by a certain amount of long branches
    /// </summary>
    /// <param name="longBranchCount"></param>
    /// <remarks></remarks>
    public void AdjustForLongBranch(int longBranchCount) =>
        _position += longBranchCount * LongBranchAdjust;

    /// <summary>
    /// Determine if this branch is long
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool IsLongBranch(ILLocation target) =>
        // The branch offset is relative to the instruction *after* the branch so we add 2 (length of a br_s) to our position
        Utility.IsLongBranch(_position + BrSLength, target._position);

    public bool Equals1(ILLocation other) => _position == other._position;

    bool IEquatable<ILLocation>.Equals(ILLocation other) => Equals1(other);

    public override string ToString() => _position.ToString("x");

    public int CompareTo(ILLocation other) => _position.CompareTo(other._position);
}
