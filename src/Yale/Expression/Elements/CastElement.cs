using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Core;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements
{
    internal class CastElement : BaseExpressionElement
    {
        private readonly BaseExpressionElement castExpression;
        private readonly Type? destType;

        public CastElement(BaseExpressionElement castExpression, string[] destintaionTypeParts, bool isArray, ExpressionContext context)
        {
            this.castExpression = castExpression;
            destType = GetDestType(destintaionTypeParts, context);

            if (destType is null)
            {
                throw CreateCompileException(CompileErrors.CouldNotResolveType, CompileExceptionReason.UndefinedName, GetDestinationTypeString(destintaionTypeParts, isArray));
            }

            if (isArray)
            {
                destType = destType.MakeArrayType();
            }

            if (IsValidCast(this.castExpression.ResultType, destType) == false)
            {
                ThrowInvalidCastException();
            }
        }

        private static string GetDestinationTypeString(string[] parts, bool isArray)
        {
            string s = string.Join(".", parts);
            if (isArray)
            {
                s += "[]";
            }
            return s;
        }

        /// <summary>
        /// Resolve the type we are casting to
        /// </summary>
        /// <param name="destTypeParts"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static Type? GetDestType(string[] destTypeParts, ExpressionContext context)
        {
            Type? type = null;

            // Try to find a builtin type with the name
            if (destTypeParts.Length == 1)
            {
                type = ImportCollection.GetBuiltinType(destTypeParts[0]);
            }

            if (type != null)
            {
                return type;
            }

            // Try to find the type in an import
            return context.Imports.FindType(destTypeParts);
        }

        private bool IsValidCast(Type sourceType, Type destType)
        {
            if (ReferenceEquals(sourceType, destType))
            {
                // Identity cast always succeeds
                return true;
            }

            if (destType.IsAssignableFrom(sourceType))
            {
                // Cast is already implicitly valid
                return true;
            }

            if (ImplicitConverter.EmitImplicitConvert(sourceType, destType, null))
            {
                // Cast is already implicitly valid
                return true;
            }

            if (IsCastableNumericType(sourceType) & IsCastableNumericType(destType))
            {
                // Explicit cast of numeric types always succeeds
                return true;
            }

            if (sourceType.IsEnum || destType.IsEnum)
            {
                return IsValidExplicitEnumCast(sourceType, destType);
            }

            if (GetExplictOverloadedOperator(sourceType, destType) != null)
            {
                // Overloaded explict cast exists
                return true;
            }

            if (sourceType.IsValueType)
            {
                // If we get here then the cast always fails since we are either casting one value type to another
                // or a value type to an invalid reference type
                return false;
            }

            if (destType.IsValueType)
            {
                // Reference type to value type
                // Can only succeed if the reference type is a base of the value type or
                // it is one of the interfaces the value type implements
                Type[] interfaces = destType.GetInterfaces();
                return IsBaseType(destType, sourceType) || Array.IndexOf(interfaces, sourceType) != -1;
            }

            // Reference type to reference type
            return IsValidExplicitReferenceCast(sourceType, destType);
        }

        private MethodInfo? GetExplictOverloadedOperator(Type sourceType, Type destType)
        {
            ExplicitOperatorMethodBinder methodBinder = new ExplicitOperatorMethodBinder(destType, sourceType);

            // Look for an operator on the source type and dest types
            MethodInfo? miSource = Utility.GetOverloadedOperator("Explicit", sourceType, methodBinder, sourceType);
            MethodInfo? miDest = Utility.GetOverloadedOperator("Explicit", destType, methodBinder, sourceType);

            if (miSource is null & miDest is null)
            {
                return null;
            }

            if (miSource is null)
            {
                return miDest;
            }

            if (miDest is null)
            {
                return miSource;
            }

            throw CreateCompileException(CompileErrors.AmbiguousOverloadedOperator, CompileExceptionReason.AmbiguousMatch,
                sourceType.Name, destType.Name, "Explicit");
        }

        private bool IsValidExplicitEnumCast(Type sourceType, Type destType)
        {
            sourceType = GetUnderlyingEnumType(sourceType);
            destType = GetUnderlyingEnumType(destType);
            return IsValidCast(sourceType, destType);
        }

        private bool IsValidExplicitReferenceCast(Type sourceType, Type destType)
        {
            Debug.Assert(sourceType.IsValueType == false & destType.IsValueType == false, "expecting reference types");

            if (ReferenceEquals(sourceType, typeof(object)))
            {
                // From object to any other reference-type
                return true;
            }

            if (sourceType.IsArray & destType.IsArray)
            {
                // From an array-type S with an element type SE to an array-type T with an element type TE,
                // provided all of the following are true:

                // S and T have the same number of dimensions
                if (sourceType.GetArrayRank() != destType.GetArrayRank())
                {
                    return false;
                }

                Type sourceElementType = sourceType.GetElementType();
                Type destElementType = destType.GetElementType();

                // Both SE and TE are reference-types
                if (sourceElementType.IsValueType | destElementType.IsValueType)
                {
                    return false;
                }

                // An explicit reference conversion exists from SE to TE
                return IsValidExplicitReferenceCast(sourceElementType, destElementType);
            }

            if (sourceType.IsClass & destType.IsClass)
            {
                // From any class-type S to any class-type T, provided S is a base class of T
                return IsBaseType(destType, sourceType);
            }

            if (sourceType.IsClass & destType.IsInterface)
            {
                // From any class-type S to any interface-type T, provided S is not sealed and provided S does not implement T
                return sourceType.IsSealed == false & ImplementsInterface(sourceType, destType) == false;
            }

            if (sourceType.IsInterface & destType.IsClass)
            {
                // From any interface-type S to any class-type T, provided T is not sealed or provided T implements S.
                return destType.IsSealed == false | ImplementsInterface(destType, sourceType);
            }

            if (sourceType.IsInterface & destType.IsInterface)
            {
                // From any interface-type S to any interface-type T, provided S is not derived from T
                return ImplementsInterface(sourceType, destType) == false;
            }

            Debug.Assert(false, "unknown explicit cast");

            return false;
        }

        private static bool IsBaseType(Type target, Type potentialBase)
        {
            Type current = target;
            while (current != null)
            {
                if (ReferenceEquals(current, potentialBase))
                {
                    return true;
                }
                current = current.BaseType;
            }
            return false;
        }

        private static bool ImplementsInterface(Type target, Type interfaceType)
        {
            Type[] interfaces = target.GetInterfaces();
            return Array.IndexOf(interfaces, interfaceType) != -1;
        }

        private void ThrowInvalidCastException()
        {
            throw CreateCompileException(CompileErrors.CannotConvertType, CompileExceptionReason.InvalidExplicitCast, castExpression.ResultType.Name, destType.Name);
        }

        private static bool IsCastableNumericType(Type t)
        {
            return t.IsPrimitive & (!ReferenceEquals(t, typeof(bool)));
        }

        private static Type GetUnderlyingEnumType(Type t)
        {
            return t.IsEnum ? Enum.GetUnderlyingType(t) : t;
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            castExpression.Emit(ilGenerator, context);

            Type sourceType = castExpression.ResultType;
            Type? destType = this.destType;

            EmitCast(ilGenerator, sourceType, destType, context);
        }

        private void EmitCast(YaleIlGenerator ilg, Type sourceType, Type destType, ExpressionContext context)
        {
            MethodInfo? explicitOperator = GetExplictOverloadedOperator(sourceType, destType);
            if (ReferenceEquals(sourceType, destType))
            {
                // Identity cast; do nothing
                return;
            }

            if (explicitOperator != null)
            {
                ilg.Emit(OpCodes.Call, explicitOperator);
            }
            else if (sourceType.IsEnum | destType.IsEnum)
            {
                EmitEnumCast(ilg, sourceType, destType, context);
            }
            else if (ImplicitConverter.EmitImplicitConvert(sourceType, destType, ilg))
            {
                // Implicit numeric cast; do nothing
                return;
            }
            else if (IsCastableNumericType(sourceType) & IsCastableNumericType(destType))
            {
                // Explicit numeric cast
                EmitExplicitNumericCast(ilg, sourceType, destType, context);
            }
            else if (sourceType.IsValueType)
            {
                Debug.Assert(destType.IsValueType == false, "expecting reference type");
                ilg.Emit(OpCodes.Box, sourceType);
            }
            else
            {
                if (destType.IsValueType)
                {
                    // Reference type to value type
                    ilg.Emit(OpCodes.Unbox_Any, destType);
                }
                else
                {
                    // Reference type to reference type
                    if (destType.IsAssignableFrom(sourceType) == false)
                    {
                        // Only emit cast if it is an explicit cast
                        ilg.Emit(OpCodes.Castclass, destType);
                    }
                }
            }
        }

        private void EmitEnumCast(YaleIlGenerator ilg, Type sourceType, Type destType, ExpressionContext context)
        {
            if (destType.IsValueType == false)
            {
                ilg.Emit(OpCodes.Box, sourceType);
            }
            else if (sourceType.IsValueType == false)
            {
                ilg.Emit(OpCodes.Unbox_Any, destType);
            }
            else
            {
                sourceType = GetUnderlyingEnumType(sourceType);
                destType = GetUnderlyingEnumType(destType);
                EmitCast(ilg, sourceType, destType, context);
            }
        }

        private static void EmitExplicitNumericCast(YaleIlGenerator ilg, Type sourceType, Type destType, ExpressionContext context)
        {
            TypeCode desttc = Type.GetTypeCode(destType);
            TypeCode sourcetc = Type.GetTypeCode(sourceType);
            bool unsigned = IsUnsignedType(sourceType);
            ExpressionBuilderOptions options = context.BuilderOptions;
            bool overflowCheck = options.OverflowChecked;
            OpCode opCode = OpCodes.Nop;
            bool unsignedAndChecked = unsigned & overflowCheck;

            switch (desttc)
            {
                case TypeCode.SByte:

                    if (unsignedAndChecked)
                    {
                        opCode = OpCodes.Conv_Ovf_I1_Un;
                    }
                    else if (overflowCheck)
                    {
                        opCode = OpCodes.Conv_Ovf_I1;
                    }
                    else
                    {
                        opCode = OpCodes.Conv_I1;
                    }
                    break;

                case TypeCode.Byte:
                    if (unsignedAndChecked)
                    {
                        opCode = OpCodes.Conv_Ovf_U1_Un;
                    }
                    else if (overflowCheck)
                    {
                        opCode = OpCodes.Conv_Ovf_U1;
                    }
                    else
                    {
                        opCode = OpCodes.Conv_U1;
                    }
                    break;

                case TypeCode.Int16:
                    if (unsignedAndChecked)
                    {
                        opCode = OpCodes.Conv_Ovf_I2_Un;
                    }
                    else if (overflowCheck)
                    {
                        opCode = OpCodes.Conv_Ovf_I2;
                    }
                    else
                    {
                        opCode = OpCodes.Conv_I2;
                    }
                    break;

                case TypeCode.UInt16:
                    if (unsignedAndChecked)
                    {
                        opCode = OpCodes.Conv_Ovf_U2_Un;
                    }
                    else if (overflowCheck)
                    {
                        opCode = OpCodes.Conv_Ovf_U2;
                    }
                    else
                    {
                        opCode = OpCodes.Conv_U2;
                    }
                    break;

                case TypeCode.Int32:
                    if (unsignedAndChecked)
                    {
                        opCode = OpCodes.Conv_Ovf_I4_Un;
                    }
                    else if (overflowCheck)
                    {
                        opCode = OpCodes.Conv_Ovf_I4;
                    }
                    else if (sourcetc != TypeCode.UInt32)
                    {
                        // Don't need to emit a convert for this case since, to the CLR, it is the same data type
                        opCode = OpCodes.Conv_I4;
                    }
                    break;

                case TypeCode.UInt32:
                    if (unsignedAndChecked)
                    {
                        opCode = OpCodes.Conv_Ovf_U4_Un;
                    }
                    else if (overflowCheck)
                    {
                        opCode = OpCodes.Conv_Ovf_U4;
                    }
                    else if (sourcetc != TypeCode.Int32)
                    {
                        opCode = OpCodes.Conv_U4;
                    }
                    break;

                case TypeCode.Int64:
                    if (unsignedAndChecked)
                    {
                        opCode = OpCodes.Conv_Ovf_I8_Un;
                    }
                    else if (overflowCheck)
                    {
                        opCode = OpCodes.Conv_Ovf_I8;
                    }
                    else if (sourcetc != TypeCode.UInt64)
                    {
                        opCode = OpCodes.Conv_I8;
                    }
                    break;

                case TypeCode.UInt64:
                    if (unsignedAndChecked)
                    {
                        opCode = OpCodes.Conv_Ovf_U8_Un;
                    }
                    else if (overflowCheck)
                    {
                        opCode = OpCodes.Conv_Ovf_U8;
                    }
                    else if (sourcetc != TypeCode.Int64)
                    {
                        opCode = OpCodes.Conv_U8;
                    }
                    break;

                case TypeCode.Single:
                    opCode = OpCodes.Conv_R4;
                    break;

                default:
                    Debug.Assert(false, "Unknown cast dest type");
                    break;
            }

            if (opCode.Equals(OpCodes.Nop) == false)
            {
                ilg.Emit(opCode);
            }
        }

        private static bool IsUnsignedType(Type t)
        {
            TypeCode tc = Type.GetTypeCode(t);
            switch (tc)
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;

                default:
                    return false;
            }
        }

        public override Type ResultType => destType;
    }
}