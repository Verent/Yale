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

        private readonly List<DependencyNode> _predecessors = new List<DependencyNode>();

        private readonly List<DependencyNode> _successors = new List<DependencyNode>();

        public void AddPredecessor(DependencyNode node)
        {
            _predecessors.Add(node);
            _precedents = null;
            node._successors.Add(this);
            node._dependents = null;
        }

        private string[] _dependents;

        public string[] Dependents
        {
            get
            {
                return _dependents ?? (_dependents = _successors.Select(s => s.Key).ToArray());
            }
        }

        private string[] _precedents;

        public string[] Precedents
        {
            get
            {
                return _precedents ?? (_precedents = _predecessors.Select(s => s.Key).ToArray());
            }
        }

        /// <summary>
        /// Removes predecessors. Used when expression is changed.
        /// </summary>
        public void ClearPredecessors()
        {
            _predecessors.Clear();
        }
    }
}