using System.IO;

namespace Yale.Parser;

/// <summary>
/// A token stream parser. Defines all productions equal to the grammar defined in
/// the Expression Grammar file.
/// </summary>
internal sealed class ExpressionParser : RecursiveDescentParser
{
    //private enum SyntheticPatterns
    //{
    //    SUBPRODUCTION_1 = 3001,
    //    SUBPRODUCTION_2 = 3002,
    //    SUBPRODUCTION_3 = 3003,
    //    SUBPRODUCTION_4 = 3004,
    //    SUBPRODUCTION_5 = 3005,
    //    SUBPRODUCTION_6 = 3006,
    //    SUBPRODUCTION_7 = 3007,
    //    SUBPRODUCTION_8 = 3008,
    //    SUBPRODUCTION_9 = 3009,
    //    SUBPRODUCTION_10 = 3010,
    //    SUBPRODUCTION_11 = 3011,
    //    SUBPRODUCTION_12 = 3012,
    //    SUBPRODUCTION_13 = 3013,
    //    SUBPRODUCTION_14 = 3014,
    //    SUBPRODUCTION_15 = 3015,
    //    SUBPRODUCTION_16 = 3016
    //}

    public ExpressionParser(TextReader input, Analyzer analyzer)
        : base(new ExpressionTokenizer(input), analyzer) => CreatePatterns();

    private void CreatePatterns()
    {
        ProductionPattern pattern = new(TokenId.EXPRESSION, "Expression");
        ProductionPatternAlternative alt = new();
        alt.AddProduction(TokenId.XOR_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.XOR_EXPRESSION, "XorExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.OR_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_1, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.OR_EXPRESSION, "OrExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.AND_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_2, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.AND_EXPRESSION, "AndExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.NOT_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_3, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.NOT_EXPRESSION, "NotExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.NOT, 0, 1);
        alt.AddProduction(TokenId.IN_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.IN_EXPRESSION, "InExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.COMPARE_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_4, 0, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.IN_TARGET_EXPRESSION, "InTargetExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.FIELD_PROPERTY_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.IN_LIST_TARGET_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            TokenId.IN_LIST_TARGET_EXPRESSION,
            "InListTargetExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.LEFT_PAREN, 1, 1);
        alt.AddProduction(TokenId.ARGUMENT_LIST, 1, 1);
        alt.AddToken(TokenId.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.COMPARE_EXPRESSION, "CompareExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.SHIFT_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_6, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SHIFT_EXPRESSION, "ShiftExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.ADDITIVE_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_8, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.ADDITIVE_EXPRESSION, "AdditiveExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.MULTIPLICATIVE_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_10, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            TokenId.MULTIPLICATIVE_EXPRESSION,
            "MultiplicativeExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.POWER_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_12, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.POWER_EXPRESSION, "PowerExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.NEGATE_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_13, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.NEGATE_EXPRESSION, "NegateExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.SUB, 0, 1);
        alt.AddProduction(TokenId.MEMBER_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.MEMBER_EXPRESSION, "MemberExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.BASIC_EXPRESSION, 1, 1);
        alt.AddProduction(TokenId.SUBPRODUCTION_14, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.MEMBER_ACCESS_EXPRESSION, "MemberAccessExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.DOT, 1, 1);
        alt.AddProduction(TokenId.MEMBER_FUNCTION_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.BASIC_EXPRESSION, "BasicExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.LITERAL_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.EXPRESSION_GROUP, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.MEMBER_FUNCTION_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.SPECIAL_FUNCTION_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            TokenId.MEMBER_FUNCTION_EXPRESSION,
            "MemberFunctionExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.FIELD_PROPERTY_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.FUNCTION_CALL_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            TokenId.FIELD_PROPERTY_EXPRESSION,
            "FieldPropertyExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.IDENTIFIER, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            TokenId.SPECIAL_FUNCTION_EXPRESSION,
            "SpecialFunctionExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.IF_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.CAST_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.IF_EXPRESSION, "IfExpression");
        //if(expr;expr;expr)
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.IF, 1, 1);
        alt.AddToken(TokenId.LEFT_PAREN, 1, 1);
        alt.AddProduction(TokenId.EXPRESSION, 1, 1);
        alt.AddToken(TokenId.ARGUMENT_SEPARATOR, 1, 1);
        alt.AddProduction(TokenId.EXPRESSION, 1, 1);
        alt.AddToken(TokenId.ARGUMENT_SEPARATOR, 1, 1);
        alt.AddProduction(TokenId.EXPRESSION, 1, 1);
        alt.AddToken(TokenId.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.CAST_EXPRESSION, "CastExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.CAST, 1, 1);
        alt.AddToken(TokenId.LEFT_PAREN, 1, 1);
        alt.AddProduction(TokenId.EXPRESSION, 1, 1);
        alt.AddToken(TokenId.ARGUMENT_SEPARATOR, 1, 1);
        alt.AddProduction(TokenId.CAST_TYPE_EXPRESSION, 1, 1);
        alt.AddToken(TokenId.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.CAST_TYPE_EXPRESSION, "CastTypeExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.IDENTIFIER, 1, 1);
        //alt.AddProduction(TokenId.SUBPRODUCTION_15, 0, -1);
        alt.AddToken(TokenId.ARRAY_BRACES, 0, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.INDEX_EXPRESSION, "IndexExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.LEFT_BRACE, 1, 1);
        alt.AddProduction(TokenId.ARGUMENT_LIST, 1, 1);
        alt.AddToken(TokenId.RIGHT_BRACE, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.FUNCTION_CALL_EXPRESSION, "FunctionCallExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.IDENTIFIER, 1, 1);
        alt.AddToken(TokenId.LEFT_PAREN, 1, 1);
        alt.AddProduction(TokenId.ARGUMENT_LIST, 0, 1);
        alt.AddToken(TokenId.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.ARGUMENT_LIST, "ArgumentList");
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.EXPRESSION, 1, 1);
        //alt.AddProduction(TokenId.SUBPRODUCTION_16, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.LITERAL_EXPRESSION, "LiteralExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.INTEGER, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.REAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.STRING_LITERAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.BOOLEAN_LITERAL_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.HEX_LITERAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.CHAR_LITERAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.NULL_LITERAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.DATETIME, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.TIMESPAN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            TokenId.BOOLEAN_LITERAL_EXPRESSION,
            "BooleanLiteralExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.TRUE, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.FALSE, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.EXPRESSION_GROUP, "ExpressionGroup");
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.LEFT_PAREN, 1, 1);
        alt.AddProduction(TokenId.EXPRESSION, 1, 1);
        alt.AddToken(TokenId.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        //Todo: What are they good for?

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_1, "Subproduction1")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.XOR, 1, 1);
        alt.AddProduction(TokenId.OR_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_2, "Subproduction2")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.OR, 1, 1);
        alt.AddProduction(TokenId.AND_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_3, "Subproduction3")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.AND, 1, 1);
        alt.AddProduction(TokenId.NOT_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_4, "Subproduction4")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.IN, 1, 1);
        alt.AddProduction(TokenId.IN_TARGET_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_5, "Subproduction5")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.EQ, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.GT, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.LT, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.GTE, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.LTE, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.NE, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_6, "Subproduction6")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.SUBPRODUCTION_5, 1, 1);
        alt.AddProduction(TokenId.SHIFT_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_7, "Subproduction7")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.LEFT_SHIFT, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.RIGHT_SHIFT, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_8, "Subproduction8")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.SUBPRODUCTION_7, 1, 1);
        alt.AddProduction(TokenId.ADDITIVE_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_9, "Subproduction9")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.ADD, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.SUB, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_10, "Subproduction10")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.SUBPRODUCTION_9, 1, 1);
        alt.AddProduction(TokenId.MULTIPLICATIVE_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_11, "Subproduction11")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.MUL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.DIV, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.MOD, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_12, "Subproduction12")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.SUBPRODUCTION_11, 1, 1);
        alt.AddProduction(TokenId.POWER_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_13, "Subproduction13")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.POWER, 1, 1);
        alt.AddProduction(TokenId.NEGATE_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_14, "Subproduction14")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.MEMBER_ACCESS_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction(TokenId.INDEX_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_15, "Subproduction15")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.DOT, 1, 1);
        alt.AddToken(TokenId.IDENTIFIER, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(TokenId.SUBPRODUCTION_16, "Subproduction16")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken(TokenId.ARGUMENT_SEPARATOR, 1, 1);
        alt.AddProduction(TokenId.EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);
    }
}
