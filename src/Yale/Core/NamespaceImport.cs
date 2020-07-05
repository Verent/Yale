using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Yale.Expression;
using Yale.Resources;

namespace Yale.Core
{
    /// <summary>
    /// Represents an imported namespace
    /// </summary>
    /// <remarks>
    /// This class acts as a container for other imports.
    /// Use it when you want to logically group expression imports.
    /// </remarks>
    internal sealed class NamespaceImport : ImportBase, ICollection<ImportBase>
    {
        private readonly string _namespace;
        private readonly List<ImportBase> _imports;

        public override string Name => _namespace;
        public override bool IsContainer => true;
        public bool IsReadOnly => false;
        public int Count => _imports.Count;

        /// <summary>
        /// Creates a new namespace import with a given namespace name
        /// </summary>
        /// <param name="importNamespace">The name of the namespace to import</param>
        /// <param name="options"></param>
        public NamespaceImport(string importNamespace, ExpressionBuilderOptions options) : base(options)
        {
            if (importNamespace == null) throw new ArgumentNullException(nameof(importNamespace));

            if (importNamespace.Length == 0)
            {
                throw new ArgumentException(GeneralErrors.InvalidNamespaceName);
            }

            _namespace = importNamespace;
            _imports = new List<ImportBase>();
        }

        protected override void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> targetCollection)
        {
            foreach (var import in NonContainerImports)
            {
                AddImportMembers(import, memberName, memberType, targetCollection);
            }
        }

        protected override void AddMembers(MemberTypes memberType, ICollection<MemberInfo> targetCollection)
        { }

        internal override Type? FindType(string typeName)
        {
            return NonContainerImports
                .Select(import => import.FindType(typeName))
                .FirstOrDefault(type => type != null);
        }

        internal override ImportBase? FindImport(string name)
        {
            foreach (var import in _imports)
            {
                if (import.IsMatch(name))
                {
                    return import;
                }
            }
            return null;
        }

        internal override bool IsMatch(string name)
        {
            return string.Equals(_namespace, name, Options.MemberStringComparison);
        }

        private ICollection<ImportBase> NonContainerImports
        {
            get
            {
                var found = new List<ImportBase>();

                foreach (var import in _imports)
                {
                    if (import.IsContainer == false)
                    {
                        found.Add(import);
                    }
                }
                return found;
            }
        }

        protected override bool EqualsInternal(ImportBase import)
        {
            return import is NamespaceImport otherSameType &&
                   _namespace.Equals(otherSameType._namespace, Options.MemberStringComparison);
        }

        public void Add(ImportBase item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _imports.Add(item);
        }

        public void Clear()
        {
            _imports.Clear();
        }

        public bool Contains(ImportBase item)
        {
            return _imports.Contains(item);
        }

        public void CopyTo(ImportBase[] array, int arrayIndex)
        {
            _imports.CopyTo(array, arrayIndex);
        }

        public bool Remove(ImportBase item)
        {
            return _imports.Remove(item);
        }

        public override IEnumerator<ImportBase> GetEnumerator()
        {
            return _imports.GetEnumerator();
        }
    }
}