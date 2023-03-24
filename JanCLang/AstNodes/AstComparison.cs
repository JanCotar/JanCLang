using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents a comparison of left and right element.
    internal class AstComparison : Ast
    {
        internal Comparator Comparator;
        internal Ast Left;
        internal Ast Right;

        internal AstComparison()
        {
            AstType = AstType.Comparison;
        }
    }
}
