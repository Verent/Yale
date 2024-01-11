using System.IO;
using static Yale.Parser.TokenPattern;

namespace Yale.Parser;

/// <summary>
/// A character stream tokenizer. Defines all tokens equal to the tokens defined in
/// the Expression.grammer file.
/// </summary>
internal sealed class ExpressionTokenizer : Tokenizer
{
    public ExpressionTokenizer(TextReader input)
        : base(input, true)
    {
        CreatePatterns();
    }

    private void CreatePatterns()
    {
        TokenPattern pattern = new((int)TokenId.ADD, "ADD", PatternType.STRING, "+");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.SUB, "SUB", PatternType.STRING, "-");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.MUL, "MUL", PatternType.STRING, "*");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.DIV, "DIV", PatternType.STRING, "/");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.POWER, "POWER", PatternType.STRING, "^");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.MOD, "MOD", PatternType.STRING, "%");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.LEFT_PAREN, "LEFT_PAREN", PatternType.STRING, "(");
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.RIGHT_PAREN,
            "RIGHT_PAREN",
            PatternType.STRING,
            ")"
        );
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.LEFT_BRACE, "LEFT_BRACE", PatternType.STRING, "[");
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.RIGHT_BRACE,
            "RIGHT_BRACE",
            PatternType.STRING,
            "]"
        );
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.EQ, "EQ", PatternType.STRING, "=");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.EQ, "EQ", PatternType.STRING, "EQ");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.LT, "LT", PatternType.STRING, "<");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.GT, "GT", PatternType.STRING, ">");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.LTE, "LTE", PatternType.STRING, "<=");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.GTE, "GTE", PatternType.STRING, ">=");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.NE, "NE", PatternType.STRING, "<>");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.AND, "AND", PatternType.STRING, "AND");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.OR, "OR", PatternType.STRING, "OR");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.XOR, "XOR", PatternType.STRING, "XOR");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.NOT, "NOT", PatternType.STRING, "NOT");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.IN, "IN", PatternType.STRING, "in");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.DOT, "DOT", PatternType.STRING, ".");
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.ARGUMENT_SEPARATOR,
            "ARGUMENT_SEPARATOR",
            PatternType.STRING,
            ";"
        );
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.ARRAY_BRACES,
            "ARRAY_BRACES",
            PatternType.STRING,
            "[]"
        );
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.LEFT_SHIFT, "LEFT_SHIFT", PatternType.STRING, "<<");
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.RIGHT_SHIFT,
            "RIGHT_SHIFT",
            PatternType.STRING,
            ">>"
        );
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.WHITESPACE,
            "WHITESPACE",
            PatternType.REGEXP,
            "\\s+"
        )
        {
            Ignore = true
        };
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.INTEGER,
            "INTEGER",
            PatternType.REGEXP,
            "\\d+(u|l|ul|lu|f|m)?"
        );
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.REAL,
            "REAL",
            PatternType.REGEXP,
            "\\d*\\.\\d+([e][+-]\\d{1,3})?(d|f|m)?"
        );
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.STRING_LITERAL,
            "STRING_LITERAL",
            PatternType.REGEXP,
            "\"([^\"\\r\\n\\\\]|\\\\u[0-9a-f]{4}|\\\\[\\\\\"'trn])*\""
        );
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.CHAR_LITERAL,
            "CHAR_LITERAL",
            PatternType.REGEXP,
            "'([^'\\r\\n\\\\]|\\\\u[0-9a-f]{4}|\\\\[\\\\\"'trn])'"
        );
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.TRUE, "TRUE", PatternType.STRING, "True");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.FALSE, "FALSE", PatternType.STRING, "False");
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.IDENTIFIER,
            "IDENTIFIER",
            PatternType.REGEXP,
            "[a-z_]\\w*"
        );
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.HEX_LITERAL,
            "HEX_LITERAL",
            PatternType.REGEXP,
            "0x[0-9a-f]+(u|l|ul|lu)?"
        );
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.NULL_LITERAL,
            "NULL_LITERAL",
            PatternType.STRING,
            "null"
        );
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.TIMESPAN,
            "TIMESPAN",
            PatternType.REGEXP,
            "##(\\d+\\.)?\\d{2}:\\d{2}(:\\d{2}(\\.\\d{1,7})?)?#"
        );
        AddPattern(pattern);

        pattern = new TokenPattern(
            (int)TokenId.DATETIME,
            "DATETIME",
            PatternType.REGEXP,
            "#[^#]+#"
        );
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.IF, "IF", PatternType.STRING, "if");
        AddPattern(pattern);

        pattern = new TokenPattern((int)TokenId.CAST, "CAST", PatternType.STRING, "cast");
        AddPattern(pattern);
    }
}
