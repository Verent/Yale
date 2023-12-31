using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Yale.Parser.Internal;

/// <summary>
/// Manages branch information and allows us to determine if we should emit a short or long branch
/// </summary>
internal class BranchManager
{
    private readonly IList<BranchInfo> branchInfos = new List<BranchInfo>();

    private readonly IDictionary<object, Label> keyLabelMap = new Dictionary<object, Label>();

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
            int longBranchesBetween = CountLongBranches(betweenBranches);

            // Adjust the branch as necessary
            branchInfo.AdjustForLongBranchesBetween(longBranchesBetween);
        }

        int longBranchCount = 0;

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
    private int CountLongBranches(ICollection<BranchInfo> dest)
    {
        var count = 0;

        foreach (BranchInfo branchInfo in dest)
        {
            count += Convert.ToInt32(branchInfo.ComputeIsLongBranch());
        }

        return count;
    }

    /// <summary>
    /// Find all the branches between the start and end locations of a target branch
    /// </summary>
    /// <param name="target"></param>
    /// <param name="dest"></param>
    /// <remarks></remarks>
    private void FindBetweenBranches(BranchInfo target, ICollection<BranchInfo> dest)
    {
        foreach (BranchInfo branchInfo in branchInfos)
        {
            if (branchInfo.IsBetween(target))
            {
                dest.Add(branchInfo);
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
        ILLocation startLoc = new ILLocation(ilg.Length);
        BranchInfo branchInfo = new BranchInfo(startLoc, target);

        int index = branchInfos.IndexOf(branchInfo);
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
        ILLocation startLoc = new ILLocation(ilg.Length);
        BranchInfo branchInfo = new BranchInfo(startLoc, target);

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
        if (keyLabelMap.TryGetValue(key, out Label lbl) == false)
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
        int pos = ilg.Length;

        foreach (var branchInfo in branchInfos)
        {
            branchInfo.Mark(target, pos);
        }
    }

    public override string ToString()
    {
        var arr = new string[branchInfos.Count];

        for (int i = 0; i <= branchInfos.Count - 1; i++)
        {
            arr[i] = branchInfos[i].ToString();
        }

        return string.Join(Environment.NewLine, arr);
    }
}
