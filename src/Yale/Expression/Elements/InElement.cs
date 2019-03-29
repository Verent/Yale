using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements
{
    internal class InElement : ExpressionElement
    {
        // Element we will search for
        private readonly ExpressionElement _operand;

        // Elements we will compare against
        private readonly List<ExpressionElement> _arguments;

        // Collection to look in
        private readonly ExpressionElement _targetCollectionElement;

        // Type of the collection
        private Type _targetCollectionType;

        // Initialize for searching a list of values
        public InElement(ExpressionElement operand, IList listElements)
        {
            _operand = operand;

            var elements = new ExpressionElement[listElements.Count];
            listElements.CopyTo(elements, 0);

            _arguments = new List<ExpressionElement>(elements);
            ResolveForListSearch();
        }

        // Initialize for searching a collection
        public InElement(ExpressionElement operand, ExpressionElement targetCollection)
        {
            _operand = operand;
            _targetCollectionElement = targetCollection;
            ResolveForCollectionSearch();
        }

        private void ResolveForListSearch()
        {
            var compareElement = new CompareElement();

            // Validate that our operand is comparable to all elements in the list
            foreach (var argumentElement in _arguments)
            {
                compareElement.Initialize(_operand, argumentElement, LogicalCompareOperation.Equal);
                compareElement.Validate();
            }
        }

        private void ResolveForCollectionSearch()
        {
            // Try to find a collection type
            _targetCollectionType = GetTargetCollectionType();

            if (_targetCollectionType == null)
            {
                ThrowCompileException(CompileErrors.SearchArgIsNotKnownCollectionType, CompileExceptionReason.TypeMismatch, _targetCollectionElement.ResultType.Name);
            }

            // Validate that the operand type is compatible with the collection
            var methodInfo = GetCollectionContainsMethod();
            var firstParameter = methodInfo.GetParameters()[0];

            if (ImplicitConverter.EmitImplicitConvert(_operand.ResultType, firstParameter.ParameterType, null) == false)
            {
                ThrowCompileException(CompileErrors.OperandNotConvertibleToCollectionType, CompileExceptionReason.TypeMismatch, _operand.ResultType.Name, firstParameter.ParameterType.Name);
            }
        }

        private Type GetTargetCollectionType()
        {
            var collType = _targetCollectionElement.ResultType;

            // Try to see if the collection is a generic ICollection or IDictionary
            var interfaces = collType.GetInterfaces();

            foreach (var interfaceType in interfaces)
            {
                if (interfaceType.IsGenericType == false)
                {
                    continue;
                }

                var genericTypeDef = interfaceType.GetGenericTypeDefinition();

                if (ReferenceEquals(genericTypeDef, typeof(ICollection<>)) | ReferenceEquals(genericTypeDef, typeof(IDictionary<,>)))
                {
                    return interfaceType;
                }
            }

            // Try to see if it is a regular IList or IDictionary
            if (typeof(IList<>).IsAssignableFrom(collType))
            {
                return typeof(IList<>);
            }

            if (typeof(IDictionary<,>).IsAssignableFrom(collType))
            {
                return typeof(IDictionary<,>);
            }

            // Not a known collection type
            return null;
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            if ((_targetCollectionType != null))
            {
                EmitCollectionIn(ilGenerator, context);
            }
            else
            {
                var branchManager = new BranchManager();
                branchManager.GetLabel("endLabel", ilGenerator);
                branchManager.GetLabel("trueTerminal", ilGenerator);

                // Do a fake emit to get branch positions
                var ilgTemp = CreateTempIlGenerator(ilGenerator);
                Utility.SyncFleeIlGeneratorLabels(ilGenerator, ilgTemp);

                EmitListIn(ilgTemp, context, branchManager);

                branchManager.ComputeBranches();

                // Do the real emit
                EmitListIn(ilGenerator, context, branchManager);
            }
        }

        private void EmitCollectionIn(YaleIlGenerator ilg, ExpressionContext context)
        {
            // Get the contains method
            var methodInfo = GetCollectionContainsMethod();
            var firstParameter = methodInfo.GetParameters()[0];

            // Load the collection
            _targetCollectionElement.Emit(ilg, context);
            // Load the argument
            _operand.Emit(ilg, context);
            // Do an implicit convert if necessary
            ImplicitConverter.EmitImplicitConvert(_operand.ResultType, firstParameter.ParameterType, ilg);
            // Call the contains method
            ilg.Emit(OpCodes.Callvirt, methodInfo);
        }

        private MethodInfo GetCollectionContainsMethod()
        {
            var methodName = "Contains";

            if (_targetCollectionType.IsGenericType && ReferenceEquals(_targetCollectionType.GetGenericTypeDefinition(), typeof(IDictionary<,>)))
            {
                methodName = "ContainsKey";
            }

            return _targetCollectionType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        private void EmitListIn(YaleIlGenerator ilg, ExpressionContext context, BranchManager branchManager)
        {
            var compareElement = new CompareElement();
            var endLabel = branchManager.FindLabel("endLabel");
            var trueTerminal = branchManager.FindLabel("trueTerminal");

            // Cache the operand since we will be comparing against it a lot
            var lb = ilg.DeclareLocal(_operand.ResultType);
            var targetIndex = lb.LocalIndex;

            _operand.Emit(ilg, context);
            Utility.EmitStoreLocal(ilg, targetIndex);

            // Wrap our operand in a local shim
            var targetShim = new LocalBasedElement(_operand, targetIndex);

            // Emit the compares
            foreach (var argumentElement in _arguments)
            {
                compareElement.Initialize(targetShim, argumentElement, LogicalCompareOperation.Equal);
                compareElement.Emit(ilg, context);

                EmitBranchToTrueTerminal(ilg, trueTerminal, branchManager);
            }

            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Br_S, endLabel);

            branchManager.MarkLabel(ilg, trueTerminal);
            ilg.MarkLabel(trueTerminal);

            ilg.Emit(OpCodes.Ldc_I4_1);

            branchManager.MarkLabel(ilg, endLabel);
            ilg.MarkLabel(endLabel);
        }

        private static void EmitBranchToTrueTerminal(YaleIlGenerator ilg, Label trueTerminal, BranchManager branchManager)
        {
            if (ilg.IsTemp)
            {
                branchManager.AddBranch(ilg, trueTerminal);
                ilg.Emit(OpCodes.Brtrue_S, trueTerminal);
            }
            else if (branchManager.IsLongBranch(ilg, trueTerminal) == false)
            {
                ilg.Emit(OpCodes.Brtrue_S, trueTerminal);
            }
            else
            {
                ilg.Emit(OpCodes.Brtrue, trueTerminal);
            }
        }

        public override Type ResultType => typeof(bool);
    }
}