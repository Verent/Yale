using Yale.Expression;
using Yale.Resources;

namespace Yale.Core;

/// <summary>
/// Represents an imported namespace
/// </summary>
/// <remarks>
/// This class acts as a container for other imports.
/// Use it when you want to logically group expression imports.
/// </remarks>
internal sealed class NamespaceImport : ImportBase, ICollection<ImportBase>
{
    private readonly string @namespace;
    private readonly List<ImportBase> imports;

    public override string Name => @namespace;
    public override bool IsContainer => true;
    public bool IsReadOnly => false;
    public int Count => imports.Count;

    /// <summary>
    /// Creates a new namespace import with a given namespace name
    /// </summary>
    /// <param name="importNamespace">The name of the namespace to import</param>
    /// <param name="options"></param>
    public NamespaceImport(string importNamespace, ExpressionBuilderOptions options)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(importNamespace);

        if (importNamespace.Length == 0)
        {
            throw new ArgumentException(GeneralErrors.InvalidNamespaceName);
        }

        @namespace = importNamespace;
        imports = new List<ImportBase>();
    }

    protected override void AddMembers(
        string memberName,
        MemberTypes memberType,
        ICollection<MemberInfo> targetCollection
    )
    {
        foreach (var import in NonContainerImports)
        {
            AddImportMembers(import, memberName, memberType, targetCollection);
        }
    }

    protected override void AddMembers(
        MemberTypes memberType,
        ICollection<MemberInfo> targetCollection
    ) { }

    internal override Type? FindType(string typeName)
    {
        return NonContainerImports
            .Select(import => import.FindType(typeName))
            .FirstOrDefault(type => type is not null);
    }

    internal override ImportBase? FindImport(string name)
    {
        foreach (var import in imports)
        {
            if (import.IsMatch(name))
            {
                return import;
            }
        }
        return null;
    }

    internal override bool IsMatch(string name) =>
        string.Equals(@namespace, name, Options.MemberStringComparison);

    private ICollection<ImportBase> NonContainerImports
    {
        get
        {
            List<ImportBase> found = new();

            foreach (var import in imports)
            {
                if (import.IsContainer == false)
                {
                    found.Add(import);
                }
            }
            return found;
        }
    }

    protected override bool EqualsInternal(ImportBase? import)
    {
        return import is NamespaceImport otherSameType
            && @namespace.Equals(otherSameType.@namespace, Options.MemberStringComparison);
    }

    public void Add(ImportBase item)
    {
        ArgumentNullException.ThrowIfNull(item);
        imports.Add(item);
    }

    public void Clear() => imports.Clear();

    public bool Contains(ImportBase item) => imports.Contains(item);

    public void CopyTo(ImportBase[] array, int arrayIndex) => imports.CopyTo(array, arrayIndex);

    public bool Remove(ImportBase item) => imports.Remove(item);

    public override IEnumerator<ImportBase> GetEnumerator() => imports.GetEnumerator();
}
