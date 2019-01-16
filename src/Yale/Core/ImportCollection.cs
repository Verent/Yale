using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Yale.Expression;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Core
{
    public sealed class ImportCollection
    {
        private const BindingFlags OwnerFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        private const BindingFlags PublicStaticIgnoreCase = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

        internal NamespaceImport RootImport { get; }
        private TypeImport _ownerImport;
        private readonly ExpressionBuilderOptions _options;

        private static readonly Dictionary<string, Type> OurBuiltinTypeMap = CreateBuiltinTypeMap();

        internal ImportCollection(ExpressionBuilderOptions options)
        {
            _options = options;
            RootImport = new NamespaceImport("true", _options);
            ImportOwner(typeof(object));
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private static Dictionary<string, Type> CreateBuiltinTypeMap()
        {
            return new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                {"boolean", typeof(bool)},
                {"byte", typeof(byte)},
                {"sbyte", typeof(sbyte)},
                {"short", typeof(short)},
                {"ushort", typeof(UInt16)},
                {"int", typeof(Int32)},
                {"uint", typeof(UInt32)},
                {"long", typeof(long)},
                {"ulong", typeof(ulong)},
                {"single", typeof(float)},
                {"double", typeof(double)},
                {"decimal", typeof(decimal)},
                {"char", typeof(char)},
                {"object", typeof(object)},
                {"string", typeof(string)}
            };
        }

        internal void ImportOwner(Type ownerType)
        {
            _ownerImport = new TypeImport(ownerType, OwnerFlags, false, _options);
        }

        private NamespaceImport GetImport(string ns)
        {
            if (ns.Length == 0)
            {
                return RootImport;
            }

            if (!(RootImport.FindImport(ns) is NamespaceImport import))
            {
                import = new NamespaceImport(ns, _options);
                RootImport.Add(import);
            }

            return import;
        }

        internal MemberInfo[] FindOwnerMembers(string memberName, MemberTypes memberType)
        {
            return _ownerImport.FindMembers(memberName, memberType);
        }

        internal Type FindType(string[] typeNameParts)
        {
            var namespaces = new string[typeNameParts.Length - 1];
            var typeName = typeNameParts[typeNameParts.Length - 1];

            Array.Copy(typeNameParts, namespaces, namespaces.Length);
            ImportBase currentImport = RootImport;

            foreach (var ns in namespaces)
            {
                currentImport = currentImport.FindImport(ns);
                if (currentImport == null)
                {
                    break;
                }
            }

            return currentImport?.FindType(typeName);
        }

        internal static Type GetBuiltinType(string name)
        {
            if (OurBuiltinTypeMap.TryGetValue(name, out var type))
            {
                return type;
            }

            return null;
        }

        public void AddType(Type type, string @namespace)
        {
            if(type == null) throw new ArgumentNullException(nameof(type));
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
            
            const BindingFlags publicStatic = BindingFlags.Public | BindingFlags.Static;
            _options.AssertTypeIsAccessible(type);

            var import = GetImport(@namespace);
            import.Add(new TypeImport(type, publicStatic, false, _options));
        }

        public void AddType(Type type)
        {
            AddType(type, string.Empty);
        }

        public void AddMethod(string methodName, Type type, string @namespace)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
            if (methodName == null) throw new ArgumentNullException(nameof(methodName));

            var methodInfo = type.GetMethod(methodName, PublicStaticIgnoreCase);

            if (methodInfo == null)
            {
                var msg = string.Format(GeneralErrors.CouldNotFindPublicStaticMethodOnType, methodName, type.Name);
                throw new ArgumentException(msg);
            }

            AddMethod(methodInfo, @namespace);
        }

        private void AddMethod(MethodInfo methodInfo, string @namespace)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
          
            _options.AssertTypeIsAccessible(methodInfo.ReflectedType);

            if (methodInfo.IsStatic == false | methodInfo.IsPublic == false)
            {
                throw new ArgumentException(GeneralErrors.OnlyPublicStaticMethodsCanBeImported);
            }

            var import = GetImport(@namespace);
            import.Add(new MethodImport(methodInfo, _options));
        }
    }
}