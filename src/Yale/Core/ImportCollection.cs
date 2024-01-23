using Yale.Expression;
using Yale.Resources;

namespace Yale.Core;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public sealed class ImportCollection
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    private const BindingFlags OwnerFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private const BindingFlags PublicStaticIgnoreCase =
        BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

    internal NamespaceImport RootImport { get; }
    private TypeImport ownerImport;
    private readonly ExpressionBuilderOptions options;

    private static readonly Dictionary<string, Type> OurBuiltinTypeMap = CreateBuiltinTypeMap();

    internal ImportCollection(ExpressionBuilderOptions options)
    {
        this.options = options;
        RootImport = new NamespaceImport("true", this.options);
        ownerImport = new TypeImport(typeof(object), OwnerFlags, false, options);
    }

    private static Dictionary<string, Type> CreateBuiltinTypeMap()
    {
        return new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "boolean", typeof(bool) },
            { "byte", typeof(byte) },
            { "sbyte", typeof(sbyte) },
            { "short", typeof(short) },
            { "ushort", typeof(UInt16) },
            { "int", typeof(Int32) },
            { "uint", typeof(UInt32) },
            { "long", typeof(long) },
            { "ulong", typeof(ulong) },
            { "single", typeof(float) },
            { "double", typeof(double) },
            { "decimal", typeof(decimal) },
            { "char", typeof(char) },
            { "object", typeof(object) },
            { "string", typeof(string) }
        };
    }

    internal void ImportOwner(Type ownerType) =>
        ownerImport = new TypeImport(ownerType, OwnerFlags, false, options);

    private NamespaceImport GetImport(string ns)
    {
        if (ns.Length == 0)
        {
            return RootImport;
        }

        if (RootImport.FindImport(ns) is not NamespaceImport import)
        {
            import = new NamespaceImport(ns, options);
            RootImport.Add(import);
        }

        return import;
    }

    internal MemberInfo[] FindOwnerMembers(string memberName, MemberTypes memberType) =>
        ownerImport.FindMembers(memberName, memberType);

    internal Type? FindType(string[] typeNameParts)
    {
        var namespaces = new string[typeNameParts.Length - 1];
        var typeName = typeNameParts[^1];

        Array.Copy(typeNameParts, namespaces, namespaces.Length);
        ImportBase? currentImport = RootImport;

        foreach (var ns in namespaces)
        {
            currentImport = currentImport.FindImport(ns);
            if (currentImport is null)
            {
                break;
            }
        }

        return currentImport?.FindType(typeName);
    }

    internal static Type? GetBuiltinType(string name) =>
        OurBuiltinTypeMap.TryGetValue(name, out Type? type) ? type : null;

    public void AddType(Type type, string @namespace)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(@namespace);

        const BindingFlags publicStatic = BindingFlags.Public | BindingFlags.Static;
        options.AssertTypeIsAccessible(type);

        var import = GetImport(@namespace);
        import.Add(new TypeImport(type, publicStatic, false, options));
    }

    public void AddType(Type type) => AddType(type, string.Empty);

    public void AddMethod(string methodName, Type type, string @namespace)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(@namespace);
        ArgumentNullException.ThrowIfNull(methodName);

        var methodInfo = type.GetMethod(methodName, PublicStaticIgnoreCase);
        if (methodInfo is null)
        {
            var msg = string.Format(
                CultureInfo.InvariantCulture,
                GeneralErrors.CouldNotFindPublicStaticMethodOnType,
                methodName,
                type.Name
            );
            throw new ArgumentException(msg);
        }

        AddMethod(type, methodInfo, @namespace);
    }

    private void AddMethod(Type type, MethodInfo methodInfo, string @namespace)
    {
        options.AssertTypeIsAccessible(type);

        if (methodInfo.IsStatic is false | methodInfo.IsPublic is false)
        {
            throw new ArgumentException(GeneralErrors.OnlyPublicStaticMethodsCanBeImported);
        }

        var import = GetImport(@namespace);
        import.Add(new MethodImport(methodInfo, options));
    }
}
