using System;
using System.Reflection.Emit;

namespace Yale.Parser.Internal
{
    /// <summary>
    /// Represents a branch from a start location to an end location
    /// </summary>
    internal class BranchInfo : IEquatable<BranchInfo>
    {
        private readonly ILLocation start;
        private readonly ILLocation end;
        private readonly Label myLabel;

        public BranchInfo(ILLocation startLocation, Label endLabel)
        {
            start = startLocation;
            myLabel = endLabel;
            end = new ILLocation();
        }

        public void AdjustForLongBranches(int longBranchCount)
        {
            start.AdjustForLongBranch(longBranchCount);
        }

        public void BakeIsLongBranch()
        {
            IsLongBranch = ComputeIsLongBranch();
        }

        public void AdjustForLongBranchesBetween(int betweenLongBranchCount)
        {
            end.AdjustForLongBranch(betweenLongBranchCount);
        }

        public bool IsBetween(BranchInfo other)
        {
            return start.CompareTo(other.start) > 0 && start.CompareTo(other.end) < 0;
        }

        public bool ComputeIsLongBranch()
        {
            return start.IsLongBranch(end);
        }

        public void Mark(Label target, int position)
        {
            if (myLabel.Equals(target))
            {
                end.SetPosition(position);
            }
        }

        public bool Equals1(BranchInfo other)
        {
            return start.Equals1(other.start) && myLabel.Equals(other.myLabel);
        }

        bool IEquatable<BranchInfo>.Equals(BranchInfo other)
        {
            return Equals1(other);
        }

        public override string ToString()
        {
            return $"{start} -> {end} (L={start.IsLongBranch(end)})";
        }

        public bool IsLongBranch { get; private set; }
    }
}