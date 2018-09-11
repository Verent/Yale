using System;
using Yale.Expression;
using Yale.Expression.Elements.Base;

namespace Yale.Parser.Internal
{
    /// <summary>
    /// Wraps an expression element so that it is loaded from a local slot
    /// </summary>
    internal class LocalBasedElement : ExpressionElement
    {
        private readonly int _index;

        private readonly ExpressionElement _target;

        public LocalBasedElement(ExpressionElement target, int index)
        {
            _target = target;
            _index = index;
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            Utility.EmitLoadLocal(ilGenerator, _index);
        }

        public override Type ResultType => _target.ResultType;
    }
}