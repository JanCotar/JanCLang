namespace JanCLang.Misc
{
    // Tokens are created by Lexer and used by Parser.
    internal class Token
    {
        internal TokenType Type;
        internal string Value;
        internal int SourceCodeLine;
        internal int OperatorPrecedence;

        internal Token(TokenType type, string value, int sourceCodeLine, int operatorPrecedence)
        {
            Type = type;
            Value = value;
            SourceCodeLine = sourceCodeLine;
            OperatorPrecedence = operatorPrecedence;
        }

        // Create End of file token
        internal Token(int sourceCodeLine)
        {
            Type = TokenType.EOF;
            Value = "End Of File";
            SourceCodeLine = sourceCodeLine;
        }
    }
}
