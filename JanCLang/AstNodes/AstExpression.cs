using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents an algebraic expression.
    internal class AstExpression : Ast
    {
        internal AstOperator Operator;
        internal Ast Left;
        internal Ast Right;

        internal AstExpression(int sourceCodeLine)
        {
            AstType = AstType.Expression;
            SourceCodeLine = sourceCodeLine;
        }
    }
}
