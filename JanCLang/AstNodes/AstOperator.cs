using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents any of the four basic arithmetic operators (+, -, *, /).
    internal class AstOperator : Ast
    {
        internal Operator Operator;

        internal AstOperator(Token token)
        {
            AstType = AstType.Operator;
            SourceCodeLine = token.SourceCodeLine;

            switch (token.Value)
            {
                case "+":
                    Operator = Operator.Plus;
                    break;
                case "-":
                    Operator = Operator.Minus;
                    break;
                case "*":
                    Operator = Operator.Multiplication;
                    break;
                case "/":
                    Operator = Operator.Division;
                    break;
            }
        }
    }
}
