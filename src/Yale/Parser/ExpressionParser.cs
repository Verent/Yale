using System.IO;
using PerCederberg.Grammatica.Runtime;

namespace Yale.Parser;

/// <summary>
/// A token stream parser. Defines all productions equal to the grammar defined in
/// the Expression.grammer file.
/// </summary>
public class ExpressionParser : RecursiveDescentParser
{
    private enum SyntheticPatterns
    {
        SUBPRODUCTION_1 = 3001,
        SUBPRODUCTION_2 = 3002,
        SUBPRODUCTION_3 = 3003,
        SUBPRODUCTION_4 = 3004,
        SUBPRODUCTION_5 = 3005,
        SUBPRODUCTION_6 = 3006,
        SUBPRODUCTION_7 = 3007,
        SUBPRODUCTION_8 = 3008,
        SUBPRODUCTION_9 = 3009,
        SUBPRODUCTION_10 = 3010,
        SUBPRODUCTION_11 = 3011,
        SUBPRODUCTION_12 = 3012,
        SUBPRODUCTION_13 = 3013,
        SUBPRODUCTION_14 = 3014,
        SUBPRODUCTION_15 = 3015,
        SUBPRODUCTION_16 = 3016
    }

    public ExpressionParser(TextReader input)
        : base(new ExpressionTokenizer(input))
    {
        CreatePatterns();
    }

    public ExpressionParser(TextReader input, Analyzer analyzer)
        : base(new ExpressionTokenizer(input), analyzer)
    {
        CreatePatterns();
    }

    private void CreatePatterns()
    {
        ProductionPattern pattern = new ProductionPattern((int)Token.EXPRESSION, "Expression");
        ProductionPatternAlternative alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.XOR_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.XOR_EXPRESSION, "XorExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.OR_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_1, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.OR_EXPRESSION, "OrExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.AND_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_2, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.AND_EXPRESSION, "AndExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.NOT_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_3, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.NOT_EXPRESSION, "NotExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.NOT, 0, 1);
        alt.AddProduction((int)Token.IN_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.IN_EXPRESSION, "InExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.COMPARE_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_4, 0, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.IN_TARGET_EXPRESSION, "InTargetExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.FIELD_PROPERTY_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.IN_LIST_TARGET_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            (int)Token.IN_LIST_TARGET_EXPRESSION,
            "InListTargetExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.LEFT_PAREN, 1, 1);
        alt.AddProduction((int)Token.ARGUMENT_LIST, 1, 1);
        alt.AddToken((int)Token.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.COMPARE_EXPRESSION, "CompareExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.SHIFT_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_6, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.SHIFT_EXPRESSION, "ShiftExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.ADDITIVE_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_8, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.ADDITIVE_EXPRESSION, "AdditiveExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.MULTIPLICATIVE_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_10, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            (int)Token.MULTIPLICATIVE_EXPRESSION,
            "MultiplicativeExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.POWER_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_12, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.POWER_EXPRESSION, "PowerExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.NEGATE_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_13, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.NEGATE_EXPRESSION, "NegateExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.SUB, 0, 1);
        alt.AddProduction((int)Token.MEMBER_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.MEMBER_EXPRESSION, "MemberExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.BASIC_EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_14, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            (int)Token.MEMBER_ACCESS_EXPRESSION,
            "MemberAccessExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.DOT, 1, 1);
        alt.AddProduction((int)Token.MEMBER_FUNCTION_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.BASIC_EXPRESSION, "BasicExpression");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.LITERAL_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.EXPRESSION_GROUP, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.MEMBER_FUNCTION_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.SPECIAL_FUNCTION_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            (int)Token.MEMBER_FUNCTION_EXPRESSION,
            "MemberFunctionExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.FIELD_PROPERTY_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.FUNCTION_CALL_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            (int)Token.FIELD_PROPERTY_EXPRESSION,
            "FieldPropertyExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.IDENTIFIER, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            (int)Token.SPECIAL_FUNCTION_EXPRESSION,
            "SpecialFunctionExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.IF_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.CAST_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.IF_EXPRESSION, "IfExpression");
        //if(expr;expr;expr)
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.IF, 1, 1);
        alt.AddToken((int)Token.LEFT_PAREN, 1, 1);
        alt.AddProduction((int)Token.EXPRESSION, 1, 1);
        alt.AddToken((int)Token.ARGUMENT_SEPARATOR, 1, 1);
        alt.AddProduction((int)Token.EXPRESSION, 1, 1);
        alt.AddToken((int)Token.ARGUMENT_SEPARATOR, 1, 1);
        alt.AddProduction((int)Token.EXPRESSION, 1, 1);
        alt.AddToken((int)Token.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.CAST_EXPRESSION, "CastExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.CAST, 1, 1);
        alt.AddToken((int)Token.LEFT_PAREN, 1, 1);
        alt.AddProduction((int)Token.EXPRESSION, 1, 1);
        alt.AddToken((int)Token.ARGUMENT_SEPARATOR, 1, 1);
        alt.AddProduction((int)Token.CAST_TYPE_EXPRESSION, 1, 1);
        alt.AddToken((int)Token.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.CAST_TYPE_EXPRESSION, "CastTypeExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.IDENTIFIER, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_15, 0, -1);
        alt.AddToken((int)Token.ARRAY_BRACES, 0, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.INDEX_EXPRESSION, "IndexExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.LEFT_BRACE, 1, 1);
        alt.AddProduction((int)Token.ARGUMENT_LIST, 1, 1);
        alt.AddToken((int)Token.RIGHT_BRACE, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            (int)Token.FUNCTION_CALL_EXPRESSION,
            "FunctionCallExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.IDENTIFIER, 1, 1);
        alt.AddToken((int)Token.LEFT_PAREN, 1, 1);
        alt.AddProduction((int)Token.ARGUMENT_LIST, 0, 1);
        alt.AddToken((int)Token.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.ARGUMENT_LIST, "ArgumentList");
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.EXPRESSION, 1, 1);
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_16, 0, -1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.LITERAL_EXPRESSION, "LiteralExpression");
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.INTEGER, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.REAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.STRING_LITERAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.BOOLEAN_LITERAL_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.HEX_LITERAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.CHAR_LITERAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.NULL_LITERAL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.DATETIME, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.TIMESPAN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern(
            (int)Token.BOOLEAN_LITERAL_EXPRESSION,
            "BooleanLiteralExpression"
        );
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.TRUE, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.FALSE, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)Token.EXPRESSION_GROUP, "ExpressionGroup");
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.LEFT_PAREN, 1, 1);
        alt.AddProduction((int)Token.EXPRESSION, 1, 1);
        alt.AddToken((int)Token.RIGHT_PAREN, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_1, "Subproduction1")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.XOR, 1, 1);
        alt.AddProduction((int)Token.OR_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_2, "Subproduction2")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.OR, 1, 1);
        alt.AddProduction((int)Token.AND_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_3, "Subproduction3")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.AND, 1, 1);
        alt.AddProduction((int)Token.NOT_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_4, "Subproduction4")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.IN, 1, 1);
        alt.AddProduction((int)Token.IN_TARGET_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_5, "Subproduction5")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.EQ, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.GT, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.LT, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.GTE, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.LTE, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.NE, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_6, "Subproduction6")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_5, 1, 1);
        alt.AddProduction((int)Token.SHIFT_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_7, "Subproduction7")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.LEFT_SHIFT, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.RIGHT_SHIFT, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_8, "Subproduction8")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_7, 1, 1);
        alt.AddProduction((int)Token.ADDITIVE_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_9, "Subproduction9")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.ADD, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.SUB, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_10, "Subproduction10")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_9, 1, 1);
        alt.AddProduction((int)Token.MULTIPLICATIVE_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_11, "Subproduction11")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.MUL, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.DIV, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.MOD, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_12, "Subproduction12")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)SyntheticPatterns.SUBPRODUCTION_11, 1, 1);
        alt.AddProduction((int)Token.POWER_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_13, "Subproduction13")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.POWER, 1, 1);
        alt.AddProduction((int)Token.NEGATE_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_14, "Subproduction14")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.MEMBER_ACCESS_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        alt = new ProductionPatternAlternative();
        alt.AddProduction((int)Token.INDEX_EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_15, "Subproduction15")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.DOT, 1, 1);
        alt.AddToken((int)Token.IDENTIFIER, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);

        pattern = new ProductionPattern((int)SyntheticPatterns.SUBPRODUCTION_16, "Subproduction16")
        {
            Synthetic = true
        };
        alt = new ProductionPatternAlternative();
        alt.AddToken((int)Token.ARGUMENT_SEPARATOR, 1, 1);
        alt.AddProduction((int)Token.EXPRESSION, 1, 1);
        pattern.AddAlternative(alt);
        AddPattern(pattern);
    }
}
