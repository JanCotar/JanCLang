using JanCLang.Misc;
using System.Collections.Generic;

namespace JanCLang.AstNodes
{
    // Ast node. Represents a block of JanC statements. Also a root node of the whole JanC program.
    internal class AstBlock : Ast
    {
        internal List<Ast> Statements = new List<Ast>();

        internal AstBlock()
        {
            AstType = AstType.Block;
        }
    }
}
