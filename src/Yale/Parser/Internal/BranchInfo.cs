using System;
using System.Reflection.Emit;

namespace Yale.Parser.Internal
{
    /// <summary>
    /// Represents a branch from a start location to an end location
    /// </summary>
    internal class BranchInfo : IEquatable<BranchInfo>
    {
        private readonly ILLocation _start;
        private readonly ILLocation _end;
        private Label _myLabel;

        public BranchInfo(ILLocation startLocation, Label endLabel)
        {
            _start = startLocation;
            _myLabel = endLabel;
            _end = new ILLocation();
        }

        public void AdjustForLongBranches(int longBranchCount)
        {
            _start.AdjustForLongBranch(longBranchCount);
        }

        public void BakeIsLongBranch()
        {
            IsLongBranch = ComputeIsLongBranch();
        }

        public void AdjustForLongBranchesBetween(int betweenLongBranchCount)
        {
            _end.AdjustForLongBranch(betweenLongBranchCount);
        }

        public bool IsBetween(BranchInfo other)
        {
            return _start.CompareTo(other._start) > 0 && _start.CompareTo(other._end) < 0;
        }

        public bool ComputeIsLongBranch()
        {
            return _start.IsLongBranch(_end);
        }

        public void Mark(Label target, int position)
        {
            if (_myLabel.Equals(target))
            {
                _end.SetPosition(position);
            }
        }

        public bool Equals1(BranchInfo other)
        {
            return _start.Equals1(other._start) && _myLabel.Equals(other._myLabel);
        }

        bool IEquatable<BranchInfo>.Equals(BranchInfo other)
        {
            return Equals1(other);
        }

        public override string ToString()
        {
            return $"{_start} -> {_end} (L={_start.IsLongBranch(_end)})";
        }

        public bool IsLongBranch { get; private set; }
    }
}