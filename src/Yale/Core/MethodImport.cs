using System;
using System.Collections.Generic;
using System.Reflection;
using Yale.Core.Interface;

namespace Yale.Core
{
    /// <summary>Represents an imported method</summary>
    /// <remarks>Use this class when you want to make a single method available to an expression</remarks>
    public sealed class MethodImport : ImportBase
    {
        public MethodImport(MethodInfo importMethod, IExpressionOptions options) : base(options)
        {
            if (importMethod == null) throw new ArgumentNullException(nameof(importMethod));
            Target = importMethod;
        }

        protected override void AddMembers(string memberName, MemberTypes memberType, ICollection<MemberInfo> destination)
        {
            if (string.Equals(memberName, Target.Name, Options.MemberStringComparison) &&
                (memberType & MemberTypes.Method) != 0)
            {
                destination.Add(Target);
            }
        }

        protected override void AddMembers(MemberTypes memberType, ICollection<MemberInfo> destination)
        {
            if ((memberType & MemberTypes.Method) != 0)
            {
                destination.Add(Target);
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

        /// <summary>Gets the method that this import represents</summary>
        /// <value>The method that this import represents</value>
        /// <remarks>Use this property to retrieve the imported method</remarks>
        public MethodInfo Target { get; }
    }
}