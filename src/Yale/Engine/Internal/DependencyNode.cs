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

        private List<DependencyNode> Predecessors { get; } = new List<DependencyNode>();

        private List<DependencyNode> Successors { get; } = new List<DependencyNode>();

        public void AddPredecessor(DependencyNode node)
        {
            Predecessors.Add(node);
            node.Successors.Add(this);
        }

        private string[] _dependents;

        public string[] Dependents
        {
            get
            {
                return _dependents ??
                       (_dependents = Successors.Select(s => s.Key).ToArray());
            }
        }

        private string[] _precedents;

        public string[] Precedents
        {
            get
            {
                return _precedents ??
                       (_precedents = Predecessors.Select(s => s.Key).ToArray());
            }
        }

        /// <summary>
        /// Removes predecessors. Used when expression is changed.
        /// </summary>
        public void ClearPredecessors()
        {
            Predecessors.Clear();
        }
    }
}