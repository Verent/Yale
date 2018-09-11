using System;
using System.Collections.Generic;
using System.Reflection;
using Yale.Expression;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Core
{
    /// <summary>
    /// Represents an imported namespace
    /// </summary>
    /// <remarks>
    /// This class acts as a container for other imports.  Use it when you want to logically group expression imports.
    /// </remarks>
    public sealed class NamespaceImport : ImportBase, ICollection<ImportBase>
    {
        private readonly string _namespace;
        private readonly List<ImportBase> _imports;

        /// <summary>
        /// Creates a new namespace import with a given namespace name
        /// </summary>
        /// <param name="importNamespace">The name of the namespace to import</param>
        /// <param name="options"></param>
        public NamespaceImport(string importNamespace, ExpressionBuilderOptions options) : base(options)
        {
            Utility.AssertNotNull(importNamespace, "importNamespace");
            if (importNamespace.Length == 0)
            {
                var msg = Utility.GetGeneralErrorMessage(GeneralErrorResourceKeys.InvalidNamespaceName);
                throw new ArgumentException(msg);
            }

            _namespace = importNamespace;
            _imports = new List<ImportBase>();
        }

        protected override void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> dest)
        {
            foreach (var import in NonContainerImports)
            {
                AddImportMembers(import, memberName, memberType, dest);
            }
        }

        protected override void AddMembers(MemberTypes memberType, ICollection<MemberInfo> dest)
        { }

        internal override Type FindType(string typeName)
        {
            foreach (var import in NonContainerImports)
            {
                var type = import.FindType(typeName);

                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        internal override ImportBase FindImport(string name)
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

        public override bool IsContainer => true;

        public override string Name => _namespace;

        public void Add(ImportBase item)
        {
            if (item == null) throw new ArgumentNullException("item");
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

        public int Count => _imports.Count;

        public bool IsReadOnly => false;
    }
}