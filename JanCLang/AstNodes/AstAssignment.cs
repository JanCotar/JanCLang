using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // Ast node. Represents an assignment of a value returned by expression to a variable.
    internal class AstAssignment : Ast
    {
        internal AstVariable Variable;
        internal AstExpression Expression;

        internal AstAssignment()
        {
            AstType = AstType.Assignment;
        }
    }
}
