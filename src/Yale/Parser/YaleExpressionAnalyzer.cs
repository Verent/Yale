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

namespace Yale.Parser;

/// <summary>
/// A class providing callback methods for the parser.
/// This extends ExpressionAnalyzer with Yale specific callback methods
/// </summary>
internal sealed class YaleExpressionAnalyzer : ExpressionAnalyzer
{
    private readonly Regex unicodeEscapeRegex =
        new("\\\\u[0-9a-f]{4}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly Regex regularEscapeRegex =
        new("\\\\[\\\\\"'trn]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private bool inUnaryNegate;
    private ExpressionContext? context;

    public void SetContext(ExpressionContext context) => this.context = context;

    public override void Reset() => context = null;

    public override Production ExitExpression(Production production)
    {
        AddFirstChildValue(production);
        return production;
    }

    public override Production ExitExpressionGroup(Production production)
    {
        production.Values.AddRange(GetChildValues(production));
        return production;
    }

    public override Production ExitXorExpression(Production production)
    {
        AddBinaryOp<XorElement>(production);
        return production;
    }

    public override Production ExitOrExpression(Production production)
    {
        AddBinaryOp<AndOrElement>(production);
        return production;
    }

    public override Production ExitAndExpression(Production production)
    {
        AddBinaryOp<AndOrElement>(production);
        return production;
    }

    public override Production ExitNotExpression(Production production)
    {
        AddUnaryOp(production, typeof(NotElement));
        return production;
    }

    public override Production ExitCompareExpression(Production production)
    {
        AddBinaryOp<CompareElement>(production);
        return production;
    }

    public override Production ExitShiftExpression(Production production)
    {
        AddBinaryOp<ShiftElement>(production);
        return production;
    }

    public override Production ExitAdditiveExpression(Production production)
    {
        AddBinaryOp<ArithmeticElement>(production);
        return production;
    }

    public override Production ExitMultiplicativeExpression(Production production)
    {
        AddBinaryOp<ArithmeticElement>(production);
        return production;
    }

    public override Production ExitPowerExpression(Production production)
    {
        AddBinaryOp<ArithmeticElement>(production);
        return production;
    }

    // Try to fold a negated constant int32.  We have to do this so that parsing int32.MinValue will work
    public override Production ExitNegateExpression(Production production)
    {
        var childValues = GetChildValues(production);

        var childElement = childValues[^1];

        // Is it an signed integer constant?
        if (
            ReferenceEquals(childElement.GetType(), typeof(Int32LiteralElement))
            & childValues.Count == 2
        )
        {
            ((Int32LiteralElement)childElement).Negate();
            // Add it directly instead of the negate element since it will already be negated
            production.Values.Add(childElement);
        }
        else if (
            ReferenceEquals(childElement.GetType(), typeof(Int64LiteralElement))
            & childValues.Count == 2
        )
        {
            ((Int64LiteralElement)childElement).Negate();
            // Add it directly instead of the negate element since it will already be negated
            production.Values.Add(childElement);
        }
        else
        {
            // No so just add a regular negate
            AddUnaryOp(production, typeof(NegateElement));
        }

        return production;
    }

    public override Production ExitMemberExpression(Production production)
    {
        var childValues = GetChildValues(production);
        var first = childValues[0];

        if (childValues.Count == 1 && first is not MemberElement)
        {
            production.Values.Add(first);
        }
        else
        {
            InvocationListElement invocationListElement = new(childValues, context);
            production.Values.Add(invocationListElement);
        }

        return production;
    }

    public override Production ExitIndexExpression(Production production)
    {
        var childValues = GetChildValues(production);
        ArgumentList args = new(childValues);
        IndexerElement e = new(args);
        production.Values.Add(e);
        return production;
    }

    public override Production ExitMemberAccessExpression(Production production)
    {
        var firstChild = production[1];
        var value = firstChild.GetValue(0);
        production.Values.Add(value);
        return production;
    }

    public override Production ExitSpecialFunctionExpression(Production production)
    {
        AddFirstChildValue(production);
        return production;
    }

    public override Production ExitIfExpression(Production production)
    {
        var childValues = GetChildValues(production);
        ConditionalElement op = new(childValues[0], childValues[1], childValues[2]);
        production.Values.Add(op);
        return production;
    }

    public override Production ExitInExpression(Production production)
    {
        var childValues = GetChildValues(production);

        if (childValues.Count == 1)
        {
            AddFirstChildValue(production);
            return production;
        }

        var operand = childValues[0];
        childValues.RemoveAt(0);

        var second = childValues[0];
        InElement op;

        if (second is IList list)
        {
            op = new(operand, list);
        }
        else
        {
            InvocationListElement invocationListElement = new(childValues, context);
            op = new(operand, invocationListElement);
        }

        production.Values.Add(op);
        return production;
    }

    public override Production ExitInTargetExpression(Production production)
    {
        AddFirstChildValue(production);
        return production;
    }

    public override Production ExitInListTargetExpression(Production production)
    {
        var childValues = GetChildValues(production);
        production.Values.AddRange(childValues);
        return production;
    }

    public override Production ExitCastExpression(Production production)
    {
        var childValues = GetChildValues(production);
        string[] destTypeParts = childValues[1];
        bool isArray = (bool)childValues[2];
        CastElement op = new(childValues[0], destTypeParts, isArray, context);
        production.Values.Add(op);
        return production;
    }

    public override Production ExitCastTypeExpression(Production production)
    {
        var childValues = GetChildValues(production);
        List<string> parts = new();

        foreach (var part in childValues)
        {
            parts.Add(part);
        }

        bool isArray = false;

        if (parts[^1] == "[]")
        {
            isArray = true;
            parts.RemoveAt(parts.Count - 1);
        }

        production.Values.Add(parts.ToArray());
        production.Values.Add(isArray);
        return production;
    }

    public override Production ExitMemberFunctionExpression(Production production)
    {
        AddFirstChildValue(production);
        return production;
    }

    public override Production ExitFieldPropertyExpression(Production production)
    {
        //string name = ((Token)node.GetChildAt(0))?.Image;
        var name = production[0].GetValue(0).ToString();
        IdentifierElement elem = new(name);
        production.Values.Add(elem);
        return production;
    }

    public override Production ExitFunctionCallExpression(Production production)
    {
        var childValues = GetChildValues(production);
        var name = (string)childValues[0];
        childValues.RemoveAt(0);
        ArgumentList args = new(childValues);
        FunctionCallElement funcCall = new(name, args);
        production.Values.Add(funcCall);
        return production;
    }

    public override Production ExitArgumentList(Production production)
    {
        var childValues = GetChildValues(production);
        production.Values.AddRange(childValues);
        return production;
    }

    public override Production ExitBasicExpression(Production production)
    {
        AddFirstChildValue(production);
        return production;
    }

    public override Production ExitLiteralExpression(Production production)
    {
        AddFirstChildValue(production);
        return production;
    }

    private static void AddFirstChildValue(Production production)
    {
        var value = GetChildAt(production, 0).Values[0];
        production.Values.Add(value);
    }

    private static void AddUnaryOp(Production production, Type elementType)
    {
        var childValues = GetChildValues(production);

        if (childValues.Count == 2)
        {
            var element = (UnaryElement)Activator.CreateInstance(elementType, childValues[1]);
            production.Values.Add(element);
        }
        else
        {
            production.Values.Add(childValues[0]);
        }
    }

    private static void AddBinaryOp<T>(Production production)
        where T : BinaryExpressionElement, new()
    {
        var childValues = GetChildValues(production);

        if (childValues.Count > 1)
        {
            var expressionElement = BinaryExpressionElement.CreateElement<T>(childValues);
            production.Values.Add(expressionElement);
        }
        else if (childValues.Count == 1)
        {
            production.Values.Add(childValues[0]);
        }
        else
        {
            //Todo: Imrove error handling
            Debug.Assert(false, "Wrong number of children");
        }
    }

    public override Token ExitReal(Token token)
    {
        var element = RealLiteralElement.Create(token.Image, context.BuilderOptions);

        token.AddValue(element);
        return token;
    }

    public override Token ExitInteger(Token token)
    {
        var element = IntegralLiteralElement.Create(
            token.Image,
            false,
            inUnaryNegate,
            context.BuilderOptions
        );
        token.AddValue(element);
        return token;
    }

    public override Token ExitHexliteral(Token token)
    {
        var element = IntegralLiteralElement.Create(
            token.Image,
            true,
            inUnaryNegate,
            context.BuilderOptions
        );
        token.AddValue(element);
        return token;
    }

    public override Token ExitBooleanLiteralExpression(Production production)
    {
        AddFirstChildValue(production);
        return production;
    }

    public override Token ExitTrue(Token token)
    {
        token.AddValue(new BooleanLiteralElement(true));
        return token;
    }

    public override Token ExitFalse(Token token)
    {
        token.AddValue(new BooleanLiteralElement(false));
        return token;
    }

    public override Token ExitStringLiteral(Token token)
    {
        var s = DoEscapes(token.Image);
        StringLiteralElement element = new(s);
        token.AddValue(element);
        return token;
    }

    public override Token ExitCharLiteral(Token token)
    {
        var s = DoEscapes(token.Image);
        token.AddValue(new CharLiteralElement(s[0]));
        return token;
    }

    public override Token ExitDatetime(Token token)
    {
        var image = token.Image[1..^1];
        DateTimeLiteralElement element = new(image, context);
        token.AddValue(element);
        return token;
    }

    public override Token ExitTimeSpan(Token token)
    {
        var image = token.Image[2..^1];
        TimeSpanLiteralElement element = new(image);
        token.AddValue(element);
        return token;
    }

    private string DoEscapes(string image)
    {
        //Todo: Add tests for this

        // Remove outer quotes
        image = image[1..^1];
        image = unicodeEscapeRegex.Replace(image, UnicodeEscapeMatcher);
        image = regularEscapeRegex.Replace(image, RegularEscapeMatcher);
        return image;
    }

    private static string RegularEscapeMatcher(Match match)
    {
        string matchValue = match.Value;
        // Remove leading \
        matchValue = matchValue.Remove(0, 1);

        return matchValue switch
        {
            "\\" or "\"" or "'" => matchValue,
            "t" or "T" => Convert.ToChar(9).ToString(),
            "n" or "N" => Convert.ToChar(10).ToString(),
            "r" or "R" => Convert.ToChar(13).ToString(),
            _ => throw new Exception("Unrecognized escape sequence"), //Todo: Throw proper yale exception
        };
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

    public override Token ExitIdentifier(Token token)
    {
        token.AddValue(token.Image);
        return token;
    }

    public override Token ExitNullLiteral(Token token)
    {
        token.AddValue(new NullLiteralElement());
        return token;
    }

    public override Token ExitArrayBraces(Token token)
    {
        token.AddValue("[]");
        return token;
    }

    public override Token ExitAdd(Token token)
    {
        token.AddValue(BinaryArithmeticOperation.Add);
        return token;
    }

    public override Token ExitSub(Token token)
    {
        token.AddValue(BinaryArithmeticOperation.Subtract);
        return token;
    }

    public override Token ExitMul(Token token)
    {
        token.AddValue(BinaryArithmeticOperation.Multiply);
        return token;
    }

    public override Token ExitDiv(Token token)
    {
        token.AddValue(BinaryArithmeticOperation.Divide);
        return token;
    }

    public override Token ExitMod(Token token)
    {
        token.AddValue(BinaryArithmeticOperation.Mod);
        return token;
    }

    public override Token ExitPower(Token token)
    {
        token.AddValue(BinaryArithmeticOperation.Power);
        return token;
    }

    public override Token ExitEq(Token token)
    {
        token.AddValue(LogicalCompareOperation.Equal);
        return token;
    }

    public override Token ExitNe(Token token)
    {
        token.AddValue(LogicalCompareOperation.NotEqual);
        return token;
    }

    public override Token ExitLt(Token token)
    {
        token.AddValue(LogicalCompareOperation.LessThan);
        return token;
    }

    public override Token ExitGt(Token token)
    {
        token.AddValue(LogicalCompareOperation.GreaterThan);
        return token;
    }

    public override Token ExitLte(Token token)
    {
        token.AddValue(LogicalCompareOperation.LessThanOrEqual);
        return token;
    }

    public override Token ExitGte(Token token)
    {
        token.AddValue(LogicalCompareOperation.GreaterThanOrEqual);
        return token;
    }

    public override Token ExitAnd(Token token)
    {
        token.AddValue(AndOrOperation.And);
        return token;
    }

    public override Token ExitOr(Token token)
    {
        token.AddValue(AndOrOperation.Or);
        return token;
    }

    public override Token ExitXor(Token token)
    {
        token.AddValue("Xor");
        return token;
    }

    public override Token ExitNot(Token token)
    {
        token.AddValue(string.Empty);
        return token;
    }

    public override Token ExitLeftShift(Token token)
    {
        token.AddValue(ShiftOperation.LeftShift);
        return token;
    }

    public override Token ExitRightShift(Token token)
    {
        token.AddValue(ShiftOperation.RightShift);
        return token;
    }

    public override void Child(Production node, Node child)
    {
        base.Child(node, child);
        inUnaryNegate = node.TypeId == TokenId.NEGATE_EXPRESSION & child.TypeId == TokenId.SUB;
    }
}
