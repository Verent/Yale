using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.MemberElements
{
    internal class FunctionCallElement : MemberElement
    {
        private readonly ArgumentList arguments;
        private readonly ICollection<MethodInfo> methods;

        private CustomMethodInfo targetMethodInfo;

        public FunctionCallElement(string name, ArgumentList arguments) : base(name)
        {
            this.arguments = arguments;
        }

        internal FunctionCallElement(string name, ICollection<MethodInfo> methods, ArgumentList arguments) : base(name)
        {
            this.arguments = arguments;
            this.methods = methods;
        }

        protected override void ResolveInternal()
        {
            // Get the types of our arguments
            var argTypes = arguments.GetArgumentTypes();
            // Find all methods with our name on the type
            var methods = this.methods;

            if (methods == null)
            {
                // Convert member info to method info
                var arr = GetMembers(MemberTypes.Method);
                var arr2 = new MethodInfo[arr.Length];
                Array.Copy(arr, arr2, arr.Length);
                methods = arr2;
            }

            if (methods.Any())
            {
                // More than one method exists with this name
                BindToMethod(methods, Previous, argTypes);
                return;
            }

            // No methods with this name exist; try to bind to an on-demand function
            //_onDemandFunctionReturnType = Context.Variables.ResolveOnDemandFunction(Name, argTypes);
            //if (_onDemandFunctionReturnType == null)
            //{
            //}

            ThrowFunctionNotFoundException(Previous);
        }

        private void ThrowFunctionNotFoundException(MemberElement previous)
        {
            if (previous == null)
            {
                throw CreateCompileException(CompileErrors.UndefinedFunction, CompileExceptionReason.UndefinedName, MemberName, arguments);
            }
            else
            {
                throw CreateCompileException(CompileErrors.UndefinedFunctionOnType, CompileExceptionReason.UndefinedName, MemberName, arguments, previous.TargetType.Name);
            }
        }

        private void ThrowNoAccessibleMethodsException(MemberElement previous)
        {
            if (previous == null)
            {
                throw CreateCompileException(CompileErrors.NoAccessibleMatches, CompileExceptionReason.AccessDenied, MemberName, arguments);
            }
            else
            {
                throw CreateCompileException(CompileErrors.NoAccessibleMatchesOnType, CompileExceptionReason.AccessDenied, MemberName, arguments, previous.TargetType.Name);
            }
        }

        private void ThrowAmbiguousMethodCallException()
        {
            throw CreateCompileException(CompileErrors.AmbiguousCallOfFunction, CompileExceptionReason.AmbiguousMatch, MemberName, arguments);
        }

        /// <summary>
        /// Try to find a match from a set of methods
        /// </summary>
        /// <param name="methods"></param>
        /// <param name="previous"></param>
        /// <param name="argTypes"></param>
        private void BindToMethod(ICollection<MethodInfo> methods, MemberElement previous, Type[] argTypes)
        {
            var customInfoList = new List<CustomMethodInfo>();

            // Wrap the MethodInfo in our custom class
            foreach (var methodInfo in methods)
            {
                var customMethodInfo = new CustomMethodInfo(methodInfo);
                customInfoList.Add(customMethodInfo);
            }

            // Discard any methods that cannot qualify as overloads
            var infoArray = customInfoList.ToArray();
            customInfoList.Clear();

            foreach (var methodInfo in infoArray)
            {
                if (methodInfo.IsMatch(argTypes))
                {
                    customInfoList.Add(methodInfo);
                }
            }

            if (customInfoList.Count == 0)
            {
                // We have no methods that can qualify as overloads; throw exception
                ThrowFunctionNotFoundException(previous);
            }
            else
            {
                // At least one method matches our criteria; do our custom overload resolution
                ResolveOverloads(customInfoList.ToArray(), previous, argTypes);
            }
        }

        /// <summary>
        /// Find the best match from a set of overloaded methods
        /// </summary>
        /// <param name="customInfoArray"></param>
        /// <param name="previous"></param>
        /// <param name="argTypes"></param>
        private void ResolveOverloads(CustomMethodInfo[] customInfoArray, MemberElement previous, Type[] argTypes)
        {
            // Compute a score for each candidate
            foreach (var customMethodInfo in customInfoArray)
            {
                customMethodInfo.ComputeScore(argTypes);
            }

            // Sort array from best to worst matches
            Array.Sort(customInfoArray);

            // Discard any matches that aren't accessible
            customInfoArray = GetAccessibleInfos(customInfoArray);

            // No accessible methods left
            if (customInfoArray.Length == 0)
            {
                ThrowNoAccessibleMethodsException(previous);
            }

            // Handle case where we have more than one match with the same score
            DetectAmbiguousMatches(customInfoArray);

            // If we get here, then there is only one best match
            targetMethodInfo = customInfoArray[0];
        }

        private CustomMethodInfo[] GetAccessibleInfos(CustomMethodInfo[] infos)
        {
            var accessible = new List<CustomMethodInfo>();

            foreach (var customMethodInfo in infos)
            {
                if (customMethodInfo.IsAccessible(this))
                {
                    accessible.Add(customMethodInfo);
                }
            }

            return accessible.ToArray();
        }

        /// <summary>
        ///  Handle case where we have overloads with the same score
        /// </summary>
        /// <param name="infos"></param>
        private void DetectAmbiguousMatches(CustomMethodInfo[] infos)
        {
            var sameScores = new List<CustomMethodInfo>();
            var first = infos[0];

            // Find all matches with the same score as the best match
            foreach (var customMethodInfo in infos)
            {
                if (((IEquatable<CustomMethodInfo>)customMethodInfo).Equals(first))
                {
                    sameScores.Add(customMethodInfo);
                }
            }

            // More than one accessible match with the same score exists
            if (sameScores.Count > 1)
            {
                ThrowAmbiguousMethodCallException();
            }
        }

        protected override void Validate()
        {
            base.Validate();

            //Todo: What is this?
            //if (_myOnDemandFunctionReturnType != null)
            //{
            //    return;
            //}

            // Any function reference in an expression must return a value
            if (ReferenceEquals(Method.ReturnType, typeof(void)))
            {
                throw CreateCompileException(CompileErrors.FunctionHasNoReturnValue, CompileExceptionReason.FunctionHasNoReturnValue, MemberName);
            }
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            base.Emit(ilGenerator, context);

            //Todo: What is this?
            //var elements = _arguments.ToArray();
            // If we are an on-demand function, then emit that and exit
            //if (_myOnDemandFunctionReturnType != null)
            //{
            //    EmitOnDemandFunction(elements, ilg, services);
            //    return;
            //}

            var isOwnerMember = Context.OwnerType.IsAssignableFrom(Method.ReflectedType);

            // Load the owner if required
            if (Previous == null && isOwnerMember && IsStatic == false)
            {
                EmitLoadOwner(ilGenerator);
            }

            EmitFunctionCall(NextRequiresAddress, ilGenerator, context);
        }

        // Emit the arguments to a paramArray method call
        private void EmitParamArrayArguments(ParameterInfo[] parameters, BaseExpressionElement[] elements, YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            // Get the fixed parameters
            var fixedParameters = new ParameterInfo[targetMethodInfo.FixedArgTypes.Length];
            Array.Copy(parameters, fixedParameters, fixedParameters.Length);

            // Get the corresponding fixed parameters
            var fixedElements = new BaseExpressionElement[targetMethodInfo.FixedArgTypes.Length];
            Array.Copy(elements, fixedElements, fixedElements.Length);

            // Emit the fixed arguments
            EmitRegularFunctionInternal(fixedParameters, fixedElements, ilGenerator, context);

            // Get the paramArray arguments
            var paramArrayElements = new BaseExpressionElement[elements.Length - fixedElements.Length];
            Array.Copy(elements, fixedElements.Length, paramArrayElements, 0, paramArrayElements.Length);

            // Emit them into an array
            EmitElementArrayLoad(paramArrayElements, targetMethodInfo.ParamArrayElementType, ilGenerator, context);
        }

        /// <summary>
        /// Emit elements into an array
        /// </summary>
        private static void EmitElementArrayLoad(BaseExpressionElement[] elements, Type arrayElementType, YaleIlGenerator ilg, ExpressionContext context)
        {
            // Load the array length
            LiteralElement.EmitLoad(elements.Length, ilg);

            // Create the array
            ilg.Emit(OpCodes.Newarr, arrayElementType);

            // Store the new array in a unique local and remember the index
            var local = ilg.DeclareLocal(arrayElementType.MakeArrayType());
            var arrayLocalIndex = local.LocalIndex;
            Utility.EmitStoreLocal(ilg, arrayLocalIndex);

            for (var i = 0; i <= elements.Length - 1; i++)
            {
                // Load the array
                Utility.EmitLoadLocal(ilg, arrayLocalIndex);
                // Load the index
                LiteralElement.EmitLoad(i, ilg);
                // Emit the element (with any required conversions)
                var element = elements[i];
                element.Emit(ilg, context);
                ImplicitConverter.EmitImplicitConvert(element.ResultType, arrayElementType, ilg);
                // Store it into the array
                Utility.EmitArrayStore(ilg, arrayElementType);
            }

            // Load the array
            Utility.EmitLoadLocal(ilg, arrayLocalIndex);
        }

        public void EmitFunctionCall(bool nextRequiresAddress, YaleIlGenerator ilg, ExpressionContext context)
        {
            var parameters = Method.GetParameters();
            var elements = arguments.ToArray();

            // Emit either a regular or paramArray call
            if (targetMethodInfo.IsParamArray == false)
            {
                EmitRegularFunctionInternal(parameters, elements, ilg, context);
            }
            else
            {
                EmitParamArrayArguments(parameters, elements, ilg, context);
            }

            EmitMethodCall(ResultType, nextRequiresAddress, Method, ilg);
        }

        /// <summary>
        ///  Emit the arguments to a regular method call
        /// </summary>
        private void EmitRegularFunctionInternal(ParameterInfo[] parameters, BaseExpressionElement[] elements, YaleIlGenerator ilg, ExpressionContext context)
        {
            Debug.Assert(parameters.Length == elements.Length, "argument count mismatch");

            // Emit each element and any required conversions to the actual parameter type
            for (var i = 0; i <= parameters.Length - 1; i++)
            {
                var element = elements[i];
                var pi = parameters[i];
                element.Emit(ilg, context);
                var success = ImplicitConverter.EmitImplicitConvert(element.ResultType, pi.ParameterType, ilg);
                Debug.Assert(success, "conversion failed");
            }
        }

        /// <summary>
        /// The method info we will be calling
        /// </summary>
        private MethodInfo Method => targetMethodInfo.Target;

        public override Type ResultType => Method.ReturnType;

        protected override bool RequiresAddress => !IsGetTypeMethod(Method);

        protected override bool IsPublic => Method.IsPublic;

        public override bool IsStatic => Method.IsStatic;
    }
}