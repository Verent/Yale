using System;
using System.Diagnostics;
using Yale.Expression.Elements.Literals.Real;

namespace Yale.Expression.Elements.Base.Literals
{
    internal abstract class RealLiteralElement : LiteralElement
    {
        private static ExpressionBuilderOptions? builderOptions;

        public static LiteralElement? CreateFromInteger(string image, ExpressionBuilderOptions options)
        {
            builderOptions = options;
            LiteralElement element = CreateSingle(image);

            if (element != null)
            {
                return element;
            }

            element = CreateDecimal(image);

            if (element != null)
            {
                return element;
            }

            if (builderOptions.IntegerAsDouble)
            {
                return DoubleLiteralElement.Parse(image);
            }

            return null;
        }

        public static object? Create(string image, ExpressionBuilderOptions options)
        {
            builderOptions = options;
            LiteralElement? element = CreateSingle(image);

            if (element != null)
            {
                return element;
            }

            element = CreateDecimal(image);
            if (element != null)
            {
                return element;
            }

            element = CreateDouble(image);
            if (element != null)
            {
                return element;
            }

            element = CreateImplicitReal(image);

            return element;
        }

        private static LiteralElement? CreateImplicitReal(string image)
        {
            var realType = builderOptions.RealLiteralDataType;

            switch (realType)
            {
                case RealLiteralDataType.Double:
                    return DoubleLiteralElement.Parse(image);

                case RealLiteralDataType.Single:
                    return SingleLiteralElement.Parse(image);

                case RealLiteralDataType.Decimal:
                    return DecimalLiteralElement.Parse(image);

                default:
                    Debug.Fail("Unknown value");
                    return null;
            }
        }

        private static DoubleLiteralElement? CreateDouble(string image)
        {
            if (image.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            {
                image = image.Remove(image.Length - 1);
                return DoubleLiteralElement.Parse(image);
            }

            return null;
        }

        private static SingleLiteralElement? CreateSingle(string image)
        {
            if (image.EndsWith("f", StringComparison.OrdinalIgnoreCase))
            {
                image = image.Remove(image.Length - 1);
                return SingleLiteralElement.Parse(image);
            }

            return null;
        }

        private static DecimalLiteralElement? CreateDecimal(string image)
        {
            if (image.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                image = image.Remove(image.Length - 1);
                return DecimalLiteralElement.Parse(image);
            }

            return null;
        }
    }
}