using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents if-else statement.
    internal class AstConditional : Ast
    {
        internal AstComparison Condition = new AstComparison();
        internal AstBlock IfBlock;
        internal AstBlock ElseBlock;

        internal AstConditional()
        {
            AstType = AstType.Conditional;
        }
    }
}
