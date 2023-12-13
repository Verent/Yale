using System;
using System.Globalization;
using System.Reflection.Emit;
using Yale.Expression.Elements.Base.Literals;
using Yale.Parser.Internal;

namespace Yale.Expression.Elements.Literals.Real
{
    internal class DoubleLiteralElement : RealLiteralElement
    {
        private readonly double value;

        private DoubleLiteralElement() { }

        public DoubleLiteralElement(double value)
        {
            this.value = value;
        }

        public static DoubleLiteralElement? Parse(string image)
        {
            DoubleLiteralElement element = new DoubleLiteralElement();

            try
            {
                double value = double.Parse(image, CultureInfo.InvariantCulture);
                return new DoubleLiteralElement(value);
            }
            catch (OverflowException)
            {
                element.OnParseOverflow(image);
                return null;
            }
        }

        public override void Emit(YaleIlGenerator ilGenerator, ExpressionContext context)
        {
            ilGenerator.Emit(OpCodes.Ldc_R8, value);
        }

        public override Type ResultType => typeof(double);
    }
}
