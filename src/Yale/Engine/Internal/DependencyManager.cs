using System;
using System.Collections.Generic;

namespace Yale.Engine.Internal
{
    /// <summary>
    /// Keeps track of expression dependencies
    /// </summary>
    internal class DependencyManager
    {
        /// <summary>
        /// Map of a node with edges
        /// </summary>
        private Dictionary<string, DependencyNode> Nodes { get; } = new Dictionary<string, DependencyNode>();

        public void Clear()
        {
            Nodes.Clear();
        }

        public void AddDependency(string expressionKey, string dependsOnKey)
        {
            DependencyNode expressionNode = null; 
            DependencyNode dependsOnNode = null; 

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

            dependsOnNode = dependsOnNode ?? Nodes[dependsOnKey];
            expressionNode = expressionNode ?? Nodes[expressionKey];

            expressionNode.AddPredecessor(dependsOnNode);
        }

        public string[] GetDirectDependents(string nodeKey)
        {
            return Nodes[nodeKey].Dependents;
        }

        public string[] GetDependents(string key)
        {
            var dependents = new List<string>();
            if (Nodes.ContainsKey(key) == false) return dependents.ToArray();

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

        public string[] GetDirectPrecedents(string nodeKey)
        {
            return Nodes[nodeKey].Precedents;
        }

        public void RemovePrecedents(string nodeKey)
        {
            if (Nodes.ContainsKey(nodeKey)) Nodes[nodeKey].ClearPredecessors();
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
}