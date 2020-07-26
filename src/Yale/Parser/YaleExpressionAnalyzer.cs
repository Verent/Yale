using PerCederberg.Grammatica.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Yale.Expression;
using Yale.Expression.Elements;
using Yale.Expression.Elements.Base;
using Yale.Expression.Elements.Base.Literals;
using Yale.Expression.Elements.Literals;
using Yale.Expression.Elements.Literals.Integral;
using Yale.Expression.Elements.LogicalBitwise;
using Yale.Expression.Elements.MemberElements;
using Yale.Parser.Internal;

namespace Yale.Parser
{
    /// <summary>
    /// A class providing callback methods for the parser.
    /// This extends ExpressionAnalyzer with Yale specific callback methods
    /// </summary>
    internal class YaleExpressionAnalyzer : ExpressionAnalyzer
    {
        private readonly Regex unicodeEscapeRegex = new Regex("\\\\u[0-9a-f]{4}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Regex regularEscapeRegex = new Regex("\\\\[\\\\\"'trn]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private bool inUnaryNegate;
        private ExpressionContext? context;

        public void SetContext(ExpressionContext context)
        {
            this.context = context;
        }

        public override void Reset()
        {
            context = null;
        }

        public override Node ExitExpression(Production node)
        {
            AddFirstChildValue(node);
            return node;
        }

        public override Node ExitExpressionGroup(Production node)
        {
            node.AddValues(GetChildValues(node));
            return node;
        }

        public override Node ExitXorExpression(Production node)
        {
            AddBinaryOp(node, typeof(XorElement));
            return node;
        }

        public override Node ExitOrExpression(Production node)
        {
            AddBinaryOp(node, typeof(AndOrElement));
            return node;
        }

        public override Node ExitAndExpression(Production node)
        {
            AddBinaryOp(node, typeof(AndOrElement));
            return node;
        }

        public override Node ExitNotExpression(Production node)
        {
            AddUnaryOp(node, typeof(NotElement));
            return node;
        }

        public override Node ExitCompareExpression(Production node)
        {
            AddBinaryOp(node, typeof(CompareElement));
            return node;
        }

        public override Node ExitShiftExpression(Production node)
        {
            AddBinaryOp(node, typeof(ShiftElement));
            return node;
        }

        public override Node ExitAdditiveExpression(Production node)
        {
            AddBinaryOp(node, typeof(ArithmeticElement));
            return node;
        }

        public override Node ExitMultiplicativeExpression(Production node)
        {
            AddBinaryOp(node, typeof(ArithmeticElement));
            return node;
        }

        public override Node ExitPowerExpression(Production node)
        {
            AddBinaryOp(node, typeof(ArithmeticElement));
            return node;
        }

        // Try to fold a negated constant int32.  We have to do this so that parsing int32.MinValue will work
        public override Node ExitNegateExpression(Production node)
        {
            IList childValues = GetChildValues(node);

            // Get last child
            var childElement = (BaseExpressionElement)childValues[childValues.Count - 1];

            // Is it an signed integer constant?
            if (ReferenceEquals(childElement.GetType(), typeof(Int32LiteralElement)) & childValues.Count == 2)
            {
                ((Int32LiteralElement)childElement).Negate();
                // Add it directly instead of the negate element since it will already be negated
                node.AddValue(childElement);
            }
            else if (ReferenceEquals(childElement.GetType(), typeof(Int64LiteralElement)) & childValues.Count == 2)
            {
                ((Int64LiteralElement)childElement).Negate();
                // Add it directly instead of the negate element since it will already be negated
                node.AddValue(childElement);
            }
            else
            {
                // No so just add a regular negate
                AddUnaryOp(node, typeof(NegateElement));
            }

            return node;
        }

        public override Node ExitMemberExpression(Production node)
        {
            IList childValues = GetChildValues(node);
            var first = childValues[0];

            if (childValues.Count == 1 && !(first is MemberElement))
            {
                node.AddValue(first);
            }
            else
            {
                var invocationListElement = new InvocationListElement(childValues, context);
                node.AddValue(invocationListElement);
            }

            return node;
        }

        public override Node ExitIndexExpression(Production node)
        {
            IList childValues = GetChildValues(node);
            var args = new ArgumentList(childValues);
            var e = new IndexerElement(args);
            node.AddValue(e);
            return node;
        }

        public override Node ExitMemberAccessExpression(Production node)
        {
            node.AddValue(node.GetChildAt(1).GetValue(0));
            return node;
        }

        public override Node ExitSpecialFunctionExpression(Production node)
        {
            AddFirstChildValue(node);
            return node;
        }

        public override Node ExitIfExpression(Production node)
        {
            IList childValues = GetChildValues(node);
            var op = new ConditionalElement((BaseExpressionElement)childValues[0], (BaseExpressionElement)childValues[1], (BaseExpressionElement)childValues[2]);
            node.AddValue(op);
            return node;
        }

        public override Node ExitInExpression(Production node)
        {
            IList childValues = GetChildValues(node);

            if (childValues.Count == 1)
            {
                AddFirstChildValue(node);
                return node;
            }

            var operand = (BaseExpressionElement)childValues[0];
            childValues.RemoveAt(0);

            var second = childValues[0];
            InElement op;

            if (second is IList list)
            {
                op = new InElement(operand, list);
            }
            else
            {
                var invocationListElement = new InvocationListElement(childValues, context);
                op = new InElement(operand, invocationListElement);
            }

            node.AddValue(op);
            return node;
        }

        public override Node ExitInTargetExpression(Production node)
        {
            AddFirstChildValue(node);
            return node;
        }

        public override Node ExitInListTargetExpression(Production node)
        {
            IList childValues = GetChildValues(node);
            node.AddValue(childValues);
            return node;
        }

        public override Node ExitCastExpression(Production node)
        {
            IList childValues = GetChildValues(node);
            var destTypeParts = (string[])childValues[1];
            var isArray = (bool)childValues[2];
            var op = new CastElement((BaseExpressionElement)childValues[0], destTypeParts, isArray, context);
            node.AddValue(op);
            return node;
        }

        public override Node ExitCastTypeExpression(Production node)
        {
            IList childValues = GetChildValues(node);
            var parts = new List<string>();

            foreach (string part in childValues)
            {
                parts.Add(part);
            }

            var isArray = false;

            if (parts[parts.Count - 1] == "[]")
            {
                isArray = true;
                parts.RemoveAt(parts.Count - 1);
            }

            node.AddValue(parts.ToArray());
            node.AddValue(isArray);
            return node;
        }

        public override Node ExitMemberFunctionExpression(Production node)
        {
            AddFirstChildValue(node);
            return node;
        }

        public override Node ExitFieldPropertyExpression(Production node)
        {
            //string name = ((Token)node.GetChildAt(0))?.Image;
            var name = node.GetChildAt(0).GetValue(0).ToString();
            var elem = new IdentifierElement(name);
            node.AddValue(elem);
            return node;
        }

        public override Node ExitFunctionCallExpression(Production node)
        {
            var childValues = GetChildValues(node);
            var name = (string)childValues[0];
            childValues.RemoveAt(0);
            var args = new ArgumentList(childValues);
            var funcCall = new FunctionCallElement(name, args);
            node.AddValue(funcCall);
            return node;
        }

        public override Node ExitArgumentList(Production node)
        {
            var childValues = GetChildValues(node);
            node.AddValues((ArrayList)childValues);
            return node;
        }

        public override Node ExitBasicExpression(Production node)
        {
            AddFirstChildValue(node);
            return node;
        }

        public override Node ExitLiteralExpression(Production node)
        {
            AddFirstChildValue(node);
            return node;
        }

        private void AddFirstChildValue(Production node)
        {
            node.AddValue(GetChildAt(node, 0).Values[0]);
        }

        private void AddUnaryOp(Production node, Type elementType)
        {
            IList childValues = GetChildValues(node);

            if (childValues.Count == 2)
            {
                var element = (UnaryElement)Activator.CreateInstance(elementType, childValues[1]);
                node.AddValue(element);
            }
            else
            {
                node.AddValue(childValues[0]);
            }
        }

        private void AddBinaryOp(Production node, Type elementType)
        {
            IList childValues = GetChildValues(node);

            if (childValues.Count > 1)
            {
                var expressionElement = BinaryExpressionElement.CreateElement(childValues, elementType);
                node.AddValue(expressionElement);
            }
            else if (childValues.Count == 1)
            {
                node.AddValue(childValues[0]);
            }
            else
            {
                Debug.Assert(false, "wrong number of children");
            }
        }

        public override Node ExitReal(PerCederberg.Grammatica.Runtime.Token node)
        {
            var element = RealLiteralElement.Create(node.Image, context.BuilderOptions);

            node.AddValue(element);
            return node;
        }

        public override Node ExitInteger(PerCederberg.Grammatica.Runtime.Token node)
        {
            var element = IntegralLiteralElement.Create(node.Image, false, inUnaryNegate, context.BuilderOptions);
            node.AddValue(element);
            return node;
        }

        public override Node ExitHexliteral(PerCederberg.Grammatica.Runtime.Token node)
        {
            var element = IntegralLiteralElement.Create(node.Image, true, inUnaryNegate, context.BuilderOptions);
            node.AddValue(element);
            return node;
        }

        public override Node ExitBooleanLiteralExpression(Production node)
        {
            AddFirstChildValue(node);
            return node;
        }

        public override Node ExitTrue(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(new BooleanLiteralElement(true));
            return node;
        }

        public override Node ExitFalse(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(new BooleanLiteralElement(false));
            return node;
        }

        public override Node ExitStringLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
            var s = DoEscapes(node.Image);
            var element = new StringLiteralElement(s);
            node.AddValue(element);
            return node;
        }

        public override Node ExitCharLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
            var s = DoEscapes(node.Image);
            node.AddValue(new CharLiteralElement(s[0]));
            return node;
        }

        public override Node ExitDatetime(PerCederberg.Grammatica.Runtime.Token node)
        {
            var image = node.Image.Substring(1, node.Image.Length - 2);
            var element = new DateTimeLiteralElement(image, context);
            node.AddValue(element);
            return node;
        }

        public override Node ExitTimeSpan(PerCederberg.Grammatica.Runtime.Token node)
        {
            var image = node.Image.Substring(2, node.Image.Length - 3);
            var element = new TimeSpanLiteralElement(image);
            node.AddValue(element);
            return node;
        }

        private string DoEscapes(string image)
        {
            // Remove outer quotes
            image = image.Substring(1, image.Length - 2);
            image = unicodeEscapeRegex.Replace(image, UnicodeEscapeMatcher);
            image = regularEscapeRegex.Replace(image, RegularEscapeMatcher);
            return image;
        }

        private static string? RegularEscapeMatcher(Match match)
        {
            var matchValue = match.Value;
            // Remove leading \
            matchValue = matchValue.Remove(0, 1);

            switch (matchValue)
            {
                case "\\":
                case "\"":
                case "'":
                    return matchValue;

                case "t":
                case "T":
                    return Convert.ToChar(9).ToString();

                case "n":
                case "N":
                    return Convert.ToChar(10).ToString();

                case "r":
                case "R":
                    return Convert.ToChar(13).ToString();

                default:
                    Debug.Assert(false, "Unrecognized escape sequence");
                    return null;
            }
        }

        private string UnicodeEscapeMatcher(Match m)
        {
            var value = m.Value;
            // Remove \u
            value = value.Remove(0, 2);
            var code = int.Parse(value, NumberStyles.AllowHexSpecifier);
            var c = Convert.ToChar(code);
            return c.ToString();
        }

        public override Node ExitIdentifier(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(node.Image);
            return node;
        }

        public override Node ExitNullLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(new NullLiteralElement());
            return node;
        }

        public override Node ExitArrayBraces(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue("[]");
            return node;
        }

        public override Node ExitAdd(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Add);
            return node;
        }

        public override Node ExitSub(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Subtract);
            return node;
        }

        public override Node ExitMul(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Multiply);
            return node;
        }

        public override Node ExitDiv(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Divide);
            return node;
        }

        public override Node ExitMod(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Mod);
            return node;
        }

        public override Node ExitPower(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Power);
            return node;
        }

        public override Node ExitEq(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(LogicalCompareOperation.Equal);
            return node;
        }

        public override Node ExitNe(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(LogicalCompareOperation.NotEqual);
            return node;
        }

        public override Node ExitLt(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(LogicalCompareOperation.LessThan);
            return node;
        }

        public override Node ExitGt(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(LogicalCompareOperation.GreaterThan);
            return node;
        }

        public override Node ExitLte(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(LogicalCompareOperation.LessThanOrEqual);
            return node;
        }

        public override Node ExitGte(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(LogicalCompareOperation.GreaterThanOrEqual);
            return node;
        }

        public override Node ExitAnd(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(AndOrOperation.And);
            return node;
        }

        public override Node ExitOr(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(AndOrOperation.Or);
            return node;
        }

        public override Node ExitXor(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue("Xor");
            return node;
        }

        public override Node ExitNot(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(string.Empty);
            return node;
        }

        public override Node ExitLeftShift(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(ShiftOperation.LeftShift);
            return node;
        }

        public override Node ExitRightShift(PerCederberg.Grammatica.Runtime.Token node)
        {
            node.AddValue(ShiftOperation.RightShift);
            return node;
        }

        public override void Child(Production node, Node child)
        {
            base.Child(node, child);
            inUnaryNegate = node.Id == (int)Token.NEGATE_EXPRESSION & child.Id == (int)Token.SUB;
        }
    }
}