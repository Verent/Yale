using System.Collections.Generic;
using System.Linq;

namespace Yale.Engine.Internal
{
    /// <summary>
    /// Dependency Graph Vertex
    /// </summary>
    internal class DependencyNode
    {
        public DependencyNode(string key)
        {
            Key = key;
        }

        private string Key { get; }

        private readonly List<DependencyNode> predecessor = new List<DependencyNode>();

        private readonly List<DependencyNode> successors = new List<DependencyNode>();

        public void AddPredecessor(DependencyNode node)
        {
            predecessor.Add(node);
            precedents = null;
            node.successors.Add(this);
            node.dependents = null;
        }

        private string[]? dependents;

        public string[] Dependents
        {
            get { return dependents ?? (dependents = successors.Select(s => s.Key).ToArray()); }
        }

        private string[]? precedents;

        public string[] Precedents
        {
            get { return precedents ?? (precedents = predecessor.Select(s => s.Key).ToArray()); }
        }

        /// <summary>
        /// Removes predecessors. Used when expression is changed.
        /// </summary>
        public void ClearPredecessors()
        {
            predecessor.Clear();
        }
    }
}
