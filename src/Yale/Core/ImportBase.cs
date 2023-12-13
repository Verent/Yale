using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Yale.Core.Interfaces;

namespace Yale.Core;

/// <summary>
/// Base class for all expression imports.
/// </summary>
internal abstract class ImportBase : IEnumerable<ImportBase>, IEquatable<ImportBase>
{
    protected ImportBase(IExpressionOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    protected abstract void AddMembers(
        string memberName,
        MemberTypes memberType,
        ICollection<MemberInfo> targetCollection
    );

    protected abstract void AddMembers(
        MemberTypes memberType,
        ICollection<MemberInfo> targetCollection
    );

    protected static void AddImportMembers(
        ImportBase import,
        string memberName,
        MemberTypes memberType,
        ICollection<MemberInfo> destination
    )
    {
        import.AddMembers(memberName, memberType, destination);
    }

    protected static void AddImportMembers(
        ImportBase import,
        MemberTypes memberType,
        ICollection<MemberInfo> destination
    )
    {
        import.AddMembers(memberType, destination);
    }

    protected static void AddMemberRange(
        ICollection<MemberInfo> members,
        ICollection<MemberInfo> destination
    )
    {
        foreach (MemberInfo memberInfo in members)
        {
            destination.Add(memberInfo);
        }
    }

    //Todo: Reimplement
    protected bool AlwaysMemberFilter(MemberInfo member, object criteria) => true;

    internal abstract bool IsMatch(string name);

    internal abstract Type? FindType(string typeName);

    //Todo: Make this abstract
    internal virtual ImportBase? FindImport(string name)
    {
        return null;
    }

    internal MemberInfo[] FindMembers(string memberName, MemberTypes memberType)
    {
        List<MemberInfo> found = new List<MemberInfo>();
        AddMembers(memberName, memberType, found);
        return found.ToArray();
    }

    public MemberInfo[] GetMembers(MemberTypes memberType)
    {
        List<MemberInfo> found = new List<MemberInfo>();
        AddMembers(memberType, found);
        return found.ToArray();
    }

    public virtual IEnumerator<ImportBase> GetEnumerator()
    {
        List<ImportBase> imports = new List<ImportBase>();
        return imports.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(ImportBase other)
    {
        return EqualsInternal(other);
    }

    protected abstract bool EqualsInternal(ImportBase import);

    internal IExpressionOptions Options { get; }

    /// <summary>
    /// Gets the name of the import
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Determines if this import can contain other imports
    /// </summary>
    public virtual bool IsContainer => false;
}
