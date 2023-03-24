using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents a while loop.
    internal class AstWhile : Ast
    {
        internal AstComparison Condition = new AstComparison();
        internal AstBlock Block;

        internal AstWhile()
        {
            AstType = AstType.While;
        }
    }
}
