using PerCederberg.Grammatica.Runtime;
using System.Diagnostics;
// ReSharper disable CyclomaticComplexity
// ReSharper disable MethodTooLong

namespace Yale.Parser
{
    /// <summary>
    /// A class providing callback methods for the parser.
    /// </summary>
    // ReSharper disable once ClassTooBig
    public class ExpressionAnalyzer : Analyzer
    {
        /// <summary>
        /// Called when entering a parse tree node.
        /// </summary>
        /// <param name="node"></param>
        // ReSharper disable once MethodTooLong
        public override void Enter(Node node)
        {
            Debug.WriteLine($"ExpressionAnalyzer enter: {node.Id}");

            switch ((Token)node.Id)
            {
                case Token.ADD:
                    EnterAdd((PerCederberg.Grammatica.Runtime.Token)node);
                    break;

                case Token.SUB:
                    EnterSub((PerCederberg.Grammatica.Runtime.Token)node);
                    break;

                case Token.MUL:
                    EnterMul((PerCederberg.Grammatica.Runtime.Token)node);
                    break;

                case Token.DIV:
                    EnterDiv((PerCederberg.Grammatica.Runtime.Token)node);
                    break;

                case Token.POWER:
                    EnterPower((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.MOD:
                    EnterMod((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.LEFT_PAREN:
                    EnterLeftParen((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.RIGHT_PAREN:
                    EnterRightParen((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.LEFT_BRACE:
                    EnterLeftBrace((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.RIGHT_BRACE:
                    EnterRightBrace((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.EQ:
                    EnterEq((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.LT:
                    EnterLt((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.GT:
                    EnterGt((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.LTE:
                    EnterLte((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.GTE:
                    EnterGte((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.NE:
                    EnterNe((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.AND:
                    EnterAnd((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.OR:
                    EnterOr((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.XOR:
                    EnterXor((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.NOT:
                    EnterNot((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.IN:
                    EnterIn((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.DOT:
                    EnterDot((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.ARGUMENT_SEPARATOR:
                    EnterArgumentSeparator((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.ARRAY_BRACES:
                    EnterArrayBraces((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.LEFT_SHIFT:
                    EnterLeftShift((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.RIGHT_SHIFT:
                    EnterRightShift((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.INTEGER:
                    EnterInteger((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.REAL:
                    EnterReal((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.STRING_LITERAL:
                    EnterStringLiteral((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.CHAR_LITERAL:
                    EnterCharLiteral((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.TRUE:
                    EnterTrue((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.FALSE:
                    EnterFalse((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.IDENTIFIER:
                    EnterIdentifier((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.HEX_LITERAL:
                    EnterHexLiteral((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.NULL_LITERAL:
                    EnterNullLiteral((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.TIMESPAN:
                    EnterTimespan((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.DATETIME:
                    EnterDatetime((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.IF:
                    EnterIf((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.CAST:
                    EnterCast((PerCederberg.Grammatica.Runtime.Token)node);

                    break;

                case Token.EXPRESSION:
                    EnterExpression((Production)node);

                    break;

                case Token.XOR_EXPRESSION:
                    EnterXorExpression((Production)node);

                    break;

                case Token.OR_EXPRESSION:
                    EnterOrExpression((Production)node);

                    break;

                case Token.AND_EXPRESSION:
                    EnterAndExpression((Production)node);

                    break;

                case Token.NOT_EXPRESSION:
                    EnterNotExpression((Production)node);

                    break;

                case Token.IN_EXPRESSION:
                    EnterInExpression((Production)node);

                    break;

                case Token.IN_TARGET_EXPRESSION:
                    EnterInTargetExpression((Production)node);

                    break;

                case Token.IN_LIST_TARGET_EXPRESSION:
                    EnterInListTargetExpression((Production)node);

                    break;

                case Token.COMPARE_EXPRESSION:
                    EnterCompareExpression((Production)node);

                    break;

                case Token.SHIFT_EXPRESSION:
                    EnterShiftExpression((Production)node);

                    break;

                case Token.ADDITIVE_EXPRESSION:
                    EnterAdditiveExpression((Production)node);

                    break;

                case Token.MULTIPLICATIVE_EXPRESSION:
                    EnterMultiplicativeExpression((Production)node);

                    break;

                case Token.POWER_EXPRESSION:
                    EnterPowerExpression((Production)node);

                    break;

                case Token.NEGATE_EXPRESSION:
                    EnterNegateExpression((Production)node);

                    break;

                case Token.MEMBER_EXPRESSION:
                    EnterMemberExpression((Production)node);

                    break;

                case Token.MEMBER_ACCESS_EXPRESSION:
                    EnterMemberAccessExpression((Production)node);

                    break;

                case Token.BASIC_EXPRESSION:
                    EnterBasicExpression((Production)node);

                    break;

                case Token.MEMBER_FUNCTION_EXPRESSION:
                    EnterMemberFunctionExpression((Production)node);

                    break;

                case Token.FIELD_PROPERTY_EXPRESSION:
                    EnterFieldPropertyExpression((Production)node);

                    break;

                case Token.SPECIAL_FUNCTION_EXPRESSION:
                    EnterSpecialFunctionExpression((Production)node);

                    break;

                case Token.IF_EXPRESSION:
                    EnterIfExpression((Production)node);

                    break;

                case Token.CAST_EXPRESSION:
                    EnterCastExpression((Production)node);

                    break;

                case Token.CAST_TYPE_EXPRESSION:
                    EnterCastTypeExpression((Production)node);

                    break;

                case Token.INDEX_EXPRESSION:
                    EnterIndexExpression((Production)node);

                    break;

                case Token.FUNCTION_CALL_EXPRESSION:
                    EnterFunctionCallExpression((Production)node);

                    break;

                case Token.ARGUMENT_LIST:
                    EnterArgumentList((Production)node);

                    break;

                case Token.LITERAL_EXPRESSION:
                    EnterLiteralExpression((Production)node);

                    break;

                case Token.BOOLEAN_LITERAL_EXPRESSION:
                    EnterBooleanLiteralExpression((Production)node);

                    break;

                case Token.EXPRESSION_GROUP:
                    EnterExpressionGroup((Production)node);

                    break;
            }
        }

        /// <summary>
        /// Called when exiting a parse tree node.
        /// The node being exited the node to add to the parse tree, or null if no parse tree should be created
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override Node Exit(Node node)
        {
            switch ((Token)node.Id)
            {
                case Token.ADD:

                    return ExitAdd((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.SUB:

                    return ExitSub((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.MUL:

                    return ExitMul((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.DIV:

                    return ExitDiv((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.POWER:

                    return ExitPower((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.MOD:

                    return ExitMod((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.LEFT_PAREN:

                    return ExitLeftParen((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.RIGHT_PAREN:

                    return ExitRightParen((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.LEFT_BRACE:

                    return ExitLeftBrace((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.RIGHT_BRACE:

                    return ExitRightBrace((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.EQ:

                    return ExitEq((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.LT:

                    return ExitLt((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.GT:

                    return ExitGt((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.LTE:

                    return ExitLte((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.GTE:

                    return ExitGte((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.NE:

                    return ExitNe((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.AND:

                    return ExitAnd((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.OR:

                    return ExitOr((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.XOR:

                    return ExitXor((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.NOT:

                    return ExitNot((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.IN:

                    return ExitIn((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.DOT:

                    return ExitDot((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.ARGUMENT_SEPARATOR:

                    return ExitArgumentSeparator((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.ARRAY_BRACES:

                    return ExitArrayBraces((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.LEFT_SHIFT:

                    return ExitLeftShift((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.RIGHT_SHIFT:

                    return ExitRightShift((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.INTEGER:

                    return ExitInteger((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.REAL:

                    return ExitReal((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.STRING_LITERAL:

                    return ExitStringLiteral((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.CHAR_LITERAL:

                    return ExitCharLiteral((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.TRUE:

                    return ExitTrue((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.FALSE:

                    return ExitFalse((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.IDENTIFIER:

                    return ExitIdentifier((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.HEX_LITERAL:

                    return ExitHexliteral((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.NULL_LITERAL:

                    return ExitNullLiteral((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.TIMESPAN:

                    return ExitTimeSpan((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.DATETIME:

                    return ExitDatetime((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.IF:

                    return ExitIf((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.CAST:

                    return ExitCast((PerCederberg.Grammatica.Runtime.Token)node);

                case Token.EXPRESSION:

                    return ExitExpression((Production)node);

                case Token.XOR_EXPRESSION:

                    return ExitXorExpression((Production)node);

                case Token.OR_EXPRESSION:

                    return ExitOrExpression((Production)node);

                case Token.AND_EXPRESSION:

                    return ExitAndExpression((Production)node);

                case Token.NOT_EXPRESSION:

                    return ExitNotExpression((Production)node);

                case Token.IN_EXPRESSION:

                    return ExitInExpression((Production)node);

                case Token.IN_TARGET_EXPRESSION:

                    return ExitInTargetExpression((Production)node);

                case Token.IN_LIST_TARGET_EXPRESSION:

                    return ExitInListTargetExpression((Production)node);

                case Token.COMPARE_EXPRESSION:

                    return ExitCompareExpression((Production)node);

                case Token.SHIFT_EXPRESSION:

                    return ExitShiftExpression((Production)node);

                case Token.ADDITIVE_EXPRESSION:

                    return ExitAdditiveExpression((Production)node);

                case Token.MULTIPLICATIVE_EXPRESSION:

                    return ExitMultiplicativeExpression((Production)node);

                case Token.POWER_EXPRESSION:

                    return ExitPowerExpression((Production)node);

                case Token.NEGATE_EXPRESSION:

                    return ExitNegateExpression((Production)node);

                case Token.MEMBER_EXPRESSION:

                    return ExitMemberExpression((Production)node);

                case Token.MEMBER_ACCESS_EXPRESSION:

                    return ExitMemberAccessExpression((Production)node);

                case Token.BASIC_EXPRESSION:

                    return ExitBasicExpression((Production)node);

                case Token.MEMBER_FUNCTION_EXPRESSION:

                    return ExitMemberFunctionExpression((Production)node);

                case Token.FIELD_PROPERTY_EXPRESSION:

                    return ExitFieldPropertyExpression((Production)node);

                case Token.SPECIAL_FUNCTION_EXPRESSION:

                    return ExitSpecialFunctionExpression((Production)node);

                case Token.IF_EXPRESSION:

                    return ExitIfExpression((Production)node);

                case Token.CAST_EXPRESSION:

                    return ExitCastExpression((Production)node);

                case Token.CAST_TYPE_EXPRESSION:

                    return ExitCastTypeExpression((Production)node);

                case Token.INDEX_EXPRESSION:

                    return ExitIndexExpression((Production)node);

                case Token.FUNCTION_CALL_EXPRESSION:

                    return ExitFunctionCallExpression((Production)node);

                case Token.ARGUMENT_LIST:

                    return ExitArgumentList((Production)node);

                case Token.LITERAL_EXPRESSION:

                    return ExitLiteralExpression((Production)node);

                case Token.BOOLEAN_LITERAL_EXPRESSION:

                    return ExitBooleanLiteralExpression((Production)node);

                case Token.EXPRESSION_GROUP:

                    return ExitExpressionGroup((Production)node);
            }
            return node;
        }

        /// <summary>
        /// Called when adding a child to a parse tree node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="child"></param>
        public override void Child(Production node, Node child)
        {
            switch ((Token)node.Id)
            {
                case Token.EXPRESSION:
                    ChildExpression(node, child);

                    break;

                case Token.XOR_EXPRESSION:
                    ChildXorExpression(node, child);

                    break;

                case Token.OR_EXPRESSION:
                    ChildOrExpression(node, child);

                    break;

                case Token.AND_EXPRESSION:
                    ChildAndExpression(node, child);

                    break;

                case Token.NOT_EXPRESSION:
                    ChildNotExpression(node, child);

                    break;

                case Token.IN_EXPRESSION:
                    ChildInExpression(node, child);

                    break;

                case Token.IN_TARGET_EXPRESSION:
                    ChildInTargetExpression(node, child);

                    break;

                case Token.IN_LIST_TARGET_EXPRESSION:
                    ChildInListTargetExpression(node, child);

                    break;

                case Token.COMPARE_EXPRESSION:
                    ChildCompareExpression(node, child);

                    break;

                case Token.SHIFT_EXPRESSION:
                    ChildShiftExpression(node, child);

                    break;

                case Token.ADDITIVE_EXPRESSION:
                    ChildAdditiveExpression(node, child);

                    break;

                case Token.MULTIPLICATIVE_EXPRESSION:
                    ChildMultiplicativeExpression(node, child);

                    break;

                case Token.POWER_EXPRESSION:
                    ChildPowerExpression(node, child);

                    break;

                case Token.NEGATE_EXPRESSION:
                    ChildNegateExpression(node, child);

                    break;

                case Token.MEMBER_EXPRESSION:
                    ChildMemberExpression(node, child);

                    break;

                case Token.MEMBER_ACCESS_EXPRESSION:
                    ChildMemberAccessExpression(node, child);

                    break;

                case Token.BASIC_EXPRESSION:
                    ChildBasicExpression(node, child);

                    break;

                case Token.MEMBER_FUNCTION_EXPRESSION:
                    ChildMemberFunctionExpression(node, child);

                    break;

                case Token.FIELD_PROPERTY_EXPRESSION:
                    ChildFieldPropertyExpression(node, child);

                    break;

                case Token.SPECIAL_FUNCTION_EXPRESSION:
                    ChildSpecialFunctionExpression(node, child);

                    break;

                case Token.IF_EXPRESSION:
                    ChildIfExpression(node, child);

                    break;

                case Token.CAST_EXPRESSION:
                    ChildCastExpression(node, child);

                    break;

                case Token.CAST_TYPE_EXPRESSION:
                    ChildCastTypeExpression(node, child);

                    break;

                case Token.INDEX_EXPRESSION:
                    ChildIndexExpression(node, child);

                    break;

                case Token.FUNCTION_CALL_EXPRESSION:
                    ChildFunctionCallExpression(node, child);

                    break;

                case Token.ARGUMENT_LIST:
                    ChildArgumentList(node, child);

                    break;

                case Token.LITERAL_EXPRESSION:
                    ChildLiteralExpression(node, child);

                    break;

                case Token.BOOLEAN_LITERAL_EXPRESSION:
                    ChildBooleanLiteralExpression(node, child);

                    break;

                case Token.EXPRESSION_GROUP:
                    ChildExpressionGroup(node, child);

                    break;
            }
        }

        public virtual void EnterAdd(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitAdd(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterSub(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitSub(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterMul(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitMul(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterDiv(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitDiv(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterPower(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitPower(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterMod(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitMod(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterLeftParen(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitLeftParen(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterRightParen(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitRightParen(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterLeftBrace(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitLeftBrace(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterRightBrace(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitRightBrace(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterEq(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitEq(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterLt(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitLt(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterGt(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitGt(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterLte(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitLte(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterGte(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitGte(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterNe(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitNe(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterAnd(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitAnd(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterOr(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitOr(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterXor(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitXor(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterNot(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitNot(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterIn(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitIn(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterDot(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitDot(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterArgumentSeparator(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitArgumentSeparator(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterArrayBraces(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitArrayBraces(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterLeftShift(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitLeftShift(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterRightShift(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitRightShift(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterInteger(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitInteger(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterReal(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitReal(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterStringLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitStringLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterCharLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitCharLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterTrue(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitTrue(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterFalse(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitFalse(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterIdentifier(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitIdentifier(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterHexLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitHexliteral(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterNullLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitNullLiteral(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterTimespan(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitTimeSpan(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterDatetime(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitDatetime(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterIf(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitIf(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterCast(PerCederberg.Grammatica.Runtime.Token node)
        {
        }

        public virtual Node ExitCast(PerCederberg.Grammatica.Runtime.Token node)
        {
            return node;
        }

        public virtual void EnterExpression(Production node)
        {
        }

        public virtual Node ExitExpression(Production node)
        {
            return node;
        }

        public virtual void ChildExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterXorExpression(Production node)
        {
        }

        public virtual Node ExitXorExpression(Production node)
        {
            return node;
        }

        public virtual void ChildXorExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterOrExpression(Production node)
        {
        }

        public virtual Node ExitOrExpression(Production node)
        {
            return node;
        }

        public virtual void ChildOrExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterAndExpression(Production node)
        {
        }

        public virtual Node ExitAndExpression(Production node)
        {
            return node;
        }

        public virtual void ChildAndExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterNotExpression(Production node)
        {
        }

        public virtual Node ExitNotExpression(Production node)
        {
            return node;
        }

        public virtual void ChildNotExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterInExpression(Production node)
        {
        }

        public virtual Node ExitInExpression(Production node)
        {
            return node;
        }

        public virtual void ChildInExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterInTargetExpression(Production node)
        {
        }

        public virtual Node ExitInTargetExpression(Production node)
        {
            return node;
        }

        public virtual void ChildInTargetExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterInListTargetExpression(Production node)
        {
        }

        public virtual Node ExitInListTargetExpression(Production node)
        {
            return node;
        }

        public virtual void ChildInListTargetExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterCompareExpression(Production node)
        {
        }

        public virtual Node ExitCompareExpression(Production node)
        {
            return node;
        }

        public virtual void ChildCompareExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterShiftExpression(Production node)
        {
        }

        public virtual Node ExitShiftExpression(Production node)
        {
            return node;
        }

        public virtual void ChildShiftExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterAdditiveExpression(Production node)
        {
        }

        public virtual Node ExitAdditiveExpression(Production node)
        {
            return node;
        }

        public virtual void ChildAdditiveExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterMultiplicativeExpression(Production node)
        {
        }

        public virtual Node ExitMultiplicativeExpression(Production node)
        {
            return node;
        }

        public virtual void ChildMultiplicativeExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterPowerExpression(Production node)
        {
        }

        public virtual Node ExitPowerExpression(Production node)
        {
            return node;
        }

        public virtual void ChildPowerExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterNegateExpression(Production node)
        {
        }

        public virtual Node ExitNegateExpression(Production node)
        {
            return node;
        }

        public virtual void ChildNegateExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterMemberExpression(Production node)
        {
        }

        public virtual Node ExitMemberExpression(Production node)
        {
            return node;
        }

        public virtual void ChildMemberExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterMemberAccessExpression(Production node)
        {
        }

        public virtual Node ExitMemberAccessExpression(Production node)
        {
            return node;
        }

        public virtual void ChildMemberAccessExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterBasicExpression(Production node)
        {
        }

        public virtual Node ExitBasicExpression(Production node)
        {
            return node;
        }

        public virtual void ChildBasicExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterMemberFunctionExpression(Production node)
        {
        }

        public virtual Node ExitMemberFunctionExpression(Production node)
        {
            return node;
        }

        public virtual void ChildMemberFunctionExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterFieldPropertyExpression(Production node)
        {
        }

        public virtual Node ExitFieldPropertyExpression(Production node)
        {
            return node;
        }

        public virtual void ChildFieldPropertyExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterSpecialFunctionExpression(Production node)
        {
        }

        public virtual Node ExitSpecialFunctionExpression(Production node)
        {
            return node;
        }

        public virtual void ChildSpecialFunctionExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterIfExpression(Production node)
        {
        }

        public virtual Node ExitIfExpression(Production node)
        {
            return node;
        }

        public virtual void ChildIfExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterCastExpression(Production node)
        {
        }

        public virtual Node ExitCastExpression(Production node)
        {
            return node;
        }

        public virtual void ChildCastExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterCastTypeExpression(Production node)
        {
        }

        public virtual Node ExitCastTypeExpression(Production node)
        {
            return node;
        }

        public virtual void ChildCastTypeExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterIndexExpression(Production node)
        {
        }

        public virtual Node ExitIndexExpression(Production node)
        {
            return node;
        }

        public virtual void ChildIndexExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterFunctionCallExpression(Production node)
        {
        }

        public virtual Node ExitFunctionCallExpression(Production node)
        {
            return node;
        }

        public virtual void ChildFunctionCallExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterArgumentList(Production node)
        {
        }

        public virtual Node ExitArgumentList(Production node)
        {
            return node;
        }

        public virtual void ChildArgumentList(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterLiteralExpression(Production node)
        {
        }

        public virtual Node ExitLiteralExpression(Production node)
        {
            return node;
        }

        public virtual void ChildLiteralExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterBooleanLiteralExpression(Production node)
        {
        }

        public virtual Node ExitBooleanLiteralExpression(Production node)
        {
            return node;
        }

        public virtual void ChildBooleanLiteralExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterExpressionGroup(Production node)
        {
        }

        public virtual Node ExitExpressionGroup(Production node)
        {
            return node;
        }

        public virtual void ChildExpressionGroup(Production node, Node child)
        {
            node.AddChild(child);
        }
    }
}