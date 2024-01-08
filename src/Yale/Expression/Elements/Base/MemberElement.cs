using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Core;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.Base;

internal abstract class MemberElement : BaseExpressionElement
{
    /// <summary>
    /// Previous is the user part of the expression user.address [previous.next]
    /// </summary>
    protected MemberElement Previous;

    /// <summary>
    /// Next is the address part of the expression user.address [previous.next]
    /// </summary>
    protected MemberElement Next;

    protected ExpressionContext Context;
    protected ImportBase Import;
    public ImportCollection Imports => Context.Imports;
    public VariableCollection Variables => Context.Variables;

    public const BindingFlags BindFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public string MemberName { get; protected set; }

    protected MemberElement() { }

    protected MemberElement(string name) => MemberName = name;

    public void Link(MemberElement nextElement)
    {
        Next = nextElement;
        if (nextElement is not null)
        {
            nextElement.Previous = this;
        }
    }

    public void Resolve(ExpressionContext context)
    {
        Context = context;
        ResolveInternal();
        Validate();
    }

    public void SetImport(ImportBase import) => Import = import;

    protected abstract void ResolveInternal();

    public abstract bool IsStatic { get; }
    protected abstract bool IsPublic { get; }

    protected virtual void Validate()
    {
        if (Previous is null)
        {
            return;
        }

        if (IsStatic && SupportsStatic == false)
        {
            throw CreateCompileException(
                CompileErrors.StaticMemberCannotBeAccessedWithInstanceReference,
                CompileExceptionReason.TypeMismatch,
                MemberName
            );
        }
        else if (IsStatic == false && SupportsInstance == false)
        {
            throw CreateCompileException(
                CompileErrors.ReferenceToNonSharedMemberRequiresObjectReference,
                CompileExceptionReason.TypeMismatch,
                MemberName
            );
        }
    }

    public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context) =>
        Previous?.Emit(ilGenerator, context);

    /// <summary>
    /// Loads the 2nd argument to the evaluation stack.
    /// </summary>
    /// <param name="ilGenerator"></param>
    protected static void EmitLoadVariables(YaleIlGenerator ilGenerator) =>
        ilGenerator.Emit(OpCodes.Ldarg_2);

    /// <summary>
    /// Handles a call emit for static, instance methods of reference/value types
    /// </summary>
    /// <param name="mi"></param>
    /// <param name="ilg"></param>
    protected void EmitMethodCall(MethodInfo mi, YaleIlGenerator ilg) =>
        EmitMethodCall(ResultType, NextRequiresAddress, mi, ilg);

    protected static void EmitMethodCall(
        Type resultType,
        bool nextRequiresAddress,
        MethodInfo mi,
        YaleIlGenerator ilg
    )
    {
        if (mi.GetType().IsValueType == false)
        {
            EmitReferenceTypeMethodCall(mi, ilg);
        }
        else
        {
            EmitValueTypeMethodCall(mi, ilg);
        }

        if (resultType.IsValueType & nextRequiresAddress)
        {
            EmitValueTypeLoadAddress(ilg, resultType);
        }
    }

    protected static bool IsGetTypeMethod(MethodInfo mi)
    {
        MethodInfo miGetType = typeof(object).GetMethod(
            "gettype",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
        );
        return mi.MethodHandle.Equals(miGetType.MethodHandle);
    }

    /// <summary>
    /// Emit a function call for a value type
    /// </summary>
    /// <param name="mi"></param>
    /// <param name="ilg"></param>
    private static void EmitValueTypeMethodCall(MethodInfo mi, YaleIlGenerator ilg)
    {
        if (mi.IsStatic)
        {
            ilg.Emit(OpCodes.Call, mi);
        }
        else if ((!ReferenceEquals(mi.DeclaringType, mi.ReflectedType)))
        {
            // Method is not defined on the value type
            if (IsGetTypeMethod(mi))
            {
                // Special GetType method which requires a box
                ilg.Emit(OpCodes.Box, mi.ReflectedType);
                ilg.Emit(OpCodes.Call, mi);
            }
            else
            {
                // Equals, GetHashCode, and ToString methods on the base
                ilg.Emit(OpCodes.Constrained, mi.ReflectedType);
                ilg.Emit(OpCodes.Callvirt, mi);
            }
        }
        else
        {
            //Call value type implementation
            ilg.Emit(OpCodes.Call, mi);
        }
    }

    private static void EmitReferenceTypeMethodCall(MethodInfo mi, YaleIlGenerator ilg) =>
        ilg.Emit(mi.IsStatic ? OpCodes.Call : OpCodes.Callvirt, mi);

    protected static void EmitValueTypeLoadAddress(YaleIlGenerator ilg, Type targetType)
    {
        int index = ilg.GetTempLocalIndex(targetType);
        Utility.EmitStoreLocal(ilg, index);
        ilg.Emit(OpCodes.Ldloca_S, Convert.ToByte(index));
    }

    protected void EmitLoadOwner(YaleIlGenerator ilg)
    {
        ilg.Emit(OpCodes.Ldarg_0);

        Type? ownerType = Context.OwnerType;

        if (ownerType.IsValueType == false)
        {
            return;
        }

        ilg.Emit(OpCodes.Unbox, ownerType);
        ilg.Emit(OpCodes.Ldobj, ownerType);

        // Emit usual stuff for value types but use the owner type as the target
        if (RequiresAddress)
        {
            EmitValueTypeLoadAddress(ilg, ownerType);
        }
    }

    /// <summary>
    /// Determine if a field, property, or method is public
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    private static bool IsMemberPublic(MemberInfo member)
    {
        var fieldInfo = member as FieldInfo;
        if (fieldInfo is not null)
        {
            return fieldInfo.IsPublic;
        }

        var propertyInfo = member as PropertyInfo;
        if (propertyInfo is not null)
        {
            var method = propertyInfo.GetGetMethod(true);
            return method?.IsPublic ?? false;
        }

        var methodInfo = member as MethodInfo;
        if (methodInfo is not null)
        {
            return methodInfo.IsPublic;
        }

        //Todo: handle error case propperly
        Debug.Assert(false, "Unknown member type");
        return false;
    }

    protected static MemberInfo[] GetAccessibleMembers(MemberInfo[] members)
    {
        List<MemberInfo> accessible = new();

        foreach (var memberInfo in members)
        {
            if (IsMemberAccessible(memberInfo))
            {
                accessible.Add(memberInfo);
            }
        }

        return accessible.ToArray();
    }

    /// <summary>
    /// Only public members are accessible.
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public static bool IsMemberAccessible(MemberInfo member) => IsMemberPublic(member);

    protected MemberInfo[] GetMembers(MemberTypes targets)
    {
        if (Previous is null)
        {
            // Do we have a namespace?
            if (Import is null)
            {
                // Get all members in the default namespace
                return GetDefaultNamespaceMembers(MemberName, targets);
            }

            return Import.FindMembers(MemberName, targets);
        }

        // We are not the first element; find all members with our name on the type of the previous member
        return Previous.TargetType.FindMembers(
            targets,
            BindFlags,
            Context.BuilderOptions.MemberFilter,
            MemberName
        );
    }

    /// <summary>
    /// Find members in the default namespace
    /// </summary>
    /// <param name="name"></param>
    /// <param name="memberType"></param>
    /// <returns></returns>
    protected MemberInfo[] GetDefaultNamespaceMembers(string name, MemberTypes memberType)
    {
        // Search the owner first
        MemberInfo[] members = Imports.FindOwnerMembers(name, memberType);

        // Keep only the accessible members
        members = GetAccessibleMembers(members);

        // If we have some matches, return them. Else search the imports.
        return members.Length > 0 ? members : Imports.RootImport.FindMembers(name, memberType);
    }

    protected static bool IsElementPublic(MemberElement e) => e.IsPublic;

    protected bool NextRequiresAddress => Next != null && Next.RequiresAddress;

    protected virtual bool RequiresAddress => false;

    protected virtual bool SupportsInstance => true;

    protected virtual bool SupportsStatic => false;

    public Type TargetType => ResultType;
}
