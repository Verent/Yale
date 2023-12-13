using System.IO;
using PerCederberg.Grammatica.Runtime;
using static PerCederberg.Grammatica.Runtime.TokenPattern;

namespace Yale.Parser
{
    /// <summary>
    /// A character stream tokenizer. Defines all tokens equal to the tokens defined in
    /// the Expression.grammer file.
    /// </summary>
    internal class ExpressionTokenizer : Tokenizer
    {
        public ExpressionTokenizer(TextReader input)
            : base(input, true)
        {
            CreatePatterns();
        }

        private void CreatePatterns()
        {
            TokenPattern pattern = new TokenPattern((int)Token.ADD, "ADD", PatternType.STRING, "+");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.SUB, "SUB", PatternType.STRING, "-");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.MUL, "MUL", PatternType.STRING, "*");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.DIV, "DIV", PatternType.STRING, "/");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.POWER, "POWER", PatternType.STRING, "^");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.MOD, "MOD", PatternType.STRING, "%");
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.LEFT_PAREN,
                "LEFT_PAREN",
                PatternType.STRING,
                "("
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.RIGHT_PAREN,
                "RIGHT_PAREN",
                PatternType.STRING,
                ")"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.LEFT_BRACE,
                "LEFT_BRACE",
                PatternType.STRING,
                "["
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.RIGHT_BRACE,
                "RIGHT_BRACE",
                PatternType.STRING,
                "]"
            );
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.EQ, "EQ", PatternType.STRING, "=");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.EQ, "EQ", PatternType.STRING, "EQ");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.LT, "LT", PatternType.STRING, "<");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.GT, "GT", PatternType.STRING, ">");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.LTE, "LTE", PatternType.STRING, "<=");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.GTE, "GTE", PatternType.STRING, ">=");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.NE, "NE", PatternType.STRING, "<>");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.AND, "AND", PatternType.STRING, "AND");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.OR, "OR", PatternType.STRING, "OR");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.XOR, "XOR", PatternType.STRING, "XOR");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.NOT, "NOT", PatternType.STRING, "NOT");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.IN, "IN", PatternType.STRING, "in");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.DOT, "DOT", PatternType.STRING, ".");
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.ARGUMENT_SEPARATOR,
                "ARGUMENT_SEPARATOR",
                PatternType.STRING,
                ";"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.ARRAY_BRACES,
                "ARRAY_BRACES",
                PatternType.STRING,
                "[]"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.LEFT_SHIFT,
                "LEFT_SHIFT",
                PatternType.STRING,
                "<<"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.RIGHT_SHIFT,
                "RIGHT_SHIFT",
                PatternType.STRING,
                ">>"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.WHITESPACE,
                "WHITESPACE",
                PatternType.REGEXP,
                "\\s+"
            )
            {
                Ignore = true
            };
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.INTEGER,
                "INTEGER",
                PatternType.REGEXP,
                "\\d+(u|l|ul|lu|f|m)?"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.REAL,
                "REAL",
                PatternType.REGEXP,
                "\\d*\\.\\d+([e][+-]\\d{1,3})?(d|f|m)?"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.STRING_LITERAL,
                "STRING_LITERAL",
                PatternType.REGEXP,
                "\"([^\"\\r\\n\\\\]|\\\\u[0-9a-f]{4}|\\\\[\\\\\"'trn])*\""
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.CHAR_LITERAL,
                "CHAR_LITERAL",
                PatternType.REGEXP,
                "'([^'\\r\\n\\\\]|\\\\u[0-9a-f]{4}|\\\\[\\\\\"'trn])'"
            );
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.TRUE, "TRUE", PatternType.STRING, "True");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.FALSE, "FALSE", PatternType.STRING, "False");
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.IDENTIFIER,
                "IDENTIFIER",
                PatternType.REGEXP,
                "[a-z_]\\w*"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.HEX_LITERAL,
                "HEX_LITERAL",
                PatternType.REGEXP,
                "0x[0-9a-f]+(u|l|ul|lu)?"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.NULL_LITERAL,
                "NULL_LITERAL",
                PatternType.STRING,
                "null"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.TIMESPAN,
                "TIMESPAN",
                PatternType.REGEXP,
                "##(\\d+\\.)?\\d{2}:\\d{2}(:\\d{2}(\\.\\d{1,7})?)?#"
            );
            AddPattern(pattern);

            pattern = new TokenPattern(
                (int)Token.DATETIME,
                "DATETIME",
                PatternType.REGEXP,
                "#[^#]+#"
            );
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.IF, "IF", PatternType.STRING, "if");
            AddPattern(pattern);

            pattern = new TokenPattern((int)Token.CAST, "CAST", PatternType.STRING, "cast");
            AddPattern(pattern);
        }
    }
}
