﻿namespace Yale.Parser.Internal;

/// <summary>
/// Manages branch information and allows us to determine if we should emit a short or long branch
/// </summary>
internal sealed class BranchManager
{
    private readonly List<BranchInfo> branchInfos = new();

    private readonly Dictionary<object, Label> keyLabelMap = new();

    /// <summary>
    /// Determine whether to use short or long branches
    /// </summary>
    /// <remarks></remarks>
    public void ComputeBranches()
    {
        List<BranchInfo> betweenBranches = new();

        foreach (var branchInfo in branchInfos)
        {
            betweenBranches.Clear();

            // Find any branches between the start and end locations of this branch
            FindBetweenBranches(branchInfo, betweenBranches);

            // Count the number of long branches in the above set
            var longBranchesBetween = CountLongBranches(betweenBranches);

            // Adjust the branch as necessary
            branchInfo.AdjustForLongBranchesBetween(longBranchesBetween);
        }

        var longBranchCount = 0;

        // Adjust the start location of each branch
        foreach (var branchInfo in branchInfos)
        {
            // Save the short/long branch type
            branchInfo.BakeIsLongBranch();

            // Adjust the start location as necessary
            branchInfo.AdjustForLongBranches(longBranchCount);

            // Keep a tally of the number of long branches
            longBranchCount += Convert.ToInt32(branchInfo.IsLongBranch);
        }
    }

    /// <summary>
    /// Count the number of long branches in a set
    /// </summary>
    /// <param name="dest"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    private static int CountLongBranches(ICollection<BranchInfo> dest)
    {
        var count = 0;

        foreach (var branchInfo in dest)
        {
            count += Convert.ToInt32(branchInfo.ComputeIsLongBranch());
        }

        return count;
    }

    /// <summary>
    /// Find all the branches between the start and end locations of a target branch
    /// </summary>
    /// <param name="target"></param>
    /// <param name="destination"></param>
    /// <remarks></remarks>
    private void FindBetweenBranches(BranchInfo target, List<BranchInfo> destination)
    {
        foreach (var branchInfo in branchInfos)
        {
            if (branchInfo.IsBetween(target))
            {
                destination.Add(branchInfo);
            }
        }
    }

    /// <summary>
    /// Determine if a branch from a point to a label will be long
    /// </summary>
    /// <param name="ilg"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool IsLongBranch(YaleIlGenerator ilg, Label target)
    {
        ILLocation startLoc = new(ilg.Length);
        BranchInfo branchInfo = new(startLoc, target);

        var index = branchInfos.IndexOf(branchInfo);
        branchInfo = branchInfos[index];

        return branchInfo.IsLongBranch;
    }

    /// <summary>
    /// Add a branch from a location to a target label
    /// </summary>
    /// <param name="ilg"></param>
    /// <param name="target"></param>
    /// <remarks></remarks>
    public void AddBranch(YaleIlGenerator ilg, Label target)
    {
        ILLocation startLoc = new(ilg.Length);
        BranchInfo branchInfo = new(startLoc, target);

        branchInfos.Add(branchInfo);
    }

    /// <summary>
    /// Get a label by a key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public Label FindLabel(object key) => keyLabelMap[key];

    /// <summary>
    /// Get a label by a key.  Create the label if it is not present.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ilg"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public Label GetLabel(object key, YaleIlGenerator ilg)
    {
        if (keyLabelMap.TryGetValue(key, out var lbl) == false)
        {
            lbl = ilg.DefineLabel();
            keyLabelMap.Add(key, lbl);
        }
        return lbl;
    }

    /// <summary>
    /// Determines if we have a label for a key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public bool HasLabel(object key) => keyLabelMap.ContainsKey(key);

    /// <summary>
    /// Set the position for a label
    /// </summary>
    /// <param name="ilg"></param>
    /// <param name="target"></param>
    /// <remarks></remarks>
    public void MarkLabel(YaleIlGenerator ilg, Label target)
    {
        var pos = ilg.Length;

        foreach (var branchInfo in branchInfos)
        {
            branchInfo.Mark(target, pos);
        }
    }

    public override string ToString()
    {
        var arr = new string[branchInfos.Count];

        for (var i = 0; i <= branchInfos.Count - 1; i++)
        {
            arr[i] = branchInfos[i].ToString();
        }

        return string.Join(Environment.NewLine, arr);
    }
}
