using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Yale.Core.Interface;
using Yale.Parser.Internal;

namespace Yale.Core
{
    /// <summary>
    /// Base class for all expression imports.
    /// </summary>
    /// <remarks>
    /// Used to make functions and objects available in the calculation context.
    /// </remarks>
    public abstract class ImportBase : IEnumerable<ImportBase>, IEquatable<ImportBase>
    {
        internal ImportBase(IExpressionOptions options)
        {
            Utility.AssertNotNull(options, "options");
            Options = options;
        }

        protected abstract void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> destination);

        protected abstract void AddMembers(MemberTypes memberType, ICollection<MemberInfo> destination);

        protected static void AddImportMembers(ImportBase import, string memberName, MemberTypes memberType, ICollection<MemberInfo> destination)
        {
            import.AddMembers(memberName, memberType, destination);
        }

        protected static void AddImportMembers(ImportBase import, MemberTypes memberType, ICollection<MemberInfo> destination)
        {
            import.AddMembers(memberType, destination);
        }

        protected static void AddMemberRange(ICollection<MemberInfo> members, ICollection<MemberInfo> destination)
        {
            foreach (var memberInfo in members)
            {
                destination.Add(memberInfo);
            }
        }

        protected bool AlwaysMemberFilter(MemberInfo member, object criteria)
        {
            return true;
        }

        internal abstract bool IsMatch(string name);

        internal abstract Type FindType(string typeName);

        internal virtual ImportBase FindImport(string name)
        {
            //Todo: wtf?
            return null;
        }

        internal MemberInfo[] FindMembers(string memberName, MemberTypes memberType)
        {
            var found = new List<MemberInfo>();
            AddMembers(memberName, memberType, found);
            return found.ToArray();
        }

        public MemberInfo[] GetMembers(MemberTypes memberType)
        {
            var found = new List<MemberInfo>();
            AddMembers(memberType, found);
            return found.ToArray();
        }

        public virtual IEnumerator<ImportBase> GetEnumerator()
        {
            var coll = new List<ImportBase>();
            return coll.GetEnumerator();
        }

        private IEnumerator GetEnumerator1()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        public bool Equals(ImportBase other)
        {
            return EqualsInternal(other);
        }

        protected abstract bool EqualsInternal(ImportBase import);

        internal IExpressionOptions Options { get; }

        /// <summary>Gets the name of the import</summary>
        /// <value>The name of the current import instance</value>
        /// <remarks>Use this property to get the name of the import</remarks>
        public abstract string Name { get; }

        /// <summary> Determines if this import can contain other imports </summary>
        /// <value>True if this import can contain other imports; False otherwise</value>
        /// <remarks>Use this property to determine if this import contains other imports</remarks>
        public virtual bool IsContainer => false;
    }
}