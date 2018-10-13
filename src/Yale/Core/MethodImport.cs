using System;
using System.Collections.Generic;
using System.Reflection;
using Yale.Core.Interface;

namespace Yale.Core
{
    /// <summary>
    /// Represents a single imported method
    /// </summary>
    internal sealed class MethodImport : ImportBase
    {
        public MethodImport(MethodInfo importMethod, IExpressionOptions options) : base(options)
        {
            Target = importMethod ?? throw new ArgumentNullException(nameof(importMethod));
        }

        protected override void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> targetCollection)
        {
            if (string.Equals(memberName, Target.Name, Options.MemberStringComparison) &&
                (memberType & MemberTypes.Method) != 0)
            {
                targetCollection.Add(Target);
            }
        }

        protected override void AddMembers(MemberTypes memberType, ICollection<MemberInfo> targetCollection)
        {
            if ((memberType & MemberTypes.Method) != 0)
            {
                targetCollection.Add(Target);
            }
        }

        internal override bool IsMatch(string name)
        {
            return string.Equals(Target.Name, name, Options.MemberStringComparison);
        }

        internal override Type FindType(string typeName)
        {
            return null;
        }

        protected override bool EqualsInternal(ImportBase import)
        {
            return import is MethodImport otherSameType && Target.MethodHandle.Equals(otherSameType.Target.MethodHandle);
        }

        public override string Name => Target.Name;

        /// <summary>
        /// Gets the method that this import represents
        /// </summary>
        public MethodInfo Target { get; }
    }
}