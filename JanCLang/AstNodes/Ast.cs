using JanCLang.Misc;

namespace JanCLang.AstNodes
{
    // All Ast classes inherit this class.
    internal abstract class Ast
    {
        internal AstType AstType;
        internal int SourceCodeLine;
    }
}
