using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base;
using Yale.Parser.Internal;
using Yale.Resources;

namespace Yale.Expression.Elements.MemberElements
{
    //Todo: remove obsolete tag?
    [Obsolete("Element representing an array index")]
    internal class IndexerElement : MemberElement
    {
        private ExpressionElement _indexerElement;

        private readonly ArgumentList _indexerElements;

        public IndexerElement(ArgumentList indexer)
        {
            _indexerElements = indexer;
        }

        protected override void ResolveInternal()
        {
            var target = Previous.TargetType;

            // Are we are indexing on an array?
            if (target.IsArray)
            {
                // Yes, so setup for an array index
                SetupArrayIndexer();
                return;
            }

            // Not an array, so try to find an indexer on the type
            if (FindIndexer(target) == false)
            {
                ThrowCompileException(CompileErrorResourceKeys.TypeNotArrayAndHasNoIndexerOfType, CompileExceptionReason.TypeMismatch, target.Name, _indexerElements);
            }
        }

        private void SetupArrayIndexer()
        {
            _indexerElement = _indexerElements[0];

            if (_indexerElements.Count > 1)
            {
                ThrowCompileException(CompileErrorResourceKeys.MultiArrayIndexNotSupported, CompileExceptionReason.TypeMismatch);
            }
            else if (ImplicitConverter.EmitImplicitConvert(_indexerElement.ResultType, typeof(Int32), null) == false)
            {
                ThrowCompileException(CompileErrorResourceKeys.ArrayIndexersMustBeOfType, CompileExceptionReason.TypeMismatch, typeof(Int32).Name);
            }
        }

        private bool FindIndexer(Type targetType)
        {
            // Get the default members
            var members = targetType.GetDefaultMembers();

            var methods = new List<MethodInfo>();

            // Use the first one that's valid for our indexer type
            foreach (var memberInfo in members)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    methods.Add(propertyInfo.GetGetMethod(true));
                }
            }

            var functionCallElement = new FunctionCallElement("Indexer", methods.ToArray(), _indexerElements);
            functionCallElement.Resolve(Context);
            _indexerElement = functionCallElement;

            return true;
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            base.Emit(ilGenerator, context);

            if (IsArray)
            {
                EmitArrayLoad(ilGenerator, context);
            }
            else
            {
                EmitIndexer(ilGenerator, context);
            }
        }

        private void EmitArrayLoad(YaleIlGenerator ilg, ExpressionContext context)
        {
            _indexerElement.Emit(ilg, context);
            ImplicitConverter.EmitImplicitConvert(_indexerElement.ResultType, typeof(Int32), ilg);

            var elementType = ResultType;

            if (elementType.IsValueType == false)
            {
                // Simple reference load
                ilg.Emit(OpCodes.Ldelem_Ref);
            }
            else
            {
                EmitValueTypeArrayLoad(ilg, elementType);
            }
        }

        private void EmitValueTypeArrayLoad(YaleIlGenerator ilg, Type elementType)
        {
            if (NextRequiresAddress)
            {
                ilg.Emit(OpCodes.Ldelema, elementType);
            }
            else
            {
                Utility.EmitArrayLoad(ilg, elementType);
            }
        }

        private void EmitIndexer(YaleIlGenerator ilg, ExpressionContext context)
        {
            var functionCallElement = (FunctionCallElement)_indexerElement;
            functionCallElement.EmitFunctionCall(NextRequiresAddress, ilg, context);
        }

        private Type ArrayType => IsArray ? Previous.TargetType : null;

        private bool IsArray => Previous.TargetType.IsArray;

        protected override bool RequiresAddress => IsArray == false;

        public override Type ResultType => IsArray ? ArrayType.GetElementType() : _indexerElement.ResultType;

        protected override bool IsPublic => IsArray || IsElementPublic((MemberElement)_indexerElement);

        public override bool IsStatic => false;
    }
}