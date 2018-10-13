using System;
using System.Collections.Generic;
using System.Reflection;
using Yale.Core.Interface;
using Yale.Expression;

namespace Yale.Core
{
    /// <summary>Represents an imported type</summary>
    /// <remarks>Use this class when you want to make the members of a type available to an expression</remarks>
    internal sealed class TypeImport : ImportBase
    {
        private readonly BindingFlags _bindFlags;
        private readonly bool _useTypeNameAsNamespace;

        public TypeImport(Type importType, ExpressionBuilderOptions options) : this(importType, false, options)
        {
            Options.AssertTypeIsAccessible(Target);
        }

        public TypeImport(Type importType, bool useTypeNameAsNamespace, IExpressionOptions options) : this(importType, BindingFlags.Public | BindingFlags.Static, useTypeNameAsNamespace, options)
        { }

        internal TypeImport(Type importType, BindingFlags flags, bool useTypeNameAsNamespace, IExpressionOptions options) : base(options)
        {
            Target = importType ?? throw new ArgumentNullException(nameof(importType));
            _bindFlags = flags;
            _useTypeNameAsNamespace = useTypeNameAsNamespace;
        }

        protected override void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> targetCollection)
        {
            var members = Target.FindMembers(memberType, _bindFlags, Options.MemberFilter, memberName);
            AddMemberRange(members, targetCollection);
        }

        protected override void AddMembers(MemberTypes memberType, ICollection<MemberInfo> targetCollection)
        {
            if (_useTypeNameAsNamespace) return;

            var members = Target.FindMembers(memberType, _bindFlags, AlwaysMemberFilter, null);
            AddMemberRange(members, targetCollection);
        }

        internal override bool IsMatch(string name)
        {
            return _useTypeNameAsNamespace && string.Equals(Target.Name, name, Options.MemberStringComparison);
        }

        internal override Type FindType(string typeName)
        {
            return string.Equals(typeName, Target.Name, Options.MemberStringComparison) ? Target : null;
        }

        protected override bool EqualsInternal(ImportBase import)
        {
            return import is TypeImport otherSameType && ReferenceEquals(Target, otherSameType.Target);
        }

        public override IEnumerator<ImportBase> GetEnumerator()
        {
            if (!_useTypeNameAsNamespace) return base.GetEnumerator();

            var coll = new List<ImportBase> { new TypeImport(Target, false, Options) };
            return coll.GetEnumerator();
        }

        public override bool IsContainer => _useTypeNameAsNamespace;

        public override string Name => Target.Name;

        /// <summary>Gets the type that this import represents</summary>
        /// <value>The type that this import represents</value>
        /// <remarks>Use this property to retrieve the imported type</remarks>
        public Type Target { get; }
    }
}