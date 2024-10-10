namespace Yale.Engine.Internal;

/// <summary>
/// Keeps track of expression dependencies
/// </summary>
internal sealed class DependencyManager
{
    /// <summary>
    /// Map of a node with edges
    /// </summary>
    private Dictionary<string, DependencyNode> Nodes { get; } = new();

    public void Clear() => Nodes.Clear();

    public void AddDependency(string expressionKey, string dependsOnKey)
    {
        DependencyNode? expressionNode = null;
        DependencyNode? dependsOnNode = null;

        if (Nodes.ContainsKey(dependsOnKey) == false)
        {
            dependsOnNode = new DependencyNode(dependsOnKey);
            Nodes.Add(dependsOnKey, dependsOnNode);
        }

        if (Nodes.ContainsKey(expressionKey) == false)
        {
            expressionNode = new DependencyNode(expressionKey);
            Nodes.Add(expressionKey, expressionNode);
        }

        dependsOnNode ??= Nodes[dependsOnKey];
        expressionNode ??= Nodes[expressionKey];

        expressionNode.AddPredecessor(dependsOnNode);
    }

    public string[] GetDirectDependents(string key)
    {
        if (Nodes.ContainsKey(key) is false)
            return Array.Empty<string>();
        return Nodes[key].Dependents;
    }

    public string[] GetDependents(string key)
    {
        if (Nodes.ContainsKey(key) is false)
            return Array.Empty<string>();

        List<string> dependents = new();
        foreach (var pair in Nodes[key].Dependents)
        {
            GetDependentsRecursive(pair, dependents);
        }

        return dependents.ToArray();
    }

    private void GetDependentsRecursive(string nodeKey, ICollection<string> dependents)
    {
        dependents.Add(nodeKey);
        foreach (var pair in Nodes[nodeKey].Dependents)
        {
            GetDependentsRecursive(pair, dependents);
        }
    }

    public string[] GetDirectPrecedents(string nodeKey) => Nodes[nodeKey].Precedents;

    public void RemovePrecedents(string nodeKey)
    {
        if (Nodes.TryGetValue(nodeKey, out var value))
        {
            value.ClearPredecessors();
        }
    }

    public string DependencyGraph
    {
        get
        {
            var lines = new string[Nodes.Count];
            var index = 0;
            foreach (var node in Nodes)
            {
                var key = node.Key;
                var dependencies = string.Join(",", node.Value.Dependents);
                lines[index] = $"{key} -> {dependencies}";
                index += 1;
            }
            return string.Join(Environment.NewLine, lines);
        }
    }

    public int DependencyNodes => Nodes.Count;
}
