using System.Xml.Linq;

namespace Yale.Parser;

/// <summary>
/// Callback methods for the parser.
/// </summary>
internal class ExpressionAnalyzer : Analyzer
{
    public override void Enter(Production production)
    {
        switch (production.TypeId)
        {
            case TokenId.EXPRESSION:
                EnterExpression(production);
                break;

            case TokenId.XOR_EXPRESSION:
                EnterXorExpression(production);

                break;

            case TokenId.OR_EXPRESSION:
                EnterOrExpression(production);

                break;

            case TokenId.AND_EXPRESSION:
                EnterAndExpression(production);

                break;

            case TokenId.NOT_EXPRESSION:
                EnterNotExpression(production);

                break;

            case TokenId.IN_EXPRESSION:
                EnterInExpression(production);

                break;

            case TokenId.IN_TARGET_EXPRESSION:
                EnterInTargetExpression(production);

                break;

            case TokenId.IN_LIST_TARGET_EXPRESSION:
                EnterInListTargetExpression(production);

                break;

            case TokenId.COMPARE_EXPRESSION:
                EnterCompareExpression(production);

                break;

            case TokenId.SHIFT_EXPRESSION:
                EnterShiftExpression(production);

                break;

            case TokenId.ADDITIVE_EXPRESSION:
                EnterAdditiveExpression(production);

                break;

            case TokenId.MULTIPLICATIVE_EXPRESSION:
                EnterMultiplicativeExpression(production);

                break;

            case TokenId.POWER_EXPRESSION:
                EnterPowerExpression(production);

                break;

            case TokenId.NEGATE_EXPRESSION:
                EnterNegateExpression(production);

                break;

            case TokenId.MEMBER_EXPRESSION:
                EnterMemberExpression(production);

                break;

            case TokenId.MEMBER_ACCESS_EXPRESSION:
                EnterMemberAccessExpression(production);

                break;

            case TokenId.BASIC_EXPRESSION:
                EnterBasicExpression(production);

                break;

            case TokenId.MEMBER_FUNCTION_EXPRESSION:
                EnterMemberFunctionExpression(production);

                break;

            case TokenId.FIELD_PROPERTY_EXPRESSION:
                EnterFieldPropertyExpression(production);

                break;

            case TokenId.SPECIAL_FUNCTION_EXPRESSION:
                EnterSpecialFunctionExpression(production);

                break;

            case TokenId.IF_EXPRESSION:
                EnterIfExpression(production);

                break;

            case TokenId.CAST_EXPRESSION:
                EnterCastExpression(production);

                break;

            case TokenId.CAST_TYPE_EXPRESSION:
                EnterCastTypeExpression(production);

                break;

            case TokenId.INDEX_EXPRESSION:
                EnterIndexExpression(production);

                break;

            case TokenId.FUNCTION_CALL_EXPRESSION:
                EnterFunctionCallExpression(production);

                break;

            case TokenId.ARGUMENT_LIST:
                EnterArgumentList(production);

                break;

            case TokenId.LITERAL_EXPRESSION:
                EnterLiteralExpression(production);

                break;

            case TokenId.BOOLEAN_LITERAL_EXPRESSION:
                EnterBooleanLiteralExpression(production);

                break;

            case TokenId.EXPRESSION_GROUP:
                EnterExpressionGroup(production);

                break;
        }
    }

    /// <summary>
    /// Called when entering a parse tree node.
    /// </summary>
    /// <param name="token"></param>
    public override void Enter(Token token)
    {
        switch (token.TypeId)
        {
            case TokenId.ADD:
                EnterAdd(token);
                break;

            case TokenId.SUB:
                EnterSub(token);
                break;

            case TokenId.MUL:
                EnterMul(token);
                break;

            case TokenId.DIV:
                EnterDiv(token);
                break;

            case TokenId.POWER:
                EnterPower(token);

                break;

            case TokenId.MOD:
                EnterMod(token);

                break;

            case TokenId.LEFT_PAREN:
                EnterLeftParen(token);

                break;

            case TokenId.RIGHT_PAREN:
                EnterRightParen(token);

                break;

            case TokenId.LEFT_BRACE:
                EnterLeftBrace(token);

                break;

            case TokenId.RIGHT_BRACE:
                EnterRightBrace(token);

                break;

            case TokenId.EQ:
                EnterEq(token);

                break;

            case TokenId.LT:
                EnterLt(token);

                break;

            case TokenId.GT:
                EnterGt(token);

                break;

            case TokenId.LTE:
                EnterLte(token);

                break;

            case TokenId.GTE:
                EnterGte(token);

                break;

            case TokenId.NE:
                EnterNe(token);

                break;

            case TokenId.AND:
                EnterAnd(token);

                break;

            case TokenId.OR:
                EnterOr(token);

                break;

            case TokenId.XOR:
                EnterXor(token);

                break;

            case TokenId.NOT:
                EnterNot(token);

                break;

            case TokenId.IN:
                EnterIn(token);

                break;

            case TokenId.DOT:
                EnterDot(token);

                break;

            case TokenId.ARGUMENT_SEPARATOR:
                EnterArgumentSeparator(token);

                break;

            case TokenId.ARRAY_BRACES:
                EnterArrayBraces(token);

                break;

            case TokenId.LEFT_SHIFT:
                EnterLeftShift(token);

                break;

            case TokenId.RIGHT_SHIFT:
                EnterRightShift(token);

                break;

            case TokenId.INTEGER:
                EnterInteger(token);

                break;

            case TokenId.REAL:
                EnterReal(token);

                break;

            case TokenId.STRING_LITERAL:
                EnterStringLiteral(token);

                break;

            case TokenId.CHAR_LITERAL:
                EnterCharLiteral(token);

                break;

            case TokenId.TRUE:
                EnterTrue(token);

                break;

            case TokenId.FALSE:
                EnterFalse(token);

                break;

            case TokenId.IDENTIFIER:
                EnterIdentifier(token);

                break;

            case TokenId.HEX_LITERAL:
                EnterHexLiteral(token);

                break;

            case TokenId.NULL_LITERAL:
                EnterNullLiteral(token);

                break;

            case TokenId.TIMESPAN:
                EnterTimespan(token);

                break;

            case TokenId.DATETIME:
                EnterDatetime(token);

                break;

            case TokenId.IF:
                EnterIf(token);

                break;

            case TokenId.CAST:
                EnterCast(token);

                break;
        }
    }

    /// <summary>
    /// Called when exiting a parse tree node.
    /// The node being exited the node to add to the parse tree, or null if no parse tree should be created
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public override Token Exit(Token token)
    {
        switch (token.TypeId)
        {
            case TokenId.ADD:

                return ExitAdd(token);

            case TokenId.SUB:

                return ExitSub(token);

            case TokenId.MUL:

                return ExitMul(token);

            case TokenId.DIV:

                return ExitDiv(token);

            case TokenId.POWER:

                return ExitPower(token);

            case TokenId.MOD:

                return ExitMod(token);

            case TokenId.LEFT_PAREN:

                return ExitLeftParen(token);

            case TokenId.RIGHT_PAREN:

                return ExitRightParen(token);

            case TokenId.LEFT_BRACE:

                return ExitLeftBrace(token);

            case TokenId.RIGHT_BRACE:

                return ExitRightBrace(token);

            case TokenId.EQ:

                return ExitEq(token);

            case TokenId.LT:

                return ExitLt(token);

            case TokenId.GT:

                return ExitGt(token);

            case TokenId.LTE:

                return ExitLte(token);

            case TokenId.GTE:

                return ExitGte(token);

            case TokenId.NE:

                return ExitNe(token);

            case TokenId.AND:

                return ExitAnd(token);

            case TokenId.OR:

                return ExitOr(token);

            case TokenId.XOR:

                return ExitXor(token);

            case TokenId.NOT:

                return ExitNot(token);

            case TokenId.IN:

                return ExitIn(token);

            case TokenId.DOT:

                return ExitDot(token);

            case TokenId.ARGUMENT_SEPARATOR:

                return ExitArgumentSeparator(token);

            case TokenId.ARRAY_BRACES:

                return ExitArrayBraces(token);

            case TokenId.LEFT_SHIFT:

                return ExitLeftShift(token);

            case TokenId.RIGHT_SHIFT:

                return ExitRightShift(token);

            case TokenId.INTEGER:

                return ExitInteger(token);

            case TokenId.REAL:

                return ExitReal(token);

            case TokenId.STRING_LITERAL:

                return ExitStringLiteral(token);

            case TokenId.CHAR_LITERAL:

                return ExitCharLiteral(token);

            case TokenId.TRUE:

                return ExitTrue(token);

            case TokenId.FALSE:

                return ExitFalse(token);

            case TokenId.IDENTIFIER:

                return ExitIdentifier(token);

            case TokenId.HEX_LITERAL:

                return ExitHexliteral(token);

            case TokenId.NULL_LITERAL:
                return ExitNullLiteral(token);

            case TokenId.TIMESPAN:
                return ExitTimeSpan(token);

            case TokenId.DATETIME:
                return ExitDatetime(token);

            case TokenId.IF:
                return ExitIf(token);

            case TokenId.CAST:
                return ExitCast(token);
        }
        return token;
    }

    public override Production Exit(Production production)
    {
        switch (production.TypeId)
        {
            case TokenId.EXPRESSION:

                return ExitExpression(production);

            case TokenId.XOR_EXPRESSION:

                return ExitXorExpression(production);

            case TokenId.OR_EXPRESSION:

                return ExitOrExpression(production);

            case TokenId.AND_EXPRESSION:

                return ExitAndExpression(production);

            case TokenId.NOT_EXPRESSION:

                return ExitNotExpression(production);

            case TokenId.IN_EXPRESSION:

                return ExitInExpression(production);

            case TokenId.IN_TARGET_EXPRESSION:

                return ExitInTargetExpression(production);

            case TokenId.IN_LIST_TARGET_EXPRESSION:

                return ExitInListTargetExpression(production);

            case TokenId.COMPARE_EXPRESSION:

                return ExitCompareExpression(production);

            case TokenId.SHIFT_EXPRESSION:

                return ExitShiftExpression(production);

            case TokenId.ADDITIVE_EXPRESSION:

                return ExitAdditiveExpression(production);

            case TokenId.MULTIPLICATIVE_EXPRESSION:

                return ExitMultiplicativeExpression(production);

            case TokenId.POWER_EXPRESSION:

                return ExitPowerExpression(production);

            case TokenId.NEGATE_EXPRESSION:

                return ExitNegateExpression(production);

            case TokenId.MEMBER_EXPRESSION:

                return ExitMemberExpression(production);

            case TokenId.MEMBER_ACCESS_EXPRESSION:

                return ExitMemberAccessExpression(production);

            case TokenId.BASIC_EXPRESSION:

                return ExitBasicExpression(production);

            case TokenId.MEMBER_FUNCTION_EXPRESSION:

                return ExitMemberFunctionExpression(production);

            case TokenId.FIELD_PROPERTY_EXPRESSION:

                return ExitFieldPropertyExpression(production);

            case TokenId.SPECIAL_FUNCTION_EXPRESSION:

                return ExitSpecialFunctionExpression(production);

            case TokenId.IF_EXPRESSION:

                return ExitIfExpression(production);

            case TokenId.CAST_EXPRESSION:

                return ExitCastExpression(production);

            case TokenId.CAST_TYPE_EXPRESSION:

                return ExitCastTypeExpression(production);

            case TokenId.INDEX_EXPRESSION:

                return ExitIndexExpression(production);

            case TokenId.FUNCTION_CALL_EXPRESSION:

                return ExitFunctionCallExpression(production);

            case TokenId.ARGUMENT_LIST:

                return ExitArgumentList(production);

            case TokenId.LITERAL_EXPRESSION:

                return ExitLiteralExpression(production);

            case TokenId.BOOLEAN_LITERAL_EXPRESSION:

                return ExitBooleanLiteralExpression(production);

            case TokenId.EXPRESSION_GROUP:

                return ExitExpressionGroup(production);
        }
        return production;
    }

    /// <summary>
    /// Called when adding a child to a parse tree node.
    /// </summary>
    /// <param name="production"></param>
    /// <param name="child"></param>
    public override void Child(Production production, Node child)
    {
        switch ((TokenId)production.TypeId)
        {
            case TokenId.EXPRESSION:
                ChildExpression(production, child);

                break;

            case TokenId.XOR_EXPRESSION:
                ChildXorExpression(production, child);

                break;

            case TokenId.OR_EXPRESSION:
                ChildOrExpression(production, child);

                break;

            case TokenId.AND_EXPRESSION:
                ChildAndExpression(production, child);

                break;

            case TokenId.NOT_EXPRESSION:
                ChildNotExpression(production, child);

                break;

            case TokenId.IN_EXPRESSION:
                ChildInExpression(production, child);

                break;

            case TokenId.IN_TARGET_EXPRESSION:
                ChildInTargetExpression(production, child);

                break;

            case TokenId.IN_LIST_TARGET_EXPRESSION:
                ChildInListTargetExpression(production, child);

                break;

            case TokenId.COMPARE_EXPRESSION:
                ChildCompareExpression(production, child);

                break;

            case TokenId.SHIFT_EXPRESSION:
                ChildShiftExpression(production, child);

                break;

            case TokenId.ADDITIVE_EXPRESSION:
                ChildAdditiveExpression(production, child);

                break;

            case TokenId.MULTIPLICATIVE_EXPRESSION:
                ChildMultiplicativeExpression(production, child);

                break;

            case TokenId.POWER_EXPRESSION:
                ChildPowerExpression(production, child);

                break;

            case TokenId.NEGATE_EXPRESSION:
                ChildNegateExpression(production, child);

                break;

            case TokenId.MEMBER_EXPRESSION:
                ChildMemberExpression(production, child);

                break;

            case TokenId.MEMBER_ACCESS_EXPRESSION:
                ChildMemberAccessExpression(production, child);

                break;

            case TokenId.BASIC_EXPRESSION:
                ChildBasicExpression(production, child);

                break;

            case TokenId.MEMBER_FUNCTION_EXPRESSION:
                ChildMemberFunctionExpression(production, child);

                break;

            case TokenId.FIELD_PROPERTY_EXPRESSION:
                ChildFieldPropertyExpression(production, child);

                break;

            case TokenId.SPECIAL_FUNCTION_EXPRESSION:
                ChildSpecialFunctionExpression(production, child);

                break;

            case TokenId.IF_EXPRESSION:
                ChildIfExpression(production, child);

                break;

            case TokenId.CAST_EXPRESSION:
                ChildCastExpression(production, child);

                break;

            case TokenId.CAST_TYPE_EXPRESSION:
                ChildCastTypeExpression(production, child);

                break;

            case TokenId.INDEX_EXPRESSION:
                ChildIndexExpression(production, child);

                break;

            case TokenId.FUNCTION_CALL_EXPRESSION:
                ChildFunctionCallExpression(production, child);

                break;

            case TokenId.ARGUMENT_LIST:
                ChildArgumentList(production, child);

                break;

            case TokenId.LITERAL_EXPRESSION:
                ChildLiteralExpression(production, child);

                break;

            case TokenId.BOOLEAN_LITERAL_EXPRESSION:
                ChildBooleanLiteralExpression(production, child);

                break;

            case TokenId.EXPRESSION_GROUP:
                ChildExpressionGroup(production, child);

                break;
        }
    }

    public virtual void EnterAdd(Token token) { }

    public virtual Token ExitAdd(Token token) => token;

    public virtual void EnterSub(Token token) { }

    public virtual Token ExitSub(Token token) => token;

    public virtual void EnterMul(Token token) { }

    public virtual Token ExitMul(Token token) => token;

    public virtual void EnterDiv(Token token) { }

    public virtual Token ExitDiv(Token token) => token;

    public virtual void EnterPower(Token token) { }

    public virtual Token ExitPower(Token token) => token;

    public virtual void EnterMod(Token token) { }

    public virtual Token ExitMod(Token token) => token;

    public virtual void EnterLeftParen(Token token) { }

    public virtual Token ExitLeftParen(Token token) => token;

    public virtual void EnterRightParen(Token token) { }

    public virtual Token ExitRightParen(Token token) => token;

    public virtual void EnterLeftBrace(Token token) { }

    public virtual Token ExitLeftBrace(Token token) => token;

    public virtual void EnterRightBrace(Token token) { }

    public virtual Token ExitRightBrace(Token token) => token;

    public virtual void EnterEq(Token token) { }

    public virtual Token ExitEq(Token token) => token;

    public virtual void EnterLt(Token token) { }

    public virtual Token ExitLt(Token token) => token;

    public virtual void EnterGt(Token token) { }

    public virtual Token ExitGt(Token token) => token;

    public virtual void EnterLte(Token token) { }

    public virtual Token ExitLte(Token token) => token;

    public virtual void EnterGte(Token token) { }

    public virtual Token ExitGte(Token token) => token;

    public virtual void EnterNe(Token token) { }

    public virtual Token ExitNe(Token token) => token;

    public virtual void EnterAnd(Token token) { }

    public virtual Token ExitAnd(Token token) => token;

    public virtual void EnterOr(Token token) { }

    public virtual Token ExitOr(Token token) => token;

    public virtual void EnterXor(Token token) { }

    public virtual Token ExitXor(Token token) => token;

    public virtual void EnterNot(Token token) { }

    public virtual Token ExitNot(Token token) => token;

    public virtual void EnterIn(Token token) { }

    public virtual Token ExitIn(Token token) => token;

    public virtual void EnterDot(Token token) { }

    public virtual Token ExitDot(Token token) => token;

    public virtual void EnterArgumentSeparator(Token token) { }

    public virtual Token ExitArgumentSeparator(Token token) => token;

    public virtual void EnterArrayBraces(Token token) { }

    public virtual Token ExitArrayBraces(Token token) => token;

    public virtual void EnterLeftShift(Token token) { }

    public virtual Token ExitLeftShift(Token token) => token;

    public virtual void EnterRightShift(Token token) { }

    public virtual Token ExitRightShift(Token token) => token;

    public virtual void EnterInteger(Token token) { }

    public virtual Token ExitInteger(Token token) => token;

    public virtual void EnterReal(Token token) { }

    public virtual Token ExitReal(Token token) => token;

    public virtual void EnterStringLiteral(Token token) { }

    public virtual Token ExitStringLiteral(Token token) => token;

    public virtual void EnterCharLiteral(Token token) { }

    public virtual Token ExitCharLiteral(Token token) => token;

    public virtual void EnterTrue(Token token) { }

    public virtual Token ExitTrue(Token token) => token;

    public virtual void EnterFalse(Token token) { }

    public virtual Token ExitFalse(Token token) => token;

    public virtual void EnterIdentifier(Token token) { }

    public virtual Token ExitIdentifier(Token token) => token;

    public virtual void EnterHexLiteral(Token token) { }

    public virtual Token ExitHexliteral(Token token) => token;

    public virtual void EnterNullLiteral(Token token) { }

    public virtual Token ExitNullLiteral(Token token) => token;

    public virtual void EnterTimespan(Token token) { }

    public virtual Token ExitTimeSpan(Token token) => token;

    public virtual void EnterDatetime(Token token) { }

    public virtual Token ExitDatetime(Token token) => token;

    public virtual void EnterIf(Token token) { }

    public virtual Token ExitIf(Token token) => token;

    public virtual void EnterCast(Token token) { }

    public virtual Token ExitCast(Token token) => token;

    public virtual void EnterExpression(Production production) { }

    public virtual Production ExitExpression(Production production) => production;

    public virtual void ChildExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterXorExpression(Production production) { }

    public virtual Production ExitXorExpression(Production production) => production;

    public virtual void ChildXorExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterOrExpression(Production production) { }

    public virtual Production ExitOrExpression(Production production) => production;

    public virtual void ChildOrExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterAndExpression(Production production) { }

    public virtual Production ExitAndExpression(Production production) => production;

    public virtual void ChildAndExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterNotExpression(Production production) { }

    public virtual Production ExitNotExpression(Production production) => production;

    public virtual void ChildNotExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterInExpression(Production production) { }

    public virtual Production ExitInExpression(Production production) => production;

    public virtual void ChildInExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterInTargetExpression(Production production) { }

    public virtual Production ExitInTargetExpression(Production production) => production;

    public virtual void ChildInTargetExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterInListTargetExpression(Production production) { }

    public virtual Production ExitInListTargetExpression(Production production) => production;

    public virtual void ChildInListTargetExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterCompareExpression(Production production) { }

    public virtual Production ExitCompareExpression(Production production) => production;

    public virtual void ChildCompareExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterShiftExpression(Production production) { }

    public virtual Production ExitShiftExpression(Production production) => production;

    public virtual void ChildShiftExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterAdditiveExpression(Production production) { }

    public virtual Production ExitAdditiveExpression(Production production) => production;

    public virtual void ChildAdditiveExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterMultiplicativeExpression(Production production) { }

    public virtual Production ExitMultiplicativeExpression(Production production) => production;

    public virtual void ChildMultiplicativeExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterPowerExpression(Production production) { }

    public virtual Production ExitPowerExpression(Production production) => production;

    public virtual void ChildPowerExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterNegateExpression(Production production) { }

    public virtual Production ExitNegateExpression(Production production) => production;

    public virtual void ChildNegateExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterMemberExpression(Production production) { }

    public virtual Production ExitMemberExpression(Production production) => production;

    public virtual void ChildMemberExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterMemberAccessExpression(Production production) { }

    public virtual Production ExitMemberAccessExpression(Production production) => production;

    public virtual void ChildMemberAccessExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterBasicExpression(Production production) { }

    public virtual Production ExitBasicExpression(Production production) => production;

    public virtual void ChildBasicExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterMemberFunctionExpression(Production production) { }

    public virtual Production ExitMemberFunctionExpression(Production production) => production;

    public virtual void ChildMemberFunctionExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterFieldPropertyExpression(Production production) { }

    public virtual Production ExitFieldPropertyExpression(Production production) => production;

    public virtual void ChildFieldPropertyExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterSpecialFunctionExpression(Production production) { }

    public virtual Production ExitSpecialFunctionExpression(Production production) => production;

    public virtual void ChildSpecialFunctionExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterIfExpression(Production production) { }

    public virtual Production ExitIfExpression(Production production) => production;

    public virtual void ChildIfExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterCastExpression(Production production) { }

    public virtual Production ExitCastExpression(Production production) => production;

    public virtual void ChildCastExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterCastTypeExpression(Production production) { }

    public virtual Production ExitCastTypeExpression(Production production) => production;

    public virtual void ChildCastTypeExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterIndexExpression(Production production) { }

    public virtual Production ExitIndexExpression(Production production) => production;

    public virtual void ChildIndexExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterFunctionCallExpression(Production production) { }

    public virtual Production ExitFunctionCallExpression(Production production) => production;

    public virtual void ChildFunctionCallExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterArgumentList(Production production) { }

    public virtual Production ExitArgumentList(Production production) => production;

    public virtual void ChildArgumentList(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterLiteralExpression(Production production) { }

    public virtual Production ExitLiteralExpression(Production production) => production;

    public virtual void ChildLiteralExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterBooleanLiteralExpression(Production production) { }

    public virtual Production ExitBooleanLiteralExpression(Production production) => production;

    public virtual void ChildBooleanLiteralExpression(Production production, Node child) =>
        production.AddChild(child);

    public virtual void EnterExpressionGroup(Production production) { }

    public virtual Production ExitExpressionGroup(Production production) => production;

    public virtual void ChildExpressionGroup(Production production, Node child) =>
        production.AddChild(child);
}
