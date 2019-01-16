using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Core;
using Yale.Core.Interface;
using Yale.Expression.Elements.Base;
using Yale.Expression.Elements.Base.Literals;
using Yale.Expression.Elements.Literals;
using Yale.Expression.Elements.Literals.Integral;
using Yale.Expression.Elements.Literals.Real;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.MemberElements
{
    internal class IdentifierElement : MemberElement
    {
        private FieldInfo _field;
        private PropertyInfo _property;
        private PropertyDescriptor _propertyDescriptor;

        //A value from the value collection
        private Type _valueType;
        
        //Another expression
        private Type _calcEngineReferenceType;

        public IdentifierElement(string name) : base(name)
        { }

        protected override void ResolveInternal()
        {
            // Property lookup (imports or object instance properties)
            if (ResolveFieldProperty(Previous))
            {
                return;
            }

            var computeInstance = Context.ComputeInstance;
            // Variable lookup
            if (Context.Variables.TryGetValue(MemberName, out IVariable value))
            {
                _valueType = value.Type;
                computeInstance?.AddDependency(Context.ExpressionName, MemberName);
                return;
            }

            // Expression lookup from compute instance
            if (computeInstance?.ContainsExpression(MemberName) == true)
            {
                computeInstance.AddDependency(Context.ExpressionName, MemberName);
                _calcEngineReferenceType = computeInstance.ResultType(MemberName);
                return;
            }

            if (Previous == null)
            {
                ThrowCompileException(CompileErrors.NoIdentifierWithName, CompileExceptionReason.UndefinedName, MemberName);
            }
            else
            {
                ThrowCompileException(CompileErrors.NoIdentifierWithNameOnType, CompileExceptionReason.UndefinedName, MemberName, Previous.TargetType.Name);
            }
        }

        private bool ResolveFieldProperty(MemberElement previous)
        {
            var allMembers = GetMembers(MemberTypes.Field | MemberTypes.Property);
            var members = GetAccessibleMembers(allMembers);

            if (members.Length == 0)
            {
                //No accessible members; try to resolve a virtual property
                return ResolveVirtualProperty(previous);
            }

            if (members.Length > 1)
            {
                // More than one accessible member
                if (previous == null)
                {
                    ThrowCompileException(CompileErrors.IdentifierIsAmbiguous, CompileExceptionReason.AmbiguousMatch, MemberName);
                }
                else
                {
                    ThrowCompileException(CompileErrors.IdentifierIsAmbiguousOnType, CompileExceptionReason.AmbiguousMatch, MemberName, previous.TargetType.Name);
                }
            }
            else
            {
                // Only one member; bind to it
                _field = members[0] as FieldInfo;
                if (_field != null)
                {
                    return true;
                }

                // Assume it must be a property
                _property = (PropertyInfo)members[0];
                return true;
            }

            return false;
        }

        private bool ResolveVirtualProperty(MemberElement previous)
        {
            if (previous == null)
            {
                // We can't use virtual properties if we are the first element
                return false;
            }

            var properties = TypeDescriptor.GetProperties(previous.ResultType);
            _propertyDescriptor = properties.Find(MemberName, true);
            return _propertyDescriptor != null;
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            base.Emit(ilGenerator, context);

            EmitFirst(ilGenerator);

            if (_calcEngineReferenceType != null)
            {
                EmitReferenceLoad(ilGenerator);
            }
            else if (_valueType != null)
            {
                EmitVariableLoad(ilGenerator);
            }
            else if (_field != null)
            {
                EmitFieldLoad(_field, ilGenerator, context);
            }
            else if (_propertyDescriptor != null)
            {
                EmitVirtualPropertyLoad(ilGenerator);
            }
            else
            {
                EmitPropertyLoad(_property, ilGenerator);
            }
        }

        private void EmitReferenceLoad(YaleIlGenerator ilg)
        {
            ilg.Emit(OpCodes.Ldarg_1);
            Context.ComputeInstance.EmitLoad(MemberName, ilg);

        }

        private void EmitFirst(YaleIlGenerator ilg)
        {
            if (Previous != null)
            {
                return;
            }

            var isVariable = _valueType != null;
            if (isVariable)
            {
                EmitLoadVariables(ilg);
            }
            else if (Context.OwnerType.IsAssignableFrom(MemberOwnerType) & IsStatic == false)
            {
                EmitLoadOwner(ilg);
            }
        }


        /// <summary>
        /// Emits il that loads a variable from the Variables collection
        /// </summary>
        /// <param name="ilg"></param>
        private void EmitVariableLoad(YaleIlGenerator ilg)
        {
            var methodInfo = VariableCollection.GetVariableLoadMethod(_valueType);
            ilg.Emit(OpCodes.Ldstr, MemberName);
            EmitMethodCall(methodInfo, ilg);
        }


        private void EmitFieldLoad(FieldInfo fi, YaleIlGenerator ilg, ExpressionContext context)
        {
            if (fi.IsLiteral)
            {
                EmitLiteral(fi, ilg, context);
            }
            else if (ResultType.IsValueType & NextRequiresAddress)
            {
                EmitLdfld(fi, true, ilg);
            }
            else
            {
                EmitLdfld(fi, false, ilg);
            }
        }

        /// <summary>
        /// Emit the value of a field in the object whose reference is currently on the evaluation stack.
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <param name="indirect"></param>
        /// <param name="ilGenerator"></param>
        private static void EmitLdfld(FieldInfo fieldInfo, bool indirect, YaleIlGenerator ilGenerator)
        {
            if (fieldInfo.IsStatic)
            {
                ilGenerator.Emit(indirect ? OpCodes.Ldsflda : OpCodes.Ldsfld, fieldInfo);
            }
            else
            {
                ilGenerator.Emit(indirect ? OpCodes.Ldflda : OpCodes.Ldfld, fieldInfo);
            }
        }

        /// <summary>
        /// Emit the load of a constant field.
        /// </summary>
        /// <remarks>
        /// We can't emit a ldsfld/ldfld of a constant so we have to get its value and then emit a ldc.
        /// </remarks>
        /// <param name="fi"></param>
        /// <param name="ilg"></param>
        /// <param name="context"></param>
        private static void EmitLiteral(FieldInfo fi, YaleIlGenerator ilg, ExpressionContext context)
        {
            var value = fi.GetValue(null);
            var type = value.GetType();
            var typeCode = Type.GetTypeCode(type);
            LiteralElement elem;

            switch (typeCode)
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    elem = new Int32LiteralElement(Convert.ToInt32(value));
                    break;

                case TypeCode.UInt32:
                    elem = new UInt32LiteralElement((UInt32)value);
                    break;

                case TypeCode.Int64:
                    elem = new Int64LiteralElement((Int64)value);
                    break;

                case TypeCode.UInt64:
                    elem = new UInt64LiteralElement((UInt64)value);
                    break;

                case TypeCode.Double:
                    elem = new DoubleLiteralElement((double)value);
                    break;

                case TypeCode.Single:
                    elem = new SingleLiteralElement((float)value);
                    break;

                case TypeCode.Boolean:
                    elem = new BooleanLiteralElement((bool)value);
                    break;

                case TypeCode.String:
                    elem = new StringLiteralElement((string)value);
                    break;

                default:
                    elem = null;
                    Debug.Fail("Unsupported constant type");
                    break;
            }

            elem.Emit(ilg, context);
        }

        private void EmitPropertyLoad(PropertyInfo pi, YaleIlGenerator ilg)
        {
            var getter = pi.GetGetMethod(true);
            EmitMethodCall(getter, ilg);
        }

        /// <summary>
        /// Load a PropertyDescriptor based property
        /// </summary>
        /// <param name="ilg"></param>
        private void EmitVirtualPropertyLoad(YaleIlGenerator ilg)
        {
            throw new NotImplementedException();
            //// The previous value is already on the top of the stack but we need it at the bottom

            //// Get a temporary local index
            //var index = ilg.GetTempLocalIndex(Previous.ResultType);

            //// Store the previous value there
            //Utility.EmitStoreLocal(ilg, index);

            //// Load the variable collection
            //EmitLoadVariables(ilg);
            //// Load the property name
            //ilg.Emit(OpCodes.Ldstr, Name);

            //// Load the previous value and convert it to object
            //Utility.EmitLoadLocal(ilg, index);
            //ImplicitConverter.EmitImplicitConvert(Previous.ResultType, typeof(object), ilg);

            //// Call the method to get the actual value
            //var methodInfo = VariableCollection.GetVirtualPropertyLoadMethod(ResultType);
            //EmitMethodCall(methodInfo, ilg);
        }

        //Todo: Get rid of this! One owner type to rule them all
        private Type MemberOwnerType
        {
            get
            {
                if (_field != null)
                {
                    return _field.ReflectedType;
                }

                if (_propertyDescriptor != null)
                {
                    return _propertyDescriptor.ComponentType;
                }

                return _property != null ? _property.ReflectedType : null;
            }
        }

        public override Type ResultType
        {
            get
            {
                if (_calcEngineReferenceType != null)
                {
                    return _calcEngineReferenceType;
                }

                if (_valueType != null)
                {
                    return _valueType;
                }

                if (_propertyDescriptor != null)
                {
                    return _propertyDescriptor.PropertyType;
                }

                if (_field != null)
                {
                    return _field.FieldType;
                }

                var methodInfo = _property.GetGetMethod(true);
                return methodInfo.ReturnType;
            }
        }

        protected override bool RequiresAddress => _propertyDescriptor == null;

        protected override bool IsPublic
        {
            get
            {
                if (_valueType != null | (_calcEngineReferenceType != null))
                {
                    return true;
                }

                if (_valueType != null)
                {
                    return true;
                }

                if (_propertyDescriptor != null)
                {
                    return true;
                }

                if (_field != null)
                {
                    return _field.IsPublic;
                }

                var methodInfo = _property.GetGetMethod(true);
                return methodInfo.IsPublic;
            }
        }

        protected override bool SupportsStatic
        {
            get
            {
                if (_valueType != null)
                {
                    // Variables never support static
                    return false;
                }

                if (_propertyDescriptor != null)
                {
                    // Neither do virtual properties
                    return false;
                }

                if (Context.OwnerType.IsAssignableFrom(MemberOwnerType) && Previous == null)
                {
                    // Owner members support static if we are the first element
                    return true;
                }

                // Support static if we are the first (ie: we are a static import)
                return Previous == null;
            }
        }

        protected override bool SupportsInstance
        {
            get
            {
                if (_valueType != null)
                {
                    // Variables always support instance
                    return true;
                }

                if (_propertyDescriptor != null)
                {
                    // So do virtual properties
                    return true;
                }

                if (Context.OwnerType.IsAssignableFrom(MemberOwnerType) && Previous == null)
                {
                    // Owner members support instance if we are the first element
                    return true;
                }

                // We always support instance if we are not the first element
                return (Previous != null);
            }
        }

        public override bool IsStatic
        {
            get
            {
                if ((_valueType != null) | (_calcEngineReferenceType != null))
                {
                    return false;
                }

                if (_valueType != null)
                {
                    return false;
                }

                if (_field != null)
                {
                    return _field.IsStatic;
                }

                if (_propertyDescriptor != null)
                {
                    return false;
                }

                var methodInfo = _property.GetGetMethod(true);
                return methodInfo.IsStatic;
            }
        }
    }
}